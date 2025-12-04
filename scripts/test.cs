#!/usr/bin/dotnet --
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

  // Build and run analyzer tests
  var result = await DotNet.Build()
    .WithProject("./tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  result = await Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-analyzer-tests")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  // Build and run state tests
  result = await DotNet.Build()
    .WithProject("./tests/timewarp-state-tests/timewarp-state-tests.csproj")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  result = await Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-tests")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  // Build and run plus tests
  result = await DotNet.Build()
    .WithProject("./tests/timewarp-state-plus-tests/timewarp-state-plus-tests.csproj")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  result = await Shell.Builder("dotnet")
    .WithArguments("fixie", "timewarp-state-plus-tests")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  // Build and run integration tests
  result = await DotNet.Build()
    .WithProject("./tests/client-integration-tests/client-integration-tests.csproj")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  result = await Shell.Builder("dotnet")
    .WithArguments("fixie", "client-integration-tests")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  // Build and run architecture tests
  result = await DotNet.Build()
    .WithProject("./tests/test-app-architecture-tests/test-app-architecture-tests.csproj")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);

  result = await Shell.Builder("dotnet")
    .WithArguments("fixie", "test-app-architecture-tests")
    .ExecuteAsync();
  if (!result.IsSuccess) Environment.Exit(1);
}