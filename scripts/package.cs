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
    .WithHandler(App.PackageNuGets)
    .AsCommand()
    .Done()
  .Build();

return await app.RunAsync(args);

static class App
{
  public static async Task PackageNuGets()
  {
    using ScriptContext context = ScriptContext.FromRelativePath("..");

    // Leftover helper for humans. CI uses `dev pack` / `dev workflow` (promote on release).
    // nuget.config lists artifacts/packages as a local source; restore fails with NU1301 when missing.
    string packageOutputPath = "./artifacts/packages";
    Directory.CreateDirectory(packageOutputPath);

    string configuration = "Release";

    WriteLine("Starting NuGet packaging process...");

    if (Directory.Exists("./source/timewarp-state/wwwroot/js"))
    {
      Directory.Delete("./source/timewarp-state/wwwroot/js", true);
    }

    string[] buildProjects =
    [
      "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj",
      "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj",
      "./source/timewarp-state/timewarp-state.csproj",
      "./source/timewarp-state-plus/timewarp-state-plus.csproj",
      "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    ];

    WriteLine("Building projects...");
    foreach (string project in buildProjects)
    {
      WriteLine($"Building {project}...");
      await DotNet.Build()
        .WithProject(project)
        .WithConfiguration(configuration)
        .RunAsync();
    }

    string[] packableProjects =
    [
      "./source/timewarp-state/timewarp-state.csproj",
      "./source/timewarp-state-plus/timewarp-state-plus.csproj",
      "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    ];

    WriteLine("Packing projects...");
    foreach (string project in packableProjects)
    {
      WriteLine($"Packing {project}...");
      await DotNet.Pack()
        .WithProject(project)
        .WithConfiguration(configuration)
        .WithOutput(packageOutputPath)
        .RunAsync();
    }

    string[] localPackages = Directory.GetFiles(packageOutputPath, "*.nupkg");
    WriteLine($"Created {localPackages.Length} packages in {packageOutputPath}:");
    foreach (string package in localPackages)
    {
      WriteLine($"  - {Path.GetFileName(package)}");
    }

    WriteLine("NuGet packaging completed successfully!");
  }
}
