# Round 4 — general (re-verification)
**Date:** 2026-09-03
**Scope reviewed:** fix commit 2d9fec36 vs round-3 ledger M12–M16

## Summary

All five round-3 findings (M12–M16) are verified fixed in 2d9fec36; each disposition note matches
what the code actually does, and no regression was found in the delta. The three behaviours called
out for scrutiny were checked empirically, not just by reading: the `bool.TryParse` default-true
expression yields true for unset/""/garbage and false only for a parseable "false"; `RunStep` exits 1
on both a thrown exception and a non-zero return and falls through on 0; `WaitForExit()` on an
already-exited process with live async readers returns in 0 ms after the handlers have drained, and
a second call does not throw. `CI == "true"` matches the value GitHub Actions sets, and skipping the
NuGet locals clear cannot shadow freshly built code because every test project uses ProjectReference,
not a PackageReference into `artifacts/packages`. All five runfiles compile (`dotnet build ./scripts/*.cs`);
the only diagnostic is the known pre-existing CS0219 for `analyzersDirectory` in build.cs. One nit is
open: the fix guarded the locals clear in clean.cs and package.cs but left the identical unguarded
clear in build.cs's `clean` route, which the same commit otherwise edited.

## Prior findings

| ID | Severity | Status | Round-4 verdict |
|----|----------|--------|-----------------|
| M12 | suggestion | fixed | verified: e2e.cs:32-33 reads `UseHttp` via `bool.TryParse` with default-true; workflow.yml:99-100 sets `UseHttp: "true"` on the E2E step, so the env now drives the branch. Truth table confirmed by execution — unset/""/"garbage"/"1"/"0" → true, "false"/"FALSE" → false, whitespace tolerated. The `InstallLinuxDevCerts` comment no longer asserts the run is http. |
| M13 | suggestion | fixed | verified: `RunStep(name, step)` at test.cs:79-100 prints `==> name`, catches on the throw path and checks the return path, writes `name failed: …` to stderr, and calls `Environment.Exit(1)`. Exercised in a standalone harness: exit 1 with a named suite for both a thrown exception and a non-zero return, exit 0 on success. Ten call sites replaced, order and project paths unchanged. CliWrap 3.9.0's `CommandExecutionException` first line carries the process name and exit code, so the `Split('\n')[0]` truncation keeps the diagnostic content. |
| M14 | suggestion | fixed | verified: clean.cs:27-34 and package.cs:41-49 skip `nuget locals all --clear` when `CI == "true"`, the exact value GitHub Actions sets, and log the skip. Both jobs that run these scripts restore `actions/cache` first (workflow.yml:61 for `ci`, :184 for `release`), so the disposition's rationale for extending the guard to package.cs holds. No stale-package regression: all test projects ProjectReference the source projects, so nothing resolves TimeWarp.State out of `~/.nuget/packages`. |
| M15 | nit | fixed | verified: package.cs:21-23 has the guard, and nothing later in `PackageNuGets` deletes `./artifacts`, so it survives to the child `dotnet` calls; build.cs:113-115 recreates `artifacts/packages` after `Directory.Delete(artifactsDirectory)`. All five runfiles now carry the guard plus the clarified comment ("protects the child `dotnet` invocations … not this runfile's own `#:package` restore"). |
| M16 | nit | fixed | verified: e2e.cs:460-467 keeps `Kill()` inside `if (!HasExited)` and hoists the single `WaitForExit()` below it, so both paths drain before the writers are disposed. No hang risk: the Auto-mode SUT is launched as the built executable directly (e2e.cs:334-342), not `dotnet run`, so no grandchild holds the redirected pipe, and `KillSut` only runs for Auto mode with a non-null process. Reproduced the exited-process case: `WaitForExit()` returned immediately with all output already flushed, and a repeat call was a no-op. |

## Issues

### Issue 1 — Severity: nit
- File: scripts/build.cs:96
- Description: The M14 guard was applied to clean.cs and package.cs, but `build.cs`'s `clean` route still calls `DotNet.NuGet().Locals().Clear(NuGetCacheType.All)` unconditionally — fifteen lines above the `Directory.CreateDirectory(packagesDirectory)` line this same commit added, so the function was open in front of the fixer. It is not a live defect today: workflow.yml only invokes `build.cs` with no args (the default `build` route), so the unguarded clear is never reached in CI. It is a latent inconsistency — wiring `dotnet run --file ./scripts/build.cs clean` into any cached job would silently reintroduce M14.
- Suggestion: apply the same `Environment.GetEnvironmentVariable("CI") == "true"` guard to build.cs's `CleanSolution`, so all three cache-clearing entry points behave identically.
- Status: open

