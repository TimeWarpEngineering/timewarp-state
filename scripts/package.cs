#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

#region Purpose
// Runfile that builds and packs the NuGet packages into artifacts/packages.
#endregion

#region Design
// A leftover helper; CI uses dev pack. Sets NUGET_PACKAGES to artifacts/nuget-packages, refuses to
// pack when the shared global cache already holds that version from a local path, builds the analyzer,
// source generator and library projects in Release, then packs timewarp-state, -blazor, -plus and
// -policies and lists the .nupkg files.
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
    string repoRoot = Directory.GetCurrentDirectory();
    DevCli.NuGetPackageIsolation.Isolation isolation = DevCli.NuGetPackageIsolation.Apply(repoRoot);
    WriteLine($"NUGET_PACKAGES={isolation.IsolatedPackagesFolder}");

    // Leftover helper for humans. CI uses `dev pack` / `dev workflow` (promote on release).
    // nuget.config lists artifacts/packages as a local source; restore fails with NU1301 when missing.
    string packageOutputPath = "./artifacts/packages";
    Directory.CreateDirectory(packageOutputPath);

    string? version = DevCli.NuGetPackageIsolation.ReadProductVersion(repoRoot);
    if (string.IsNullOrWhiteSpace(version))
    {
      Error.WriteLine("Pack failed: could not read <Version> from source/Directory.Build.props.");
      Environment.Exit(1);
      return;
    }

    string[] packableProjects =
    [
      "./source/timewarp-state/timewarp-state.csproj",
      "./source/timewarp-state-blazor/timewarp-state-blazor.csproj",
      "./source/timewarp-state-plus/timewarp-state-plus.csproj",
      "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    ];

    List<string> packageIds = [];
    foreach (string project in packableProjects)
    {
      string? packageId = DevCli.NuGetPackageIsolation.ReadPackageId(project);
      if (string.IsNullOrWhiteSpace(packageId))
      {
        Error.WriteLine($"Pack failed: could not read <PackageId> from {project}.");
        Environment.Exit(1);
        return;
      }

      packageIds.Add(packageId);
    }

    IReadOnlyList<DevCli.NuGetPackageIsolation.PoisonedCacheEntry> poisoned =
      DevCli.NuGetPackageIsolation.FindPoisonedEntries(
        DevCli.NuGetPackageIsolation.SharedCacheFolders(isolation),
        version,
        packageIds);
    if (DevCli.NuGetPackageIsolation.ReportPoisonedEntries(poisoned, message => Error.WriteLine(message)) != 0)
    {
      Environment.Exit(1);
      return;
    }

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
