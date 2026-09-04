#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

NuruApp app = NuruApp.CreateBuilder()
  .Map("build {config?|Build configuration (Debug/Release)}")
    .WithHandler(App.Build)
    .AsCommand()
    .Done()
  .Map("clean")
    .WithHandler(App.CleanSolution)
    .AsCommand()
    .Done()
  .Map("")
    .WithHandler(App.BuildDefault)
    .AsCommand()
    .Done()
  .Build();

return await app.RunAsync(args);

static class App
{
  const string ArtifactsDirectory = "./artifacts";
  const string PackagesDirectory = "./artifacts/packages";

  public static Task BuildDefault() => BuildProjects("Release");

  public static Task Build(string? config) => BuildProjects(config ?? "Release");

  static async Task BuildProjects(string configuration)
  {
    using ScriptContext context = ScriptContext.FromRelativePath("..");

    Directory.CreateDirectory(PackagesDirectory);

    WriteLine($"Script location: {context.ScriptDirectory}");
    WriteLine($"Working from: {Directory.GetCurrentDirectory()}");
    WriteLine($"Configuration: {configuration}");

    WriteLine("\nListing installed .NET SDKs:");
    await DotNet.WithListSdks().RunAsync();

    WriteLine("\nRestoring dotnet tools...");
    await DotNet.Tool().Restore().RunAsync();

    string[] projects =
    [
      "./source/timewarp-state/timewarp-state.csproj",
      "./source/timewarp-state-plus/timewarp-state-plus.csproj",
      "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    ];

    foreach (string project in projects)
    {
      if (!File.Exists(project))
      {
        WriteLine($"⚠️ Project not found: {project}");
        continue;
      }

      WriteLine($"\nBuilding {Path.GetFileNameWithoutExtension(project)}...");

      await DotNet.Build()
        .WithProject(project)
        .WithConfiguration(configuration)
        .WithVerbosity("minimal")
        .RunAsync();

      WriteLine($"✅ Built {Path.GetFileNameWithoutExtension(project)}");
    }

    WriteLine("\n✅ Build completed successfully!");
    WriteLine($"Packages available in: {PackagesDirectory}");
  }

  public static async Task CleanSolution()
  {
    using ScriptContext context = ScriptContext.FromRelativePath("..");

    WriteLine("Cleaning solution...");

    try
    {
      await Shell.Builder("pkill")
        .WithArguments("-f", "dotnet")
        .RunAsync();
    }
    catch
    {
      // Ignore if pkill not found or no processes
    }

    if (Environment.GetEnvironmentVariable("CI") == "true")
    {
      WriteLine("Skipping NuGet local cache clear under CI so the restored actions/cache is reused.");
    }
    else
    {
      WriteLine("Clearing NuGet caches...");
      await DotNet.NuGet()
        .Locals()
        .Clear(NuGetCacheType.All)
        .RunAsync();
    }

    WriteLine("Cleaning solution...");
    await DotNet.Clean().RunAsync();

    if (Directory.Exists(ArtifactsDirectory))
    {
      WriteLine("Removing artifacts directory...");
      Directory.Delete(ArtifactsDirectory, recursive: true);
    }

    Directory.CreateDirectory(PackagesDirectory);

    if (Directory.Exists("./source/timewarp-state/wwwroot/js"))
    {
      WriteLine("Removing generated JS...");
      Directory.Delete("./source/timewarp-state/wwwroot/js", recursive: true);
    }

    WriteLine("✅ Clean completed!");
  }
}
