#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007
#:property RunAnalyzers=false

#region Purpose
// Git post-checkout hook: re-indexes memsearch and re-attests the tree after a branch checkout.
#endregion

#region Design
// Returns 0 at once unless the third argument (branch flag) is 1, so file checkouts do nothing.
// Runs ganda memsearch index-repo --background, then ganda repo attest unless GANDA_ATTEST_HOOK=0.
// Both run with no validation, so failures are ignored and the hook always returns 0.
#endregion

// Unified dispatcher for branch checkouts only ($3 == 1).
// Exit 0 always.
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
  return 0;
}

await Shell.Builder("ganda")
  .WithArguments("memsearch", "index-repo", "--background")
  .WithWorkingDirectory(root)
  .WithNoValidation()
  .RunAsync();

if (!string.Equals(Environment.GetEnvironmentVariable("GANDA_ATTEST_HOOK"), "0", StringComparison.Ordinal))
{
  await Shell.Builder("ganda")
    .WithArguments("repo", "attest")
    .WithWorkingDirectory(root)
    .WithNoValidation()
    .RunAsync();
}

return 0;
