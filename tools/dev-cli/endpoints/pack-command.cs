#region Purpose
// Pack the derived IsPackable project set into artifacts/packages
#endregion
#region Design
// Discovers packable projects via IPackableProjectService (MSBuild IsPackable).
// Clears artifacts/packages first so leftovers cannot ship. Reads <Version> from
// source/Directory.Build.props, verifies the nupkg set with CiRunPromotion, then
// rejects any nuspec dependency id outside NuspecDependencyAllowList.
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

    public async Task<Unit> Handle(PackCommand command, CancellationToken ct)
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
            "--output", artifactsDir,
            // source/Directory.Build.props sets GeneratePackageOnBuild=true. Under the .NET 11 SDK an
            // explicit `dotnet pack` of such a project skips Build and silently omits the Razor class
            // library's staticwebassets/ files (scoped CSS bundle, wwwroot JS) from the nupkg.
            // Packing with GeneratePackageOnBuild=false makes Pack depend on Build so they are included.
            "-p:GeneratePackageOnBuild=false")
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

      if (!NuspecDependenciesAllowed(actualNupkgPaths, version))
      {
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

    private bool NuspecDependenciesAllowed(IEnumerable<string> nupkgPaths, string version)
    {
      bool allowed = true;
      foreach (string nupkgPath in nupkgPaths.OrderBy(path => path, StringComparer.Ordinal))
      {
        string fileName = Path.GetFileName(nupkgPath);
        string packageId = PackageIdFromNupkgFileName(fileName, version);
        IReadOnlyList<string> dependencyIds = NuspecDependencyAllowList.ReadDependencyIdsFromNupkg(nupkgPath);
        IReadOnlyList<string> disallowed = NuspecDependencyAllowList.FindDisallowed(packageId, dependencyIds);
        if (disallowed.Count > 0)
        {
          Terminal.WriteErrorLine(
            $"Pack failed: {packageId} nuspec dependency outside the allow-list: {string.Join(", ", disallowed)}.".Red());
          allowed = false;
          continue;
        }

        string listed = dependencyIds.Count == 0 ? "(none)" : string.Join(", ", dependencyIds);
        Terminal.WriteLine($"  {packageId} dependencies: {listed}");
      }

      return allowed;
    }

    private static string PackageIdFromNupkgFileName(string fileName, string version)
    {
      string suffix = $".{version}.nupkg";
      if (!fileName.EndsWith(suffix, StringComparison.Ordinal))
      {
        throw new InvalidOperationException($"Nupkg '{fileName}' does not end with '{suffix}'.");
      }

      return fileName[..^suffix.Length];
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
