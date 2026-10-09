#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007;CA1849;RS0030
#:property RunAnalyzers=false

// Refuse commits while HEAD is master or main.
// Escape hatch: git commit --no-verify
using TimeWarp.Amuru;

string? root = Git.FindRoot();
if (root is null)
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
#pragma warning disable RS0030, CA1849 // hook runfile: stderr to git, no ITerminal host
  Console.Error.WriteLine($"Refusing commit on '{branch}'.");
  Console.Error.WriteLine("Do not commit on master/main — use a feature worktree/branch (e.g. ganda worktree / task branch).");
  Console.Error.WriteLine("Escape hatch (intentional only): git commit --no-verify");
#pragma warning restore RS0030, CA1849
  return 1;
}

return 0;
