#region Purpose
// Restore, build, publish, client-settings, and browser-install steps for the end-to-end runfile.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. Amuru 3 returns a non-zero exit instead of throwing, so each
// required step passes its exit code to EnsureSuccess, which throws E2eStepFailedException. Run catches that and
// sets Environment.ExitCode, so ScriptContext still disposes. The Playwright install is best effort and only warns.
// The browser step reads the Chromium revision from browsers.json that Microsoft.Playwright copies into the test
// output (.playwright/package) and skips the install when chromium-<rev> and chromium_headless_shell-<rev> both have
// INSTALLATION_COMPLETE in the browsers path (PLAYWRIGHT_BROWSERS_PATH, else the per-OS ms-playwright cache).
// A fresh CI runner has no cache, so CI still installs. --with-deps (apt packages, needs root) is added only in CI
// or on request, and is dropped with a warning when not root and `sudo -n true` fails, so sudo never prompts.
// The install gets empty stdin and a timeout, so nothing can wait on a terminal.
#endregion

/// <summary>A required e2e step failed. Run reports the message and exits 1.</summary>
sealed class E2eStepFailedException(string message) : Exception(message);

static partial class App
{
  static readonly Regex UseHttpSetting = new("\"UseHttp\"\\s*:\\s*(?:true|false)", RegexOptions.CultureInvariant);
  static readonly string[] RequiredBrowsers = ["chromium", "chromium-headless-shell"];
  static readonly TimeSpan BrowserInstallTimeout = TimeSpan.FromMinutes(10);
  static readonly TimeSpan SudoCheckTimeout = TimeSpan.FromSeconds(10);

  /// <summary>True on GitHub Actions or any runner that sets CI.</summary>
  static bool IsCi() =>
    IsTrueEnvironmentVariable("GITHUB_ACTIONS") || IsTrueEnvironmentVariable("CI");

  static bool IsTrueEnvironmentVariable(string name)
  {
    string? value = Environment.GetEnvironmentVariable(name);
    return !string.IsNullOrWhiteSpace(value)
      && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
      && value != "0";
  }

  static void EnsureSuccess(int exitCode, string description)
  {
    if (exitCode != 0)
    {
      throw new E2eStepFailedException($"{description} failed with exit code {exitCode}.");
    }
  }

  static async Task RestoreToolsAndCleanup(string outputPath, bool restoreTools)
  {
    if (restoreTools)
    {
      EnsureSuccess(await DotNet.Tool().Restore().Build().RunAsync(), "dotnet tool restore");
    }
    else
    {
      WriteLine("Skipping dotnet tool restore: --skip-setup.");
    }

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

  static async Task EnsureBrowsersInstalled(string testProjectPath, bool withDepsRequested)
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

    string browsersJsonPath = Path.Combine(outputDirectory, ".playwright", "package", "browsers.json");
    string? browsersPath = ResolvePlaywrightBrowsersPath();
    string? revision = ReadChromiumRevision(browsersJsonPath);
    if (revision is null)
    {
      WriteLine($"Could not read the Chromium revision from {browsersJsonPath}; installing.");
    }
    else if (browsersPath is null)
    {
      WriteLine($"Playwright wants Chromium {revision}; the browsers path is not a fixed folder, so installing.");
    }
    else if (AreBrowsersInstalled(browsersPath, revision))
    {
      WriteLine($"Playwright Chromium {revision} found in {browsersPath}; skipping install.");
      return;
    }
    else
    {
      WriteLine($"Playwright Chromium {revision} not found in {browsersPath}; installing.");
    }

    bool ci = IsCi();
    bool withDeps = (ci || withDepsRequested) && await CanInstallSystemDependencies(playwrightPath);

    // Best effort: a cached or partial browser may still let the tests run, so a failed install only warns.
    List<string> arguments = [playwrightPath, "install", "chromium"];
    if (withDeps)
    {
      arguments.Add("--with-deps");
    }

    WriteLine($"Running: pwsh {string.Join(' ', arguments)}");
    int exitCode = await Shell.Builder("pwsh")
      .WithArguments([.. arguments])
      .WithEnvironmentVariable("DEBIAN_FRONTEND", "noninteractive")
      .WithStandardInput(string.Empty)
      .WithTimeout(BrowserInstallTimeout)
      .WithNoValidation()
      .RunAsync();
    if (exitCode == CommandResult.TimeoutExitCode)
    {
      WriteLine($"Warning: Playwright install timed out after {BrowserInstallTimeout.TotalMinutes:0} minutes; continuing.");
    }
    else if (exitCode != 0)
    {
      WriteLine($"Warning: Playwright installation may have issues. Exit code: {exitCode}");
    }
  }

  /// <summary>
  /// The folder Playwright installs browsers into, or null when PLAYWRIGHT_BROWSERS_PATH=0 (browsers live inside
  /// the driver package) or the OS cache folder cannot be found.
  /// </summary>
  static string? ResolvePlaywrightBrowsersPath()
  {
    string? overridePath = Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH");
    if (!string.IsNullOrWhiteSpace(overridePath))
    {
      return overridePath == "0" ? null : Path.GetFullPath(overridePath);
    }

    if (OperatingSystem.IsWindows())
    {
      string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      return string.IsNullOrEmpty(localAppData) ? null : Path.Combine(localAppData, "ms-playwright");
    }

    string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    if (OperatingSystem.IsMacOS())
    {
      return string.IsNullOrEmpty(home) ? null : Path.Combine(home, "Library", "Caches", "ms-playwright");
    }

    string? cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
    if (string.IsNullOrWhiteSpace(cacheHome))
    {
      if (string.IsNullOrEmpty(home))
      {
        return null;
      }

      cacheHome = Path.Combine(home, ".cache");
    }

    return Path.Combine(cacheHome, "ms-playwright");
  }

  /// <summary>The chromium revision in Playwright's browsers.json, or null when the file is missing or unreadable.</summary>
  static string? ReadChromiumRevision(string browsersJsonPath)
  {
    if (!File.Exists(browsersJsonPath))
    {
      return null;
    }

    try
    {
      // JsonDocument is not reflection-based, so it works with reflection serialization disabled.
      using JsonDocument document = JsonDocument.Parse(File.ReadAllText(browsersJsonPath));
      foreach (JsonElement browser in document.RootElement.GetProperty("browsers").EnumerateArray())
      {
        if (browser.GetProperty("name").GetString() == "chromium")
        {
          return browser.GetProperty("revision").GetString();
        }
      }
    }
    catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or IOException)
    {
      WriteLine($"Warning: could not parse {browsersJsonPath}: {ex.Message}");
    }

    return null;
  }

  /// <summary>
  /// True when every required browser folder for this revision has Playwright's INSTALLATION_COMPLETE marker.
  /// Folder names follow Playwright's registry: dashes in the browser name become underscores, then -revision.
  /// </summary>
  static bool AreBrowsersInstalled(string browsersPath, string revision) =>
    RequiredBrowsers.All(name =>
      File.Exists(Path.Combine(browsersPath, $"{name.Replace('-', '_')}-{revision}", "INSTALLATION_COMPLETE")));

  /// <summary>
  /// True when --with-deps can run without a password prompt: not Linux (Playwright handles it), already root, or
  /// passwordless sudo works.
  /// </summary>
  static async Task<bool> CanInstallSystemDependencies(string playwrightPath)
  {
    if (!OperatingSystem.IsLinux() || Environment.UserName == "root")
    {
      return true;
    }

    int sudoExitCode;
    try
    {
      CommandOutput sudoCheck = await Shell.Builder("sudo")
        .WithArguments("-n", "true")
        .WithStandardInput(string.Empty)
        .WithTimeout(SudoCheckTimeout)
        .WithNoValidation()
        .CaptureAsync();
      sudoExitCode = sudoCheck.ExitCode;
    }
    catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
    {
      WriteLine($"Warning: could not run sudo ({ex.Message}); installing without --with-deps.");
      return false;
    }

    if (sudoExitCode != 0)
    {
      WriteLine("Warning: --with-deps needs passwordless sudo, and `sudo -n true` failed; installing without it.");
      WriteLine($"Install the system packages yourself with: sudo pwsh {playwrightPath} install-deps chromium");
      return false;
    }

    return true;
  }
}
