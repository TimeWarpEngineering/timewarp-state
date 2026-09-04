#region Purpose
// Pack the derived IsPackable project set into artifacts/packages
#endregion
#region Design
// Discovers packable projects via IPackableProjectService (MSBuild IsPackable).
// Clears artifacts/packages first so leftovers cannot ship. Reads <Version> from
// source/Directory.Build.props and verifies the nupkg set with CiRunPromotion.
#endregion

namespace DevCli.Commands;

[NuruRoute("pack", Description = "Pack packable projects into artifacts/packages")]
internal sealed class PackCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<PackCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly IPackableProjectService PackableProjectService;

    public Handler(ITerminal terminal, IPackableProjectService packableProjectService)
    {
      Terminal = terminal;
      PackableProjectService = packableProjectService;
    }

    public async ValueTask<Unit> Handle(PackCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Value;
      }

      IReadOnlyList<PackableProject> packableProjects = await PackableProjectService
        .GetPackableProjectsAsync(repoRoot, ct)
        .ConfigureAwait(false);

      if (packableProjects.Count == 0)
      {
        Terminal.WriteErrorLine("Pack failed: no packable projects found under source/.");
        Environment.ExitCode = 1;
        return Value;
      }

      string? version = ReadPropsVersion(repoRoot);
      if (string.IsNullOrWhiteSpace(version))
      {
        Terminal.WriteErrorLine("Pack failed: could not read <Version> from source/Directory.Build.props.");
        Environment.ExitCode = 1;
        return Value;
      }

      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");
      if (Directory.Exists(artifactsDir))
      {
        Directory.Delete(artifactsDir, recursive: true);
      }

      Directory.CreateDirectory(artifactsDir);

      Terminal.WriteLine($"Packable set ({packableProjects.Count}): {string.Join(", ", packableProjects.Select(project => project.PackageId))}");
      Terminal.WriteLine($"Output: {artifactsDir}");

      foreach (PackableProject project in packableProjects)
      {
        Terminal.WriteLine($"Packing {project.PackageId} {version}...");
        int packExit = await Shell.Builder("dotnet")
          .WithArguments(
            "pack",
            project.ProjectPath,
            "--configuration", "Release",
            "--output", artifactsDir)
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync(ct);

        if (packExit != 0)
        {
          Terminal.WriteErrorLine($"dotnet pack failed for {project.PackageId}.".Red());
          Environment.ExitCode = packExit;
          return Value;
        }
      }

      string[] actualNupkgPaths = Directory.GetFiles(artifactsDir, "*.nupkg");
      IReadOnlyList<string> actualFileNames = [.. actualNupkgPaths.Select(Path.GetFileName)!];
      PackageSetVerification verification = CiRunPromotion.VerifyPackageSet(actualFileNames, packableProjects, version);

      if (!verification.IsMatch)
      {
        if (verification.Missing.Count > 0)
        {
          Terminal.WriteErrorLine($"Pack failed: missing package(s): {string.Join(", ", verification.Missing)}.");
        }

        if (verification.Unexpected.Count > 0)
        {
          Terminal.WriteErrorLine($"Pack failed: unexpected package(s): {string.Join(", ", verification.Unexpected)}.");
        }

        Environment.ExitCode = 1;
        return Value;
      }

      Terminal.WriteLine("\nPacked set verified.".Green());
      foreach (string fileName in actualFileNames.OrderBy(name => name, StringComparer.Ordinal))
      {
        Terminal.WriteLine($"  {fileName}");
      }

      return Value;
    }

    private static string? ReadPropsVersion(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      if (!File.Exists(propsPath))
      {
        return null;
      }

      XDocument document = XDocument.Load(propsPath);
      string? value = document.Descendants("Version").FirstOrDefault()?.Value.Trim();
      return string.IsNullOrWhiteSpace(value) ? null : value;
    }
  }
}
