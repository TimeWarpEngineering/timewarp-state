#!/usr/bin/dotnet --
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
    await DotNet.NuGet().Locals().Clear(NuGetCacheType.All).RunAsync();

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

    WriteLine("Solution cleaned successfully.");
}