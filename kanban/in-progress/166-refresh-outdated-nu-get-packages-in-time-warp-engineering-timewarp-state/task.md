# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-state.
Filter: (all)
Outdated:
  Microsoft.Playwright.MSTest.v4 1.65.0 -> 1.65.1 (dev-only: tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj)
Every outdated pin is dev-only. Bump them. Do not change <Version> in source/Directory.Build.props: the wave merges this task and does not release this repo.
Edit Directory.Packages.props in this repo only.

## Goal (implementer restatement)

Bump the one outdated dev-only pin, Microsoft.Playwright.MSTest.v4, from 1.65.0 to 1.65.1 in the root Directory.Packages.props.

Acceptance criteria:
- Directory.Packages.props pins Microsoft.Playwright.MSTest.v4 at 1.65.1.
- source/Directory.Build.props <Version> is unchanged (no release).
- `./bin/dev build` passes and `ganda repo audit` passes.

Resume pass (2026-11-19): local CI failed at `Run analyzer tests` because `fixie` was not found.
Find why, and fix it in the dev CLI so a fresh task worktree passes without manual steps.

Acceptance criteria (resume):
- Root cause named and reproduced.
- The dev CLI fixes it with no manual cache clearing; a test covers the fix.
- `dotnet fixie timewarp-state-tests` passes in this worktree after the fix.

## Session

- Minted by ganda task wave run 20261119T023451Z-133310
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-19T03:06:57Z
- Review oracle: general (grok, vendor xai, model grok-4.7), session `01a1f1c5-a8bc-7a21-a68b-0021ecab2b97`, effort 1, turn cap 80, clean — 2026-11-19
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1f1c5-a8bc-7a21-a68b-0021ecab2b97, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-19T03:10:44Z
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-19T04:20:08Z
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1f1c5-a8bc-7a21-a68b-0021ecab2b97, max-turns 120, artifacts disposition.md — 2026-11-19T04:20:09Z
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1f1c5-a8bc-7a21-a68b-0021ecab2b97, max-turns 120, artifacts disposition.md — 2026-11-19T04:20:09Z

## Results

- `Directory.Packages.props`: Microsoft.Playwright.MSTest.v4 1.65.0 -> 1.65.1 (dev-only, used by tests/test-app-end-to-end-tests).
- `source/Directory.Build.props` <Version> unchanged.

### Gates

- `./bin/dev build` — PASS (0 warnings, 0 errors).
- Test files: none added or edited. The only consumer is the Playwright E2E project (MSTest, not a Jaribu runfile; needs a running app); CI runs it.
- `ganda repo audit` — PASS with 1 advisory warning (`vscode-window-icon`: .vscode/settings.json missing). `ganda repo audit --fix` wrote the git-ignored local `.vscode/settings.json`; rerun reports "Repository passes all audit checks" (44 passed, 0 failed). No tracked file changed by the fix.

### Resume pass: stale tool resolver cache (2026-11-19)

- Root cause (reproduced): `~/.dotnet/toolResolverCache/1/*` named tool paths under the gc'd
  task-164 worktree's `artifacts/nuget-packages`. That SDK cache is machine-wide and keyed by tool
  id and version only, and `dotnet tool restore` does not rewrite an entry it finds, so
  `dotnet tool run fixie` failed in every other worktree. Not a product or package-bump failure.
- Fix: `NuGetPackageIsolation.Apply` (`tools/dev-cli/endpoints/nuget-package-isolation.cs`, shared
  by the dev CLI and `scripts/*.cs`) now calls `PruneStaleToolResolverEntries`. It removes cache
  entries whose `PathToExecutable` no longer exists (rewrites the file with the live entries, or
  deletes it when none remain). Best effort: an unreadable file or folder is left alone. The next
  restore writes the entry for the current worktree.
- Tests: 5 new cases in `tests/timewarp-state-tests/packaging/nuget-package-isolation-tests.cs`
  against a temp cache folder (prune a gone path, keep a live path, keep live entries in a mixed
  file, leave invalid JSON alone, missing folder).
- Docs: `scripts/overview.md` isolation section.
- Verified: `self-install` pruned all 7 stale entries; `dotnet tool restore` then wrote fixie's
  entry for this worktree; `dotnet fixie timewarp-state-tests` ran.

### Gates (resume pass)

- `dotnet run --file tools/dev-cli/dev.cs -- self-install` — installed (dev CLI changed).
- `./bin/dev build` — PASS (0 warnings, 0 errors).
- `dotnet fixie timewarp-state-tests` (with `NUGET_PACKAGES=$PWD/artifacts/nuget-packages`, after `dotnet tool restore`) — PASS, 173 passed, 1 skipped, including the 5 new `NuGetPackageIsolation_Should_` cases. Fixie project, not a Jaribu runfile.
- `ganda repo audit` — PASS ("Repository passes all audit checks"), run once after the last product edit.

### How to validate

Resume smoke: point a fake entry at a missing path, e.g.
`echo '[{"Version":"3.4.0","PathToExecutable":"/nonexistent/Fixie.Console.dll"}]' > ~/.dotnet/toolResolverCache/1/fixie.console`,
then `./bin/dev test` (or `./bin/dev ci`).
Expect: the analyzer tests step finds `fixie`; the entry now names this worktree's `artifacts/nuget-packages`.

Smoke: `grep -n 'Microsoft.Playwright.MSTest.v4' Directory.Packages.props` then `./bin/dev build`.
Expect: the pin reads `Version="1.65.1"` and the build succeeds with 0 errors; CI's E2E job restores 1.65.1 and passes.

### Review

- Round 1, roster general, effort 1. Counts: bug 0/0/0, suggestion 0/0/0, nit 0/0/0 (open/fixed/wontfix).
- Disposition: clean. Final open count 0. No wontfix and no escalation. The goal restatement matches the original task (one listed pin, no `<Version>` change); no misread.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- Gates were not re-run in review. The implementer's recorded `./bin/dev build` and `ganda repo audit` stand.

## Review

- Round 1: Done

## Resume note (2026-11-19) — resolved

Resolved in the resume implement pass: stale `~/.dotnet/toolResolverCache` entries from the gc'd
task-164 worktree; the dev CLI now prunes them. See `### Resume pass` under `## Results`.


The walk parked at open-pr. Implement and review are done and clean (one dev-only pin,
Microsoft.Playwright.MSTest.v4 1.65.0 to 1.65.1). Local CI
(`dotnet run --file tools/dev-cli/dev.cs -- ci`) failed in the test step:

```
==> Run analyzer tests
Run "dotnet tool restore" to make the "fixie" command available.
Run analyzer tests failed: exit code 1
Pipeline FAILED — Test failed
```

What is known:

- `assert-version-ssot`, `clean` and `build` passed (0 errors), so task 165's single version works.
- `tools/dev-cli/endpoints/test-command.cs` already runs `dotnet tool restore` before
  `scripts/test.cs`, so this is not simply a missing restore. The `fixie` command was still not
  found when the analyzer tests ran.
- Tasks 164 and 165 passed the same pipeline earlier the same day. 164's own notes run fixie with
  `NUGET_PACKAGES=$PWD/artifacts/nuget-packages` set by hand, so the isolated package folder is a
  likely place to look (restore and run resolving tools from different folders, or `clean`
  removing what the restore wrote).

To resume: reproduce with `./bin/dev ci` in this worktree and find why `fixie` is not available
after the restore. If the pipeline has a real gap in a fresh worktree, fix it in the dev CLI so a
new worktree passes without manual steps; do not just rerun until green. If it does not
reproduce, record that here and let the walk continue from open-pr.

- Why: wave run 20261119T023451Z-133310 parked this task (Steve launched the libs wave 2026-11-19).

## Blocked

2026-11-19: review verdict refused: Done with no roster findings. Round folder: /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-state/task-166-refresh-outdated-nu-get-packages-in-time-warp/kanban/in-progress/166-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-state/review/round-2. Oracle log: /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-state/task-166-refresh-outdated-nu-get-packages-in-time-warp/kanban/in-progress/166-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-state/oracle-review-20261119t031044879z.log.
