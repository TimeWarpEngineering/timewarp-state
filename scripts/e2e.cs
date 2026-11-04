#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true
#:include e2e-build.cs
#:include e2e-sut.cs
#:include e2e-tests.cs
#:include e2e-dev-certs.cs

#region Purpose
// Runfile behind dev e2e: publishes the test app, starts it, and runs the Playwright end-to-end tests.
#endregion

#region Design
// Sets NUGET_PACKAGES to artifacts/nuget-packages, then restores tools and cleans, skips dev-cert trust
// over http, builds the analyzer and source generator, sets UseHttp in the client appsettings, publishes
// the server to tests/test-app/output, builds the tests, installs Chromium, starts the SUT on port 7011,
// waits up to 30 tries, runs dotnet test with chrome.runsettings, then kills the SUT. runMode is fixed
// to Auto. Any test failure exits 1.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .Map("")
    .WithHandler(App.Run)
    .AsCommand()
    .Done()
  .Build();

return await app.RunAsync(args);

static partial class App
{
  // SUT log writers must outlive StartSut: the OutputDataReceived handlers keep writing until the SUT exits.
  static StreamWriter? SutOutputWriter;
  static StreamWriter? SutErrorWriter;

  public static async Task Run()
  {
    using var context = ScriptContext.FromRelativePath("..");
    DevCli.NuGetPackageIsolation.Isolation isolation =
      DevCli.NuGetPackageIsolation.Apply(Directory.GetCurrentDirectory());
    WriteLine($"NUGET_PACKAGES={isolation.IsolatedPackagesFolder}");

    // Configuration variables
    var sutProjectDir = "./tests/test-app/test-app-server";
    var outputPath = "./tests/test-app/output";
    // workflow.yml sets UseHttp; default to http when the variable is unset or unparseable.
    bool useHttp = !bool.TryParse(Environment.GetEnvironmentVariable("UseHttp"), out bool parsedUseHttp) || parsedUseHttp;
    var protocol = useHttp ? "http" : "https";
    var sutUrl = $"{protocol}://localhost";
    var testProjectDir = "./tests/test-app-end-to-end-tests";
    var testProjectPath = $"{testProjectDir}/test-app-end-to-end-tests.csproj";
    var analyzerProjectPath = "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj";
    var sourceGeneratorProjectPath = "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj";
    var sutPort = 7011;
    var maxRetries = 30;
    var retryInterval = 1;
    var runMode = "Auto"; // Possible values: "Auto", "Manual", "Development", "Release"

    await WriteStepHeader("Restore-Tools-And-Cleanup");
    await RestoreToolsAndCleanup();
    await WriteStepFooter("Restore-Tools-And-Cleanup");

    await WriteStepHeader("Install-LinuxDevCerts");
    if (useHttp)
    {
        WriteLine("Skipping dev-certs trust: E2E runs over http (UseHttp=true).");
    }
    else
    {
        await InstallLinuxDevCerts();
    }
    await WriteStepFooter("Install-LinuxDevCerts");

    await WriteStepHeader("Build-Analyzer");
    await BuildAnalyzer(analyzerProjectPath);
    await WriteStepFooter("Build-Analyzer");

    await WriteStepHeader("Build-SourceGenerator");
    await BuildSourceGenerator(sourceGeneratorProjectPath);
    await WriteStepFooter("Build-SourceGenerator");

    await WriteStepHeader("Update-ClientAppSettings");
    await UpdateClientAppSettings(useHttp);
    await WriteStepFooter("Update-ClientAppSettings");

    await WriteStepHeader("Build-And-Publish-Sut");
    await BuildAndPublishSut(sutProjectDir, outputPath);
    await WriteStepFooter("Build-And-Publish-Sut");

    await WriteStepHeader("Build-Test");
    await BuildTest(testProjectDir);
    await WriteStepFooter("Build-Test");

    await WriteStepHeader("Ensure-Browsers-Installed");
    await EnsureBrowsersInstalled(testProjectDir);
    await WriteStepFooter("Ensure-Browsers-Installed");

    await WriteStepHeader("Start-Sut");
    var sutProcess = await StartSut(runMode, outputPath, sutUrl, sutPort, useHttp);
    await WriteStepFooter("Start-Sut");

    var runFailed = false;
    try
    {
        await WaitForSut($"{sutUrl}:{sutPort}", maxRetries, retryInterval);

        var testsFailed = await RunTests(testProjectDir, useHttp);
        runFailed = testsFailed;

        if (runMode == "Auto")
        {
            var outputLogPath = Path.Combine(outputPath, "sut_output.log");
            var errorLogPath = Path.Combine(outputPath, "sut_error.log");

            if (testsFailed)
            {
                WriteLine("Tests failed. Displaying SUT logs:");
                await DisplaySutLogs(outputLogPath, errorLogPath);
            }
            else
            {
                WriteLine("Tests passed. SUT logs available at:");
                WriteLine($"Output log: {outputLogPath}");
                WriteLine($"Error log: {errorLogPath}");
            }
        }

        if (runMode is "Development" or "Release")
        {
            WriteLine($"Tests completed. SUT is still running in {runMode} mode. Press Ctrl+C to stop.");
            // In a real implementation, you'd handle cancellation tokens here
        }
    }
    catch (Exception ex)
    {
        runFailed = true;
        WriteLine($"An error occurred during test execution: {ex.Message}");
        if (runMode == "Auto")
        {
            var outputLogPath = Path.Combine(outputPath, "sut_output.log");
            var errorLogPath = Path.Combine(outputPath, "sut_error.log");
            await DisplaySutLogs(outputLogPath, errorLogPath);
        }
    }
    finally
    {
        if (runMode == "Auto" && sutProcess != null)
        {
            await KillSut(sutProcess);
        }
        else
        {
            WriteLine($"Please remember to stop the SUT process running in {runMode} mode.");
        }
    }

    // A failed or aborted test run must fail the script; the Amuru line in use throws on a non-zero
    // dotnet test exit, which previously landed in the catch above and left the exit code at 0.
    if (runFailed)
    {
        WriteLine("E2E tests failed.");
        Environment.Exit(1);
    }
  }

  static async Task WriteStepHeader(string stepName)
{
    WriteLine($"\n========== Starting: {stepName} ==========");
}

  static async Task WriteStepFooter(string stepName)
{
    WriteLine($"========== Completed: {stepName} ==========\n");
}
}
