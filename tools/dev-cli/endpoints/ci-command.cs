#region Purpose
// `dev ci`: run the PR pipeline locally, then publish the packed nupkgs and an unsigned CI note for HEAD's tree
// so GitHub Actions can skip the pipeline on that same tree.
#endregion

#region Design
// Stages are PrPipeline, the same list `dev workflow` runs on a pull request (no build classifier: a local
// run always builds). The note is timewarp-ganda 660's plain JSON note on refs/notes/ganda-ci, keyed by
// HEAD^{tree}, written with `ganda repo ci-note write --from`. No key and no signature (Steven 2026-11-17,
// task 150 override): anyone who can push can edit the workflow, so a signature protects nothing.
// Order: the clean-tree check, the stages, a second clean-tree check (a stage that rewrote a tracked file
// would leave a note for a tree nobody built), the package ref, then the note. The package ref is pushed
// before the note so a note never points at packages the remote lacks.
// Package ref: refs/ci-packages/<tree> is a parentless commit whose tree holds the *.nupkg files the
// Packages-* upload takes (not the .snupkg files). It is built with git plumbing (hash-object, mktree,
// commit-tree) so the worktree index is never touched, then force-pushed. The gate checks every sha256
// against the note.
// Exit codes follow timewarp-ganda local-ci.md, because task-work runs this as the repo's localCi:
// 0 published; 1 a stage failed or the tree is dirty (nothing to cover); 2 the stages passed but the
// package ref or the note could not be published (ganda missing, auth, network). Exit 2 is a warning to
// the walk; the pull request then runs the full pipeline.
// The --from file goes under artifacts/ci (ignored), because ci-note write refuses untracked files.
// why: task 150, RFC resolutions 3-5 under the no-signing override (2026-11-18)
#endregion

namespace DevCli.Commands;

[NuruRoute("ci", Description = "Run the full PR pipeline locally, then publish the packages and the CI note so GitHub CI skips this tree")]
internal sealed class CiCommand : ICommand<Unit>
{
  internal const int StageFailedExit = 1;
  internal const int PublishFailedExit = 2;
  internal const string PackageRefPrefix = "refs/ci-packages/";
  private const string PlaywrightPackageId = "Microsoft.Playwright.MSTest.v4";

  internal sealed class Handler : IRequestHandler<CiCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private readonly PipelineConsole Output;
    private readonly IRepoCleanService RepoCleanService;
    private readonly IPackableProjectService PackableProjectService;

    public Handler(ITerminal terminal, IRepoCleanService repoCleanService, IPackableProjectService packableProjectService)
    {
      Terminal = terminal;
      Output = new PipelineConsole(terminal);
      RepoCleanService = repoCleanService;
      PackableProjectService = packableProjectService;
    }

    public async ValueTask<Unit> Handle(CiCommand command, CancellationToken ct)
    {
      Environment.ExitCode = await RunAsync(ct);
      return Unit.Value;
    }

    private async Task<int> RunAsync(CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        return StageFailedExit;
      }

      string? dirty = await DirtyStatusAsync(repoRoot, ct);
      if (dirty is not null)
      {
        Terminal.WriteErrorLine($"dev ci: the worktree is not clean. The note covers HEAD's tree, so commit first.\n{dirty}".Red());
        return StageFailedExit;
      }

      string tree = await GitAsync(repoRoot, ct, "rev-parse", "HEAD^{tree}");
      string head = await GitAsync(repoRoot, ct, "rev-parse", "HEAD");
      Terminal.WriteLine($"\ndev ci — HEAD {head}, tree {tree}\n".Cyan());

      PipelineResult result = await new PrPipeline(Output, RepoCleanService, PackableProjectService).RunAsync(repoRoot, ct);
      if (!result.Succeeded)
      {
        Terminal.WriteErrorLine($"dev ci: {result.FailureReason}. No CI note written.".Red());
        return StageFailedExit;
      }

      if (await GitAsync(repoRoot, ct, "rev-parse", "HEAD") != head)
      {
        Terminal.WriteErrorLine("dev ci: HEAD moved during the run. No CI note written.".Red());
        return StageFailedExit;
      }

      dirty = await DirtyStatusAsync(repoRoot, ct);
      if (dirty is not null)
      {
        Terminal.WriteErrorLine($"dev ci: the run changed files git tracks, so it did not build tree {tree}. No CI note written.\n{dirty}".Red());
        return StageFailedExit;
      }

      string packagesDir = Path.Combine(repoRoot, "artifacts", "packages");
      string[] nupkgs = Directory.Exists(packagesDir)
        ? [.. Directory.GetFiles(packagesDir, "*.nupkg").Where(path => path.EndsWith(".nupkg", StringComparison.Ordinal)).Order(StringComparer.Ordinal)]
        : [];
      if (nupkgs.Length == 0)
      {
        Terminal.WriteErrorLine($"dev ci: verify-samples passed but {packagesDir} has no *.nupkg. No CI note written.".Red());
        return StageFailedExit;
      }

      List<CiNotePackage> packages = [.. nupkgs.Select(path => new CiNotePackage(Path.GetFileName(path), CiNoteInput.Sha256Hex(path)))];
      Dictionary<string, string> tools = await ReadToolsAsync(repoRoot, ct);
      foreach (KeyValuePair<string, string> tool in tools.OrderBy(pair => pair.Key, StringComparer.Ordinal))
      {
        Terminal.WriteLine($"  {tool.Key}: {tool.Value}");
      }

      if (!await PushPackageRefAsync(repoRoot, tree, nupkgs, ct))
      {
        return PublishFailedExit;
      }

      return await WriteNoteAsync(repoRoot, tools, packages, ct);
    }

    private async Task<Dictionary<string, string>> ReadToolsAsync(string repoRoot, CancellationToken ct)
    {
      CommandOutput sdk = await Shell.Builder("dotnet")
        .WithArguments("--version")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);

      Dictionary<string, string> tools = new(StringComparer.Ordinal)
      {
        [CiNoteInput.SdkTool] = sdk.ExitCode == 0 ? sdk.Stdout.Trim() : "unknown",
        [CiNoteInput.RidTool] = CiNoteInput.CurrentRid(),
        [CiNoteInput.PlaywrightTool] = ReadPlaywrightVersion(repoRoot) ?? "unknown",
        [CiNoteInput.ChromiumTool] = ReadChromiumRevision(repoRoot) ?? "unknown"
      };
      return tools;
    }

    private static string? ReadPlaywrightVersion(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "Directory.Packages.props");
      if (!File.Exists(propsPath))
      {
        return null;
      }

      return XDocument.Load(propsPath)
        .Descendants()
        .Where(element => element.Name.LocalName == "PackageVersion"
          && (string?)element.Attribute("Include") == PlaywrightPackageId)
        .Select(element => (string?)element.Attribute("Version"))
        .FirstOrDefault();
    }

    private static string? ReadChromiumRevision(string repoRoot)
    {
      string binDir = Path.Combine(repoRoot, "tests", "test-app-end-to-end-tests", "bin");
      if (!Directory.Exists(binDir))
      {
        return null;
      }

      string? browsersJson = Directory.EnumerateFiles(binDir, "browsers.json", SearchOption.AllDirectories)
        .Where(path => path.Contains(Path.Combine(".playwright", "package"), StringComparison.Ordinal))
        .OrderByDescending(File.GetLastWriteTimeUtc)
        .FirstOrDefault();
      return browsersJson is null ? null : CiNoteInput.ReadChromiumRevision(File.ReadAllText(browsersJson));
    }

    private async Task<bool> PushPackageRefAsync(string repoRoot, string tree, string[] nupkgs, CancellationToken ct)
    {
      string packageRef = PackageRefPrefix + tree;
      try
      {
        List<string> entries = [];
        foreach (string path in nupkgs)
        {
          string blob = await GitAsync(repoRoot, ct, "hash-object", "-w", "--", path);
          entries.Add($"100644 blob {blob}\t{Path.GetFileName(path)}");
        }

        CommandOutput mktree = await Shell.Builder("git")
          .WithArguments("mktree")
          .WithWorkingDirectory(repoRoot)
          .WithStandardInput(string.Join('\n', entries) + "\n")
          .WithNoValidation()
          .CaptureAsync(ct);
        if (mktree.ExitCode != 0)
        {
          throw new InvalidOperationException($"git mktree failed: {mktree.Stderr.Trim()}");
        }

        string commit = await GitAsync(
          repoRoot,
          ct,
          "-c", "user.name=dev ci",
          "-c", "user.email=dev-ci@timewarp.invalid",
          "commit-tree", mktree.Stdout.Trim(),
          "-m", $"dev ci packages for tree {tree}");

        Terminal.WriteLine($"Pushing {nupkgs.Length} package(s) to {packageRef}...");
        CommandOutput push = await Shell.Builder("git")
          .WithArguments("push", "--force", "--quiet", "origin", $"{commit}:{packageRef}")
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .CaptureAsync(ct);
        if (push.ExitCode != 0)
        {
          throw new InvalidOperationException($"git push {packageRef} failed: {push.Stderr.Trim()}");
        }

        return true;
      }
      catch (InvalidOperationException exception)
      {
        Terminal.WriteErrorLine($"dev ci warning: stages passed, but the package ref was not published ({exception.Message}). GitHub CI will run in full.".Yellow());
        return false;
      }
    }

    private async Task<int> WriteNoteAsync(
      string repoRoot,
      IReadOnlyDictionary<string, string> tools,
      IReadOnlyList<CiNotePackage> packages,
      CancellationToken ct)
    {
      string ciDir = Path.Combine(repoRoot, "artifacts", "ci");
      Directory.CreateDirectory(ciDir);
      string inputPath = Path.Combine(ciDir, "ci-note-input.json");
      await File.WriteAllTextAsync(inputPath, CiNoteInput.BuildJson(tools, packages), ct);

      int exitCode;
      try
      {
        exitCode = await Shell.Builder("ganda")
          .WithArguments("repo", "ci-note", "write", "--from", inputPath)
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync(ct);
      }
      catch (Exception exception) when (exception is not OperationCanceledException)
      {
        Terminal.WriteErrorLine($"dev ci warning: stages passed, but ganda could not start ({exception.Message}). GitHub CI will run in full.".Yellow());
        return PublishFailedExit;
      }

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine($"dev ci warning: stages passed, but `ganda repo ci-note write` exited {exitCode}. GitHub CI will run in full.".Yellow());
        return PublishFailedExit;
      }

      Terminal.WriteLine("\ndev ci: stages passed; CI note and packages published. GitHub CI takes the fast path on this tree.".Green());
      return 0;
    }

    private static async Task<string?> DirtyStatusAsync(string repoRoot, CancellationToken ct)
    {
      string status = await GitAsync(repoRoot, ct, "status", "--porcelain");
      return status.Length == 0 ? null : status;
    }

    private static async Task<string> GitAsync(string repoRoot, CancellationToken ct, params string[] arguments)
    {
      CommandOutput output = await Shell.Builder("git")
        .WithArguments(arguments)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);
      if (output.ExitCode != 0)
      {
        throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed: {output.Stderr.Trim()}");
      }

      return output.Stdout.Trim();
    }
  }
}
