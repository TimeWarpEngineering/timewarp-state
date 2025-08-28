#!/usr/bin/dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

var app = new NuruAppBuilder()
    .AddDefaultRoute(async () => await RunTestApp())
    .AddAutoHelp()
    .Build();

return await app.RunAsync(args);

static async Task RunTestApp()
{
    using var context = ScriptContext.FromRelativePath("..");

    // Set environment variables like the original PowerShell script
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    Environment.SetEnvironmentVariable("UseHttp", "true");

    // The analyzer is not directly referenced by the test app, so we need to build it first
    var exitCode = await DotNet.Build()
        .WithProject("./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj")
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    exitCode = await DotNet.Build()
        .WithProject("./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj")
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Run the test app server with launch profile http
    exitCode = await DotNet.Run()
        .WithProject("./tests/test-app/test-app-server/test-app-server.csproj")
        .WithArguments("--launch-profile", "http")
        .RunAsync();

    if (exitCode != 0)
    {
        Environment.Exit(1);
    }
}