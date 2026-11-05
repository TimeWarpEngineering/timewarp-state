#region Purpose
// Restore, build, publish, client-settings, and browser-install steps for the end-to-end runfile.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. Amuru 3 returns a non-zero exit instead of throwing, so each
// required step passes its exit code to EnsureSuccess, which throws E2eStepFailedException. Run catches that and
// sets Environment.ExitCode, so ScriptContext still disposes. The Playwright install is best effort and only warns.
#endregion

/// <summary>A required e2e step failed. Run reports the message and exits 1.</summary>
sealed class E2eStepFailedException(string message) : Exception(message);

static partial class App
{
  static readonly Regex UseHttpSetting = new("\"UseHttp\"\\s*:\\s*(?:true|false)", RegexOptions.CultureInvariant);

  static void EnsureSuccess(int exitCode, string description)
  {
    if (exitCode != 0)
    {
      throw new E2eStepFailedException($"{description} failed with exit code {exitCode}.");
    }
  }

  static async Task RestoreToolsAndCleanup(string outputPath)
  {
    EnsureSuccess(await DotNet.Tool().Restore().Build().RunAsync(), "dotnet tool restore");
    EnsureSuccess(await DotNet.Clean().Build().RunAsync(), "dotnet clean");

    if (Directory.Exists(outputPath))
    {
      Directory.Delete(outputPath, recursive: true);
      WriteLine($"Deleted: {outputPath}");
    }
  }

  static async Task BuildReleaseProject(string projectPath)
  {
    int exitCode = await DotNet.Build()
      .WithProject(projectPath)
      .WithConfiguration("Release")
      .Build()
      .RunAsync();
    EnsureSuccess(exitCode, $"dotnet build {projectPath}");
  }

  static void UpdateClientAppSettings(string appSettingsPath, bool useHttp)
  {
    if (!File.Exists(appSettingsPath))
    {
      throw new E2eStepFailedException($"Client appsettings not found at {appSettingsPath}.");
    }

    // Plain text edit: reflection-based System.Text.Json is disabled for this runfile.
    string json = File.ReadAllText(appSettingsPath);
    if (!UseHttpSetting.IsMatch(json))
    {
      throw new E2eStepFailedException($"{appSettingsPath} has no \"UseHttp\": true|false setting to update.");
    }

    string value = useHttp ? "true" : "false";
    string updatedJson = UseHttpSetting.Replace(json, $"\"UseHttp\": {value}");
    if (updatedJson != json)
    {
      File.WriteAllText(appSettingsPath, updatedJson);
    }

    WriteLine($"UseHttp is {value} in {appSettingsPath}");
  }

  static async Task BuildAndPublishSut(string sutProjectPath, string outputPath)
  {
    EnsureSuccess(await DotNet.Restore().WithProject(sutProjectPath).Build().RunAsync(), "dotnet restore (SUT)");

    int exitCode = await DotNet.Build()
      .WithProject(sutProjectPath)
      .WithConfiguration("Release")
      .WithNoRestore()
      .Build()
      .RunAsync();
    EnsureSuccess(exitCode, "dotnet build (SUT)");

    exitCode = await DotNet.Publish()
      .WithProject(sutProjectPath)
      .WithConfiguration("Release")
      .WithOutput(outputPath)
      .Build()
      .RunAsync();
    EnsureSuccess(exitCode, "dotnet publish (SUT)");
  }

  static async Task BuildTest(string testProjectPath)
  {
    EnsureSuccess(await DotNet.Restore().WithProject(testProjectPath).Build().RunAsync(), "dotnet restore (tests)");

    int exitCode = await DotNet.Build()
      .WithProject(testProjectPath)
      .WithConfiguration("Debug")
      .Build()
      .RunAsync();
    EnsureSuccess(exitCode, "dotnet build (tests)");
  }

  static async Task EnsureBrowsersInstalled(string testProjectPath)
  {
    // Ask MSBuild for the Debug output so the target framework is not hard-coded here.
    CommandOutput targetPath = await Shell.Builder("dotnet")
      .WithArguments("msbuild", testProjectPath, "-getProperty:TargetPath", "-p:Configuration=Debug")
      .CaptureAsync();
    EnsureSuccess(targetPath.ExitCode, "dotnet msbuild -getProperty:TargetPath (tests)");

    string outputDirectory = Path.GetDirectoryName(targetPath.Stdout.Trim()) ?? ".";
    string playwrightPath = Path.Combine(outputDirectory, "playwright.ps1");
    if (!File.Exists(playwrightPath))
    {
      throw new E2eStepFailedException(
        $"Playwright script not found at {playwrightPath}. Make sure the test-app-end-to-end-tests project is built.");
    }

    // Best effort: --with-deps needs passwordless sudo (GitHub runners have it; a dev box may not). A cached
    // browser still lets the tests run, so a failed install only warns.
    WriteLine("Installing Playwright Chromium browser...");
    int exitCode = await Shell.Builder("pwsh")
      .WithArguments(playwrightPath, "install", "chromium", "--with-deps")
      .RunAsync();
    if (exitCode != 0)
    {
      WriteLine($"Warning: Playwright installation may have issues. Exit code: {exitCode}");
    }
  }
}
