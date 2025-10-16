#!/usr/bin/dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

var app = new NuruAppBuilder()
    .AddDefaultRoute(async () => await PackageNuGets())
    .AddAutoHelp()
    .Build();

return await app.RunAsync(args);

async Task PackageNuGets()
{
    using var context = ScriptContext.FromRelativePath("..");

    var configuration = "Release";
    var packageOutputPath = "./Nuget";
    var localSourcePath = "./LocalNugetFeed";

    WriteLine("Starting NuGet packaging process...");

    // Kill any running dotnet processes
    try
    {
        await Shell.Builder("taskkill").WithArguments("/F", "/IM", "dotnet.exe", "/T").RunAsync();
    }
    catch
    {
        // Ignore errors on non-Windows or if no processes found
    }

    // Clear NuGet locals
    await DotNet.NuGet().Locals().Clear(NuGetCacheType.All).RunAsync();

    // Clean and setup
    await DotNet.Clean().RunAsync();

    // Remove old directories
    if (Directory.Exists(localSourcePath))
    {
        Directory.Delete(localSourcePath, true);
    }
    if (Directory.Exists("./source/timewarp-state/wwwroot/js"))
    {
        Directory.Delete("./source/timewarp-state/wwwroot/js", true);
    }

    Directory.CreateDirectory(localSourcePath);
    Directory.CreateDirectory(packageOutputPath);

    // Define projects to build and pack
    var projects = new[]
    {
        "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj",
        "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj",
        "./source/timewarp-state/timewarp-state.csproj",
        "./source/timewarp-state-plus/timewarp-state-plus.csproj",
        "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    };

    // Build all projects
    WriteLine("Building projects...");
    foreach (var project in projects)
    {
        WriteLine($"Building {project}...");
        await DotNet.Build()
            .WithProject(project)
            .WithConfiguration(configuration)
            .RunAsync();
    }

    // Pack the projects that should be packaged
    var packableProjects = new[]
    {
        "./source/timewarp-state/timewarp-state.csproj",
        "./source/timewarp-state-plus/timewarp-state-plus.csproj",
        "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    };

    WriteLine("Packing projects...");
    foreach (var project in packableProjects)
    {
        WriteLine($"Packing {project}...");
        await DotNet.Pack()
            .WithProject(project)
            .WithConfiguration(configuration)
            .WithOutput(packageOutputPath)
            .RunAsync();
    }

    // Move packages to local feed
    WriteLine("Moving packages to local feed...");
    var nupkgFiles = Directory.GetFiles(packageOutputPath, "*.nupkg");
    foreach (var nupkgFile in nupkgFiles)
    {
        var fileName = Path.GetFileName(nupkgFile);
        var destPath = Path.Combine(localSourcePath, fileName);
        File.Move(nupkgFile, destPath);
        WriteLine($"Moved {fileName} to local feed");
    }

    // Verify packages were created
    var localPackages = Directory.GetFiles(localSourcePath, "*.nupkg");
    WriteLine($"Created {localPackages.Length} packages in local feed:");
    foreach (var package in localPackages)
    {
        WriteLine($"  - {Path.GetFileName(package)}");
    }

    WriteLine("NuGet packaging completed successfully!");
}