#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007
#:property RunAnalyzers=false

#region Purpose
// After a commit lands, run ganda repo attest and write a missing root, a failed attest, or a ganda binary that will not start to stderr.
// The hook exits 0 in every case, because that status cannot undo the commit.
#endregion

// Dispatcher: ganda repo attest.
// Runs after the commit. Report a missing root, a failed attest, or a ganda
// binary that cannot be started on stderr and exit 0. The exit code cannot
// undo the commit.
using TimeWarp.Amuru;

string? root = Git.FindRoot();
if (root is null)
{
  Console.Error.WriteLine("post-commit: repository root was not found; skipped ganda repo attest.");
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
      Console.Error.WriteLine($"post-commit: ganda repo attest failed (exit {exitCode}).");
    }
  }
  catch (System.ComponentModel.Win32Exception exception)
  {
    Console.Error.WriteLine($"post-commit: ganda repo attest could not start ({exception.Message}).");
  }
}

return 0;
