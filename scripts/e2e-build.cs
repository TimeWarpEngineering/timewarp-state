#region Purpose
// Restore, build, publish, and browser-install steps for the end-to-end runfile.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. A sibling .cs is not part of the runfile unless the entry includes it.
#endregion

static partial class App
{
  static async Task EnsureBrowsersInstalled(string testProjectDir)
{
    var playwrightPath = $"{testProjectDir}/bin/Debug/net11.0/playwright.ps1";
    if (File.Exists(playwrightPath))
    {
        WriteLine("Installing Playwright Chromium browser...");
        // Best effort: --with-deps needs passwordless sudo (GitHub runners have it; a dev box may not) and this
        // Amuru line throws on a non-zero exit. A cached browser still lets the tests run, so only warn here.
        try
        {
            var exitCode = await Shell.Builder("pwsh")
                .WithArguments(playwrightPath, "install", "chromium", "--with-deps")
                .RunAsync();
            if (exitCode != 0)
            {
                WriteLine($"Warning: Playwright installation may have issues. Exit code: {exitCode}");
            }
        }
        catch (Exception ex)
        {
            WriteLine($"Warning: Playwright installation may have issues. {ex.Message.Split('\n')[0]}");
        }
    }
    else
    {
        throw new Exception($"Playwright script not found at {playwrightPath}. Make sure the Test.App.EndToEnd.Tests project is built.");
    }
}

  static async Task RestoreToolsAndCleanup()
{
    // Restore .NET tools
    var exitCode = await DotNet.Tool().Restore().Build().RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Clean the solution
    exitCode = await DotNet.Clean().Build().RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Clean the Output directory
    if (Directory.Exists("./tests/test-app/output"))
    {
        Directory.Delete("./tests/test-app/output", true);
        WriteLine("Deleted: ./tests/test-app/output");
    }
}

  static async Task BuildAnalyzer(string analyzerProjectPath)
{
    var exitCode = await DotNet.Build()
        .WithProject(analyzerProjectPath)
        .WithConfiguration("Release")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

  static async Task BuildSourceGenerator(string sourceGeneratorProjectPath)
{
    var exitCode = await DotNet.Build()
        .WithProject(sourceGeneratorProjectPath)
        .WithConfiguration("Release")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

  static async Task UpdateClientAppSettings(bool useHttp)
{
    var appSettingsPath = "./tests/test-app/test-app-client/wwwroot/appsettings.json";
    if (File.Exists(appSettingsPath))
    {
        var json = File.ReadAllText(appSettingsPath);
        // Plain string replacement: reflection-based System.Text.Json is disabled for this runfile.
        var updatedJson = json.Replace("\"UseHttp\": false", "\"UseHttp\": true")
                             .Replace("\"UseHttp\": true", $"\"UseHttp\": {useHttp.ToString().ToLower()}");
        File.WriteAllText(appSettingsPath, updatedJson);
    }
}

  static async Task BuildAndPublishSut(string sutProjectDir, string outputPath)
{
    // Restore dependencies
    var exitCode = await DotNet.Restore()
        .WithProject($"{sutProjectDir}/test-app-server.csproj")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Build the solution
    exitCode = await DotNet.Build()
        .WithProject($"{sutProjectDir}/test-app-server.csproj")
        .WithConfiguration("Release")
        .WithNoRestore()
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Publish the SUT
    exitCode = await DotNet.Publish()
        .WithProject($"{sutProjectDir}/test-app-server.csproj")
        .WithConfiguration("Release")
        .WithOutput(outputPath)
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

  static async Task BuildTest(string testProjectDir)
{
    // Restore dependencies
    var exitCode = await DotNet.Restore()
        .WithProject($"{testProjectDir}/test-app-end-to-end-tests.csproj")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Build the test project
    exitCode = await DotNet.Build()
        .WithProject($"{testProjectDir}/test-app-end-to-end-tests.csproj")
        .WithConfiguration("Debug")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}
}
