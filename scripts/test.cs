#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

#region Purpose
// Runfile behind dev test: builds and runs each Fixie test project in turn.
#endregion

#region Design
// Sets NUGET_PACKAGES to artifacts/nuget-packages, restores local tools, then for each suite
// (analyzer, source generator, state, plus, telemetry, devtools, devtools isolation, client
// integration, architecture) builds the project and runs dotnet fixie. RunStep names the failing
// suite and exits 1, whether Amuru throws or returns a non-zero code. Root nuget.config does
// not list artifacts/packages, so this runfile does not create that folder.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .Map("")
    .WithHandler(App.RunTests)
    .AsCommand()
    .Done()
  .Build();

return await app.RunAsync(args);

static class App
{
  public static async Task RunTests()
  {
  using var context = ScriptContext.FromRelativePath("..");
  DevCli.NuGetPackageIsolation.Isolation isolation =
    DevCli.NuGetPackageIsolation.Apply(Directory.GetCurrentDirectory());
  WriteLine($"NUGET_PACKAGES={isolation.IsolatedPackagesFolder}");

  await RunStep("Restore local tools", () => Shell.Builder("dotnet")
    .WithArguments("tool", "restore")
    .RunAsync());

  await RunStep("Build analyzer tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj")
    .RunAsync());

  await RunStep("Run analyzer tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-analyzer-tests")
    .RunAsync());

  await RunStep("Build source generator tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-source-generator-tests/timewarp-state-source-generator-tests.csproj")
    .RunAsync());

  await RunStep("Run source generator tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-source-generator-tests")
    .RunAsync());

  await RunStep("Build state tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-tests/timewarp-state-tests.csproj")
    .RunAsync());

  await RunStep("Run state tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-tests")
    .RunAsync());

  await RunStep("Build plus tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-plus-tests/timewarp-state-plus-tests.csproj")
    .RunAsync());

  await RunStep("Run plus tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-plus-tests")
    .RunAsync());

  await RunStep("Build telemetry tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-telemetry-tests/timewarp-state-telemetry-tests.csproj")
    .RunAsync());

  await RunStep("Run telemetry tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-telemetry-tests")
    .RunAsync());

  await RunStep("Build devtools tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj")
    .RunAsync());

  await RunStep("Run devtools tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-devtools-tests")
    .RunAsync());

  await RunStep("Build devtools isolation tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-devtools-isolation-tests/timewarp-state-devtools-isolation-tests.csproj")
    .RunAsync());

  await RunStep("Run devtools isolation tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-devtools-isolation-tests")
    .RunAsync());

  await RunStep("Build client integration tests", () => DotNet.Build()
    .WithProject("./tests/client-integration-tests/client-integration-tests.csproj")
    .RunAsync());

  await RunStep("Run client integration tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "client-integration-tests")
    .RunAsync());

  await RunStep("Build architecture tests", () => DotNet.Build()
    .WithProject("./tests/test-app-architecture-tests/test-app-architecture-tests.csproj")
    .RunAsync());

    await RunStep("Run architecture tests", () => Shell.Builder("dotnet")
      .WithArguments("fixie", "test-app-architecture-tests")
      .RunAsync());
  }

  // Amuru throws CommandExecutionException on a non-zero child exit, so name the failing suite on both paths.
  static async Task RunStep(string name, Func<Task<int>> step)
  {
    WriteLine($"==> {name}");

    int exitCode;
    try
    {
      exitCode = await step();
    }
    catch (Exception ex)
    {
      await Error.WriteLineAsync($"{name} failed: {ex.Message.Split('\n')[0]}");
      Environment.Exit(1);
      return;
    }

    if (exitCode != 0)
    {
      await Error.WriteLineAsync($"{name} failed: exit code {exitCode}");
      Environment.Exit(1);
    }
  }
}
