#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

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

  // nuget.config lists artifacts/packages as a local source; restore fails with NU1301 when that folder is missing.
  // The guard protects the child `dotnet` invocations below, not this runfile's own `#:package` restore.
  Directory.CreateDirectory("./artifacts/packages");

  await RunStep("Restore local tools", () => Shell.Builder("dotnet")
    .WithArguments("tool", "restore")
    .RunAsync());

  await RunStep("Build analyzer tests", () => DotNet.Build()
    .WithProject("./tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj")
    .RunAsync());

  await RunStep("Run analyzer tests", () => Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-analyzer-tests")
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
