#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true
#:include e2e-build.cs
#:include e2e-sut-host.cs
#:include e2e-tests.cs
#:include e2e-dev-certs.cs

#region Purpose
// Runfile behind dev e2e: publishes the test app, starts it, and runs the Playwright end-to-end tests.
#endregion

#region Design
// Run is orchestration only. It sets NUGET_PACKAGES to artifacts/nuget-packages, then restores tools and cleans,
// trusts dev certs only over https, builds the analyzer and source generator, sets UseHttp in the client
// appsettings, publishes the server to tests/test-app/output, builds the tests, and installs Chromium. It then
// starts the SUT on port 7011 through SutHost, waits up to 30 tries, runs dotnet test with chrome.runsettings, and
// stops the SUT. A failed required step (E2eStepFailedException) or a failed test run sets Environment.ExitCode to 1.
// The handler returns normally, so ScriptContext disposes; Environment.Exit is never called.
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
  const string SutProjectPath = "./tests/test-app/test-app-server/test-app-server.csproj";
  const string OutputPath = "./tests/test-app/output";
  const string ClientAppSettingsPath = "./tests/test-app/test-app-client/wwwroot/appsettings.json";
  const string TestProjectDir = "./tests/test-app-end-to-end-tests";
  const string TestProjectPath = TestProjectDir + "/test-app-end-to-end-tests.csproj";
  const string AnalyzerProjectPath = "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj";
  const string SourceGeneratorProjectPath = "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj";
  const int SutPort = 7011;
  const int MaxReadyAttempts = 30;
  static readonly TimeSpan ReadyRetryInterval = TimeSpan.FromSeconds(1);

  public static async Task Run()
  {
    using ScriptContext context = ScriptContext.FromRelativePath("..");
    DevCli.NuGetPackageIsolation.Isolation isolation =
      DevCli.NuGetPackageIsolation.Apply(Directory.GetCurrentDirectory());
    WriteLine($"NUGET_PACKAGES={isolation.IsolatedPackagesFolder}");

    // workflow.yml sets UseHttp; default to http when the variable is unset or unparseable.
    bool useHttp = !bool.TryParse(Environment.GetEnvironmentVariable("UseHttp"), out bool parsedUseHttp) || parsedUseHttp;
    string sutUrl = $"{(useHttp ? "http" : "https")}://localhost:{SutPort}";

    try
    {
      await RunStep("Restore-Tools-And-Cleanup", () => RestoreToolsAndCleanup(OutputPath));
      await RunStep("Install-LinuxDevCerts", async () =>
      {
        if (useHttp)
        {
          WriteLine("Skipping dev-certs trust: E2E runs over http (UseHttp=true).");
          return;
        }

        await InstallLinuxDevCerts();
      });
      await RunStep("Build-Analyzer", () => BuildReleaseProject(AnalyzerProjectPath));
      await RunStep("Build-SourceGenerator", () => BuildReleaseProject(SourceGeneratorProjectPath));
      RunStep("Update-ClientAppSettings", () => UpdateClientAppSettings(ClientAppSettingsPath, useHttp));
      await RunStep("Build-And-Publish-Sut", () => BuildAndPublishSut(SutProjectPath, OutputPath));
      await RunStep("Build-Test", () => BuildTest(TestProjectPath));
      await RunStep("Ensure-Browsers-Installed", () => EnsureBrowsersInstalled(TestProjectPath));

      if (!await RunTestsAgainstSut(sutUrl, useHttp))
      {
        WriteLine("E2E tests failed.");
        Environment.ExitCode = 1;
      }
    }
    catch (E2eStepFailedException ex)
    {
      WriteLine($"E2E run failed: {ex.Message}");
      Environment.ExitCode = 1;
    }
  }

  static async Task<bool> RunTestsAgainstSut(string sutUrl, bool useHttp)
  {
    WriteStepHeader("Start-Sut");
    SutHost sut = SutHost.Start(OutputPath, sutUrl, useHttp);
    WriteStepFooter("Start-Sut");

    bool testsPassed = false;
    try
    {
      await RunStep("Wait-For-Sut", () => sut.WaitUntilReadyAsync(MaxReadyAttempts, ReadyRetryInterval));

      WriteStepHeader("Run-Tests");
      testsPassed = await RunTests(TestProjectDir, useHttp);
      WriteStepFooter("Run-Tests");
    }
    finally
    {
      // DisposeAsync never throws, and the logs are read only after the writers are closed.
      await sut.DisposeAsync();
      if (testsPassed)
      {
        WriteLine("Tests passed. SUT logs available at:");
        WriteLine($"Output log: {sut.OutputLogPath}");
        WriteLine($"Error log: {sut.ErrorLogPath}");
      }
      else
      {
        await sut.DisplayLogsAsync();
      }
    }

    return testsPassed;
  }

  static async Task RunStep(string stepName, Func<Task> step)
  {
    WriteStepHeader(stepName);
    await step();
    WriteStepFooter(stepName);
  }

  static void RunStep(string stepName, Action step)
  {
    WriteStepHeader(stepName);
    step();
    WriteStepFooter(stepName);
  }

  static void WriteStepHeader(string stepName) => WriteLine($"\n========== Starting: {stepName} ==========");

  static void WriteStepFooter(string stepName) => WriteLine($"========== Completed: {stepName} ==========\n");
}
