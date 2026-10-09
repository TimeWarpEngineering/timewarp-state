#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property NoWarn=CA2007
#:property RunAnalyzers=false

#region Purpose
// Git post-commit hook: re-indexes memsearch and attests the new tree with ganda.
#endregion

#region Design
// Runs ganda memsearch index-repo --background, then ganda repo attest unless GANDA_ATTEST_HOOK=0.
// Both run with no validation, so failures are ignored and the hook always returns 0.
#endregion

// Unified dispatcher: memsearch (best-effort) + ganda repo attest.
// Exit 0 always — commit already landed.
using TimeWarp.Amuru;

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
