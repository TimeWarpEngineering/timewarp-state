#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007;CA1849;RS0030
#:property RunAnalyzers=false

// Refuse pushes that update home branches (master/main) while HEAD is master or main.
// Allow other dests (feature/*, etc.) so origin-home can publish a missing --into ref.
// Allow refs/tags/* (release tags) and refs/ganda/* (claims CAS) when every dest is one of those.
// Mixed tag + branch batches stay refused while HEAD is home.
// Commits on master stay blocked by pre-commit. Escape hatch: git push --no-verify
using TimeWarp.Amuru;

string? root = Git.FindRoot();
if (root is null)
{
  return 0;
}

// stdin: <local ref> <local sha> <remote ref> <remote sha> (one line per dest)
List<string> remoteRefs = [];
string? line;
while ((line = Console.In.ReadLine()) is not null)
{
  if (string.IsNullOrWhiteSpace(line))
    continue;

  string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
  if (parts.Length >= 3)
    remoteRefs.Add(parts[2]);
}

if (remoteRefs.Count > 0 && remoteRefs.All(IsExemptDest))
{
  return 0;
}

CommandOutput revParse = await Shell.Builder("git")
  .WithArguments("rev-parse", "--abbrev-ref", "HEAD")
  .WithWorkingDirectory(root)
  .WithNoValidation()
  .CaptureAsync();

string branch = revParse.Stdout.Trim();
if (branch is "master" or "main")
{
  bool updatesHome = remoteRefs.Count == 0
    || remoteRefs.Any(IsHomeBranchDest);
  bool mixesTagAndBranch = remoteRefs.Any(IsTagDest) && remoteRefs.Any(IsBranchDest);
  if (updatesHome || mixesTagAndBranch)
  {
#pragma warning disable RS0030, CA1849 // hook runfile: stderr to git, no ITerminal host
    Console.Error.WriteLine($"Refusing push while HEAD is '{branch}'.");
    Console.Error.WriteLine("Do not push from master/main — use a feature worktree/branch (e.g. ganda worktree / task branch).");
    Console.Error.WriteLine("Escape hatch (intentional only): git push --no-verify");
#pragma warning restore RS0030, CA1849
    return 1;
  }
}

return 0;

static bool IsExemptDest(string remoteRef) =>
  IsTagDest(remoteRef)
  || remoteRef.StartsWith("refs/ganda/", StringComparison.Ordinal);

static bool IsTagDest(string remoteRef) =>
  remoteRef.StartsWith("refs/tags/", StringComparison.Ordinal);

static bool IsBranchDest(string remoteRef) =>
  remoteRef.StartsWith("refs/heads/", StringComparison.Ordinal);

static bool IsHomeBranchDest(string remoteRef) =>
  remoteRef is "refs/heads/master" or "refs/heads/main";
