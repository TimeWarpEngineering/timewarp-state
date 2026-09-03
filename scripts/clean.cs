#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

var app = new NuruAppBuilder()
    .AddDefaultRoute(async () => await CleanSolution())
    .AddAutoHelp()
    .Build();

return await app.RunAsync(args);

async Task CleanSolution()
{
    using var context = ScriptContext.FromRelativePath("..");

    WriteLine("Cleaning solution...");

    // Clean the solution
    await DotNet.Clean().RunAsync();

    // Clean NuGet cache
    if (Environment.GetEnvironmentVariable("CI") == "true")
    {
        WriteLine("Skipping NuGet local cache clear under CI so the restored actions/cache is reused.");
    }
    else
    {
        await DotNet.NuGet().Locals().Clear(NuGetCacheType.All).RunAsync();
    }

    // Remove common build artifacts
    var directoriesToRemove = new[]
    {
        "./LocalNugetFeed",
        "./tests/test-app/output",
        "./artifacts"
    };

    foreach (var dir in directoriesToRemove)
    {
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
            WriteLine($"Removed: {dir}");
        }
    }

    // Remove bin and obj directories recursively
    var binDirs = Directory.GetDirectories(".", "bin", SearchOption.AllDirectories);
    var objDirs = Directory.GetDirectories(".", "obj", SearchOption.AllDirectories);

    foreach (var dir in binDirs.Concat(objDirs))
    {
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
            WriteLine($"Removed: {dir}");
        }
    }

    // nuget.config lists artifacts/packages as a local source; restore fails with NU1301 when that folder is missing.
    // The guard protects later child `dotnet` invocations, not this runfile's own `#:package` restore.
    Directory.CreateDirectory("./artifacts/packages");

    WriteLine("Solution cleaned successfully.");
}