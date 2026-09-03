#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

var app = new NuruAppBuilder()
    .AddDefaultRoute(async () => await RunTests())
    .AddAutoHelp()
    .Build();

return await app.RunAsync(args);

static async Task RunTests()
{
  using var context = ScriptContext.FromRelativePath("..");

  // nuget.config lists artifacts/packages as a local source; restore fails with NU1301 when that folder is missing.
  Directory.CreateDirectory("./artifacts/packages");

  // Build and run analyzer tests
  int exitCode = await DotNet.Build()
    .WithProject("./tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  exitCode = await Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-analyzer-tests")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  // Build and run state tests
  exitCode = await DotNet.Build()
    .WithProject("./tests/timewarp-state-tests/timewarp-state-tests.csproj")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  exitCode = await Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-tests")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  // Build and run plus tests
  exitCode = await DotNet.Build()
    .WithProject("./tests/timewarp-state-plus-tests/timewarp-state-plus-tests.csproj")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  exitCode = await Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-plus-tests")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  // Build and run integration tests
  exitCode = await DotNet.Build()
    .WithProject("./tests/client-integration-tests/client-integration-tests.csproj")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  exitCode = await Shell.Builder("dotnet")
    .WithArguments("fixie", "client-integration-tests")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  // Build and run architecture tests
  exitCode = await DotNet.Build()
    .WithProject("./tests/test-app-architecture-tests/test-app-architecture-tests.csproj")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);

  exitCode = await Shell.Builder("dotnet")
    .WithArguments("fixie", "test-app-architecture-tests")
    .RunAsync();
  if (exitCode != 0) Environment.Exit(1);
}