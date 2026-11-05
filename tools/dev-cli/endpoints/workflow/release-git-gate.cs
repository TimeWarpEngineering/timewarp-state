#region Purpose
// Git checks that gate a release: the version tag pin and the "HEAD is on master" ancestor check.
#endregion

#region Design
// Every git call uses WithNoValidation and reads the exit code; failures come back as outcome statuses,
// never as exceptions, so ReleasePipeline owns the operator-facing message. The token reaches every call.
#endregion

namespace DevCli.Commands;

internal enum TagPinStatus
{
  NoTag,
  Match,
  Mismatch,
  GitError
}

internal sealed record TagPinOutcome(TagPinStatus Status, string? TagCommit, string? HeadCommit, string? Detail);

internal enum AncestorCheckStatus
{
  Ancestor,
  NotAncestor,
  MasterUnresolvable,
  GitError
}

internal sealed record AncestorCheckOutcome(AncestorCheckStatus Status, string? Detail);

internal sealed class ReleaseGitGate
{
  private readonly ITerminal Terminal;

  public ReleaseGitGate(ITerminal terminal)
  {
    Terminal = terminal;
  }

  public static async Task<TagPinOutcome> CheckTagPinAsync(string version, CancellationToken ct)
  {
    string tag = $"v{version}";

    CommandOutput verifyResult = await GitAsync(ct, "rev-parse", "-q", "--verify", $"refs/tags/{tag}");

    if (verifyResult.ExitCode != 0)
    {
      return string.IsNullOrWhiteSpace(verifyResult.Stderr)
        ? new TagPinOutcome(TagPinStatus.NoTag, null, null, null)
        : new TagPinOutcome(TagPinStatus.GitError, null, null, verifyResult.Stderr.Trim());
    }

    CommandOutput tagCommitResult = await GitAsync(ct, "rev-parse", $"{tag}^{{commit}}");

    if (tagCommitResult.ExitCode != 0)
    {
      return new TagPinOutcome(TagPinStatus.GitError, null, null, tagCommitResult.Stderr.Trim());
    }

    CommandOutput headResult = await GitAsync(ct, "rev-parse", "HEAD");

    if (headResult.ExitCode != 0)
    {
      return new TagPinOutcome(TagPinStatus.GitError, null, null, headResult.Stderr.Trim());
    }

    string tagCommit = tagCommitResult.Stdout.Trim();
    string headCommit = headResult.Stdout.Trim();

    TagPinStatus status = string.Equals(tagCommit, headCommit, StringComparison.Ordinal)
      ? TagPinStatus.Match
      : TagPinStatus.Mismatch;
    return new TagPinOutcome(status, tagCommit, headCommit, null);
  }

  public async Task<AncestorCheckOutcome> CheckHeadAncestorOfMasterAsync(CancellationToken ct)
  {
    string masterRef = "origin/master";

    CommandOutput verifyResult = await GitAsync(ct, "rev-parse", "--verify", masterRef);

    if (verifyResult.ExitCode != 0)
    {
      masterRef = "master";

      CommandOutput fallbackVerifyResult = await GitAsync(ct, "rev-parse", "--verify", masterRef);

      if (fallbackVerifyResult.ExitCode != 0)
      {
        return new AncestorCheckOutcome(AncestorCheckStatus.MasterUnresolvable, null);
      }

      Terminal.WriteLine("origin/master not found; using local master.");
    }

    CommandOutput ancestorResult = await GitAsync(ct, "merge-base", "--is-ancestor", "HEAD", masterRef);

    return ancestorResult.ExitCode switch
    {
      0 => new AncestorCheckOutcome(AncestorCheckStatus.Ancestor, null),
      1 => new AncestorCheckOutcome(AncestorCheckStatus.NotAncestor, null),
      _ => new AncestorCheckOutcome(AncestorCheckStatus.GitError, ancestorResult.Stderr.Trim())
    };
  }

  internal static Task<CommandOutput> GitAsync(CancellationToken ct, params string[] arguments) =>
    Shell.Builder("git")
      .WithArguments(arguments)
      .WithNoValidation()
      .CaptureAsync(ct);
}
