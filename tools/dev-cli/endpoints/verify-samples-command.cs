#region Purpose
// Verify sample projects compile, then publish the persistence sample trimmed.
#endregion
#region Design
// Builds each samples/**/*.csproj so the required verify-samples capability is
// a real gate, not a stub. Samples that PackageReference TimeWarp.State restore
// with --source artifacts/packages (root nuget.config does not list that folder).
// Workflow runs pack before this command so the in-tree version is in that feed.
// Sample restores extract into artifacts/nuget-packages, not ~/.nuget/packages.
// After the builds, publishes samples/05-persistence trimmed with TrimmerSingleWarn=false
// so Blazor and Plus trim warnings fail this gate.
#endregion

namespace DevCli.Commands;

[NuruRoute("verify-samples", Description = "Verify code samples compile")]
internal sealed class VerifySamplesCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<VerifySamplesCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(VerifySamplesCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Value;
      }

      string localFeed = Path.Combine(repoRoot, "artifacts", "packages");
      // --source requires the folder. Pack creates it; this covers a verify-samples run after a partial pack.
      Directory.CreateDirectory(localFeed);

      string samplesDirectory = Path.Combine(repoRoot, "samples");
      if (!Directory.Exists(samplesDirectory))
      {
        Terminal.WriteLine("No samples/ directory — nothing to verify.");
        return Value;
      }

      string[] projects = Directory
        .GetFiles(samplesDirectory, "*.csproj", SearchOption.AllDirectories)
        .Where(path =>
          !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
          && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

      if (projects.Length == 0)
      {
        Terminal.WriteLine("No sample projects found.");
        return Value;
      }

      Terminal.WriteLine($"Verifying {projects.Length} sample project(s)...");

      foreach (string project in projects)
      {
        string relativePath = Path.GetRelativePath(repoRoot, project);
        Terminal.WriteLine($"\nBuilding {relativePath}...");
        int exitCode = await BuildSampleAsync(project, repoRoot, localFeed, ct);

        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"Sample failed: {relativePath}".Red());
          Environment.ExitCode = exitCode;
          return Value;
        }
      }

      string persistenceProject = Path.Combine
      (
        repoRoot,
        "samples",
        "05-persistence",
        "wasm",
        "sample-05-wasm",
        "sample-05-wasm.csproj"
      );
      if (File.Exists(persistenceProject))
      {
        string publishOutput = Path.Combine(repoRoot, "artifacts", "publish", "sample-05-wasm-trimmed");
        Terminal.WriteLine("\nPublishing samples/05-persistence trimmed...");
        int publishExitCode = await Shell.Builder("dotnet")
          .WithArguments
          (
            "publish",
            persistenceProject,
            "--configuration", "Release",
            "--output", publishOutput,
            "-p:PublishTrimmed=true",
            "-p:TrimmerSingleWarn=false",
            "--source", localFeed,
            "--source", NuGetOrgSource
          )
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync(ct);

        if (publishExitCode != 0)
        {
          Terminal.WriteErrorLine("Trimmed publish failed: samples/05-persistence".Red());
          Environment.ExitCode = publishExitCode;
          return Value;
        }
      }

      Terminal.WriteLine("\nSamples verified successfully!".Green());
      return Value;
    }

    private static Task<int> BuildSampleAsync(string project, string repoRoot, string localFeed, CancellationToken ct) =>
      Shell.Builder("dotnet")
        .WithArguments
        (
          "build",
          project,
          "--configuration", "Release",
          "--source", localFeed,
          "--source", NuGetOrgSource
        )
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

    private const string NuGetOrgSource = "https://api.nuget.org/v3/index.json";
  }
}
