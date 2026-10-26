#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

#region Purpose
// Runfile that cleans the repo: dotnet clean, NuGet caches, build output folders and every bin/obj.
#endregion

#region Design
// Sets NUGET_PACKAGES to artifacts/nuget-packages before clearing NuGet locals, so the global-packages
// folder that clear removes is the isolated one. Skips that clear under CI. Does not recreate
// artifacts/packages: root nuget.config does not list that folder as a source.
#endregion

NuruApp app = NuruApp.CreateBuilder()
  .Map("")
    .WithHandler(App.CleanSolution)
    .AsCommand()
    .Done()
  .Build();

return await app.RunAsync(args);

static class App
{
  public static async Task CleanSolution()
  {
    using var context = ScriptContext.FromRelativePath("..");
    DevCli.NuGetPackageIsolation.Isolation isolation =
      DevCli.NuGetPackageIsolation.Apply(Directory.GetCurrentDirectory());
    WriteLine($"NUGET_PACKAGES={isolation.IsolatedPackagesFolder}");

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

    WriteLine("Solution cleaned successfully.");
  }
}