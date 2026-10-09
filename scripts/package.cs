#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

#region Purpose
// Runfile that builds and packs the NuGet packages into artifacts/packages.
#endregion

#region Design
// A leftover helper; CI uses dev pack. Builds the analyzer, source generator and library projects in
// Release, then packs timewarp-state, -blazor, -plus and -policies and lists the .nupkg files.
#endregion

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

    if (Directory.Exists("./source/timewarp-state-blazor/wwwroot/js"))
    {
      Directory.Delete("./source/timewarp-state-blazor/wwwroot/js", true);
    }

    string[] buildProjects =
    [
      "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj",
      "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj",
      "./source/timewarp-state/timewarp-state.csproj",
      "./source/timewarp-state-blazor/timewarp-state-blazor.csproj",
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
      "./source/timewarp-state-blazor/timewarp-state-blazor.csproj",
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
