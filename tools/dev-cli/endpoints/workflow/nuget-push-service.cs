#region Purpose
// Pushes the verified package set in artifacts/packages to NuGet.org.
#endregion

#region Design
// The caller passes the version it already asserted, so this does not re-read the props file.
// API key exposure: `dotnet nuget push` takes the key only as --api-key (no env var or stdin option, and
// NuGet.Config apikeys need DPAPI, which Linux runners lack). The key is therefore visible in the child
// process argv for the length of each push. Mitigations: CI passes it to `dev workflow` via NUGET_API_KEY
// (not argv), the OIDC Trusted Publishing key is short-lived, and the hosted runner is single-tenant.
// Each push uses WithNoValidation and checks the exit code so "Failed to push X" is reachable.
#endregion

namespace DevCli.Commands;

internal sealed class NuGetPushService
{
  private const string NuGetSource = "https://api.nuget.org/v3/index.json";

  private readonly ITerminal Terminal;

  public NuGetPushService(ITerminal terminal)
  {
    Terminal = terminal;
  }

  public async Task PushPackagesAsync(
    string repoRoot,
    IReadOnlyList<PackableProject> projects,
    string version,
    string? apiKey,
    CancellationToken ct)
  {
    string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
    HashSet<string> expectedNupkgFileNames = [.. projects.Select(project => $"{project.PackageId}.{version}.nupkg")];

    string[] actualNupkgFiles = Directory.Exists(artifactsDir)
      ? Directory.GetFiles(artifactsDir, $"*.{version}.nupkg")
      : [];

    List<string> unexpectedNupkgFileNames = [.. actualNupkgFiles
      .Select(Path.GetFileName)
      .OfType<string>()
      .Where(fileName => !expectedNupkgFileNames.Contains(fileName))];

    if (unexpectedNupkgFileNames.Count > 0)
    {
      throw new InvalidOperationException($"Unexpected package(s) in {artifactsDir} not in the derived packable set: {string.Join(", ", unexpectedNupkgFileNames)}");
    }

    foreach (PackableProject project in projects)
    {
      string nupkgPath = Path.Combine(artifactsDir, $"{project.PackageId}.{version}.nupkg");

      if (!File.Exists(nupkgPath))
      {
        throw new FileNotFoundException($"Package not found: {nupkgPath}");
      }

      Terminal.WriteLine($"Pushing {project.PackageId}.{version}.nupkg...");

      List<string> args = ["nuget", "push", nupkgPath, "--source", NuGetSource, "--skip-duplicate"];

      if (!string.IsNullOrEmpty(apiKey))
      {
        args.AddRange(["--api-key", apiKey]);
      }

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments([.. args])
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        throw new InvalidOperationException($"Failed to push {project.PackageId} (dotnet nuget push exit {exitCode})!");
      }
    }

    Terminal.WriteLine("\nAll packages pushed successfully!");
  }
}
