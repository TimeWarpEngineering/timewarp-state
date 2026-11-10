#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007
#:property RunAnalyzers=false

// Dispatcher for branch checkouts only ($3 == 1): ganda repo attest.
// A non-zero exit becomes the exit status of git checkout, git switch, and
// git worktree add. Report a missing root, a failed attest, or a ganda
// binary that cannot be started on stderr, and still exit 0.
using TimeWarp.Amuru;

// git post-checkout <previous> <new> <branch_flag>
// branch_flag is 1 for branch checkout, 0 for file checkout.
if (args.Length < 3 || args[2] != "1")
{
  return 0;
}

string? root = Git.FindRoot();
if (root is null)
{
  Console.Error.WriteLine("post-checkout: repository root was not found; skipped ganda repo attest.");
  return 0;
}

if (!string.Equals(Environment.GetEnvironmentVariable("GANDA_ATTEST_HOOK"), "0", StringComparison.Ordinal))
{
  try
  {
    int exitCode = await Shell.Builder("ganda")
      .WithArguments("repo", "attest")
      .WithWorkingDirectory(root)
      .WithNoValidation()
      .RunAsync();
    if (exitCode != 0)
    {
      Console.Error.WriteLine($"post-checkout: ganda repo attest failed (exit {exitCode}).");
    }
  }
  catch (System.ComponentModel.Win32Exception exception)
  {
    Console.Error.WriteLine($"post-checkout: ganda repo attest could not start ({exception.Message}).");
  }
}

return 0;
