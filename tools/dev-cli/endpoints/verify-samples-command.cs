#region Purpose
// Verify sample projects compile, pack, then restore the one packaged consumer.
#endregion
#region Design
// Nine samples ProjectReference source, so they build with no local feed.
// Sample 00 auto (server host and WebAssembly client) is the packaged consumer:
// this command packs, then restores that folder with a config file whose
// packageSourceMapping sends TimeWarp.State and TimeWarp.State.* only to
// artifacts/packages. Command-line --source is not combined with that file,
// because those sources skip the mapping. dotnet build does not take the
// config file, so the packaged projects restore with it and then build
// --no-restore. Before that restore, each packable id's version folder under
// artifacts/nuget-packages is deleted and the restore passes --force. NuGet
// otherwise reuses a same-version extract and never reads the nupkg just packed.
// RestorePackagesPath names that folder and wins over NUGET_PACKAGES, so a
// per-run env folder would not move the extract. The trimmed publish of
// samples/05-persistence stays on the project-reference build and does not
// pass a feed. It sets VerifySamplesTrim instead of a global PublishTrimmed,
// because that property would also mark the netstandard analyzer projects.
#endregion

namespace DevCli.Commands;

[NuruRoute("verify-samples", Description = "Verify code samples compile")]
internal sealed class VerifySamplesCommand : ICommand<Unit>
{
  private const string PackagedSampleMarker = "samples/00-state-action-handler/auto/";

  internal sealed class Handler : ICommandHandler<VerifySamplesCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly IPackableProjectService PackableProjectService;

    public Handler(ITerminal terminal, IPackableProjectService packableProjectService)
    {
      Terminal = terminal;
      PackableProjectService = packableProjectService;
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

      string[] packagedProjects = projects.Where(IsPackagedConsumer).ToArray();
      string[] sourceProjects = projects.Where(project => !IsPackagedConsumer(project)).ToArray();
      Terminal.WriteLine(
        $"Verifying {sourceProjects.Length} project-referenced sample(s), then packing, then {packagedProjects.Length} packaged sample(s)...");

      foreach (string project in sourceProjects)
      {
        if (!await BuildSourceSampleAsync(project, repoRoot, ct))
        {
          return Value;
        }
      }

      if (!await PublishPersistenceTrimmedAsync(repoRoot, ct))
      {
        return Value;
      }

      Terminal.WriteLine("\nPacking so sample 00 auto can restore the local feed...");
      await new PackCommand.Handler(Terminal, PackableProjectService).Handle(new PackCommand(), ct);
      if (Environment.ExitCode != 0)
      {
        return Value;
      }

      if (packagedProjects.Length == 0)
      {
        Terminal.WriteLine("\nSamples verified successfully!".Green());
        return Value;
      }

      if (!await PurgePackedExtractsAsync(repoRoot, ct))
      {
        return Value;
      }

      string configPath = Path.Combine(repoRoot, "artifacts", "sample-00-auto.nuget.config");
      WritePackagedConsumerConfig(repoRoot, configPath);

      foreach (string project in packagedProjects)
      {
        if (!await BuildPackagedSampleAsync(project, repoRoot, configPath, ct))
        {
          return Value;
        }
      }

      Terminal.WriteLine("\nSamples verified successfully!".Green());
      return Value;
    }

    private async Task<bool> BuildSourceSampleAsync(string project, string repoRoot, CancellationToken ct)
    {
      string relativePath = Path.GetRelativePath(repoRoot, project);
      Terminal.WriteLine($"\nBuilding {relativePath}...");
      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("build", project, "--configuration", "Release")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine($"Sample failed: {relativePath}".Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> PublishPersistenceTrimmedAsync(string repoRoot, CancellationToken ct)
    {
      string persistenceProject = Path.Combine
      (
        repoRoot,
        "samples",
        "05-persistence",
        "wasm",
        "sample-05-wasm",
        "sample-05-wasm.csproj"
      );
      if (!File.Exists(persistenceProject))
      {
        return true;
      }

      string publishOutput = Path.Combine(repoRoot, "artifacts", "publish", "sample-05-wasm-trimmed");
      Terminal.WriteLine("\nPublishing samples/05-persistence trimmed...");
      int publishExitCode = await Shell.Builder("dotnet")
        .WithArguments
        (
          "publish",
          persistenceProject,
          "--configuration", "Release",
          "--output", publishOutput,
          "-p:VerifySamplesTrim=true",
          "-p:TrimmerSingleWarn=false"
        )
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (publishExitCode != 0)
      {
        Terminal.WriteErrorLine("Trimmed publish failed: samples/05-persistence".Red());
        Environment.ExitCode = publishExitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> BuildPackagedSampleAsync(string project, string repoRoot, string configPath, CancellationToken ct)
    {
      string relativePath = Path.GetRelativePath(repoRoot, project);
      Terminal.WriteLine($"\nRestoring {relativePath} from the local feed...");
      int restoreExitCode = await Shell.Builder("dotnet")
        .WithArguments("restore", project, "--configfile", configPath, "--force")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (restoreExitCode != 0)
      {
        Terminal.WriteErrorLine($"Sample restore failed: {relativePath}".Red());
        Environment.ExitCode = restoreExitCode;
        return false;
      }

      Terminal.WriteLine($"Building {relativePath}...");
      int buildExitCode = await Shell.Builder("dotnet")
        .WithArguments("build", project, "--configuration", "Release", "--no-restore")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (buildExitCode != 0)
      {
        Terminal.WriteErrorLine($"Sample failed: {relativePath}".Red());
        Environment.ExitCode = buildExitCode;
        return false;
      }

      return true;
    }

    private async Task<bool> PurgePackedExtractsAsync(string repoRoot, CancellationToken ct)
    {
      string? version = NuGetPackageIsolation.ReadProductVersion(repoRoot);
      if (string.IsNullOrWhiteSpace(version))
      {
        Terminal.WriteErrorLine("Error: could not read <Version> from source/Directory.Build.props.");
        Environment.ExitCode = 1;
        return false;
      }

      IReadOnlyList<PackableProject> packableProjects = await PackableProjectService
        .GetPackableProjectsAsync(repoRoot, ct)
        .ConfigureAwait(false);
      if (packableProjects.Count == 0)
      {
        Terminal.WriteErrorLine(
          "Error: no packable projects found; refusing to restore sample 00 auto against the existing cache.");
        Environment.ExitCode = 1;
        return false;
      }

      string packagesFolder = NuGetPackageIsolation.Apply(repoRoot).IsolatedPackagesFolder;
      IReadOnlyList<string> deleted;
      try
      {
        deleted = NuGetPackageIsolation.DeleteVersionFolders(
          packagesFolder,
          version,
          packableProjects.Select(project => project.PackageId));
      }
      catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
      {
        Terminal.WriteErrorLine($"Could not purge the isolated package cache: {exception.Message}");
        Environment.ExitCode = 1;
        return false;
      }

      if (deleted.Count == 0)
      {
        Terminal.WriteLine("No same-version extracts were present before the packaged sample restore.");
        return true;
      }

      foreach (string path in deleted)
      {
        Terminal.WriteLine($"Purged {Path.GetRelativePath(repoRoot, path)}");
      }

      return true;
    }

    private static void WritePackagedConsumerConfig(string repoRoot, string configPath)
    {
      string localFeed = Path.GetFullPath(Path.Combine(repoRoot, "artifacts", "packages"));
      string packageCache = Path.GetFullPath(Path.Combine(repoRoot, "artifacts", "nuget-packages"));
      Directory.CreateDirectory(localFeed);
      Directory.CreateDirectory(packageCache);
      Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

      XDocument document = new(
        new XElement(
          "configuration",
          new XElement(
            "config",
            new XElement(
              "add",
              new XAttribute("key", "globalPackagesFolder"),
              new XAttribute("value", packageCache))),
          new XElement(
            "packageSources",
            new XElement("clear"),
            new XElement(
              "add",
              new XAttribute("key", "local"),
              new XAttribute("value", localFeed)),
            new XElement(
              "add",
              new XAttribute("key", "nuget.org"),
              new XAttribute("value", "https://api.nuget.org/v3/index.json"),
              new XAttribute("protocolVersion", "3"))),
          new XElement(
            "packageSourceMapping",
            new XElement(
              "packageSource",
              new XAttribute("key", "local"),
              new XElement("package", new XAttribute("pattern", "TimeWarp.State")),
              new XElement("package", new XAttribute("pattern", "TimeWarp.State.*"))),
            new XElement(
              "packageSource",
              new XAttribute("key", "nuget.org"),
              new XElement("package", new XAttribute("pattern", "*"))))));

      document.Save(configPath);
    }

    private static bool IsPackagedConsumer(string projectPath)
    {
      string normalized = projectPath.Replace('\\', '/');
      return normalized.Contains(PackagedSampleMarker, StringComparison.OrdinalIgnoreCase);
    }
  }
}
