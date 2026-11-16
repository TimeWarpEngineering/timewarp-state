#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007;CA1849;RS0030
#:property RunAnalyzers=false

#region Purpose
// Stop a commit when HEAD is master or main, so the commit happens on a task branch.
// git commit --no-verify is the only way past the refusal.
#endregion

// Refuse commits while HEAD is master or main.
// Escape hatch: git commit --no-verify
using TimeWarp.Amuru;

string? root = Git.FindRoot();
if (root is null)
{
  Console.Error.WriteLine("Refusing commit: repository root was not found.");
  return 1;
}

CommandOutput revParse = await Shell.Builder("git")
  .WithArguments("rev-parse", "--abbrev-ref", "HEAD")
  .WithWorkingDirectory(root)
  .WithNoValidation()
  .CaptureAsync();

string branch = revParse.Stdout.Trim();
if (branch is "master" or "main")
{
#pragma warning disable RS0030, CA1849 // hook runfile: stderr to git, no ITerminal host
  Console.Error.WriteLine($"Refusing commit on '{branch}'.");
  Console.Error.WriteLine("Do not commit on master/main — use a feature worktree/branch (e.g. ganda worktree / task branch).");
  Console.Error.WriteLine("Escape hatch (intentional only): git commit --no-verify");
#pragma warning restore RS0030, CA1849
  return 1;
}

return 0;
