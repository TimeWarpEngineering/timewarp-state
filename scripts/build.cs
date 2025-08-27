#!/usr/bin/dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

var app = new NuruAppBuilder()
    .AddRoute("build {configuration?|Build configuration (Debug/Release)}", 
        async (string? configuration) => await BuildProjects(configuration ?? "Release", false))
    .AddRoute("pack {configuration?|Build configuration (Debug/Release)}", 
        async (string? configuration) => await BuildProjects(configuration ?? "Release", true))
    .AddRoute("clean", async () => await CleanSolution())
    .AddDefaultRoute(async () => await BuildProjects("Release", false))  // Default when no args
    .AddAutoHelp()  // Automatically generates help for all routes
    .Build();

return await app.RunAsync(args);

// Build implementation
async Task BuildProjects(string configuration, bool pack)
{
    // Use ScriptContext to manage directory changes - automatically restores on dispose
    using var context = ScriptContext.FromRelativePath("..");  // Go up one level from scripts/ to repo root

    WriteLine($"Script location: {context.ScriptDirectory}");
    WriteLine($"Working from: {Directory.GetCurrentDirectory()}");
    WriteLine($"Configuration: {configuration}");

// List installed .NET SDKs
WriteLine("\nListing installed .NET SDKs:");
await DotNet.WithListSdks().RunAsync();

// Restore tools
WriteLine("\nRestoring dotnet tools...");
await DotNet.Tool().Restore().RunAsync();
// Create local NuGet feed directory
WriteLine("\nCreating local NuGet feed directory...");
Directory.CreateDirectory("./local-nuget-feed");

// Projects to build in order
var projects = new[]
{
    "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj",
    "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj",
    "./source/timewarp-state/timewarp-state.csproj",
    "./source/timewarp-state-plus/timewarp-state-plus.csproj",
    "./source/timewarp-state-policies/timewarp-state-policies.csproj"
};

// Build each project
foreach (var project in projects)
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

    // Pack main projects if requested
    if (pack)
{
    var packProjects = new[]
    {
        "./source/timewarp-state/timewarp-state.csproj",
        "./source/timewarp-state-plus/timewarp-state-plus.csproj",
        "./source/timewarp-state-policies/timewarp-state-policies.csproj"
    };
    
    foreach (var project in packProjects)
    {
        if (!File.Exists(project))
            continue;
            
        WriteLine($"\nPacking {Path.GetFileNameWithoutExtension(project)}...");
        
        await DotNet.Pack()
            .WithProject(project)
            .WithConfiguration(configuration)
            .WithOutput("./local-nuget-feed")
            .RunAsync();
        
        WriteLine($"✅ Packed {Path.GetFileNameWithoutExtension(project)}");
    }
}

    WriteLine("\n✅ Build completed successfully!");
}

// Clean implementation
async Task CleanSolution()
{
    using var context = ScriptContext.FromRelativePath("..");
    
    WriteLine("Cleaning solution...");
    
    // Kill any running dotnet processes
    try
    {
        await Shell.Builder("pkill")
            .WithArguments("-f", "dotnet")
            .RunAsync();
    }
    catch { /* Ignore if pkill not found or no processes */ }

    // Clear NuGet caches
    WriteLine("Clearing NuGet caches...");
    await DotNet.NuGet()
        .Locals()
        .Clear(NuGetCacheType.All)
        .RunAsync();

    // Clean solution
    WriteLine("Cleaning solution...");
    await DotNet.Clean().RunAsync();
    
    // Remove directories
    if (Directory.Exists("./local-nuget-feed"))
    {
        WriteLine("Removing local-nuget-feed...");
        Directory.Delete("./local-nuget-feed", recursive: true);
    }
    
    if (Directory.Exists("./source/timewarp-state/wwwroot/js"))
    {
        WriteLine("Removing generated JS...");
        Directory.Delete("./source/timewarp-state/wwwroot/js", recursive: true);
    }
    
    WriteLine("✅ Clean completed!");
}