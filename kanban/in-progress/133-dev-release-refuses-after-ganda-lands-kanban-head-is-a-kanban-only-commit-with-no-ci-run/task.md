# dev release refuses after ganda lands kanban: HEAD is a kanban-only commit with no CI run

## Description

Since ganda started landing the kanban move as its own commit after a PR merge (`land kanban <id>`,
seen first with task 132 on 2026-10-31), master HEAD right after a merge is a commit that touches only
`kanban/**`. The CI workflow's `push` trigger has a `paths:` filter that excludes `kanban/**`, so no
`workflow.yml` run exists for that commit. `dev release --dry-run` then refuses:

```
Error: no successful CI run of workflow.yml exists for e05df145… The Release event would fail at
locate-run — wait for CI on this commit or rerun it (gh run rerun <id>).
```

The merge commit one below (`d120f3a0`) has a green run and byte-identical product content. The
operator remedy was `gh workflow run workflow.yml --ref master` (dispatch, merge mode) to create a run
at HEAD, then `dev release`. That costs a full CI cycle for no new code and will happen after every
merge from now on.

Filed by Claude (cockpit) at Steven's request, 2026-10-31.

## Requirements

Pick one and record it in `## Notes` (the first is preferred; the second is the fallback if the
pipeline's own `locate-run` cannot be changed safely):

1. **Release guard and `locate-run` accept an ancestor run when the diff to HEAD is kanban-only.**
   In `dev release` (tools/dev-cli) and in the pipeline's locate-run step: walk back from HEAD while
   `git diff --name-only <commit>^ <commit>` is entirely under `kanban/**`; the first commit with a
   successful `workflow.yml` run is the artifact source. Record that commit in the dry-run output and
   in the Release notes body. The tag still goes on HEAD. Add a test with a fake run list.
2. **Trigger CI on the land commit.** Either ganda lands kanban before the merge commit (so the merge
   commit is HEAD; that is a timewarp-ganda change — file it there and link it here), or the workflow's
   push `paths:` filter gains `kanban/**` (wasteful: a full build per board move).

Either way:

- `documentation/developer/guides/releasing.md` explains the rule.
- `dev release --dry-run` prints which commit's artifact will be promoted when it differs from HEAD.
- Version is unaffected; no bump.

## Goal (implementer restatement)

After ganda lands the kanban move as its own commit, master HEAD changes only `kanban/**`, and the
`workflow.yml` push filter skips it, so HEAD has no CI run. A release from that HEAD must still work
without dispatching a fresh CI run for unchanged product code.

Acceptance criteria:

- `dev release` (guard + `--dry-run`) accepts a HEAD that is only kanban-only commits past the newest
  green first-parent commit, and the dry run names the commit whose artifact ships when it is not HEAD.
- The pipeline's locate-run step (`dev workflow`, release mode) does the same walk: it promotes the
  artifact of the newest green commit when every commit after it touches only `kanban/`, and refuses
  when a commit in between touches anything else. A test drives that walk with a fake run list.
- A releasing guide explains the rule. Version is unchanged (no bump).
- `ganda repo audit` clean; build and tests green.

## Checklist

- [x] Direction recorded
- [x] Guard + locate-run (or trigger) change with tests
- [x] releasing.md updated; dry-run prints the artifact commit
- [x] Manual: merge a kanban-only landing on a throwaway branch or replay 2026-10-31 and confirm the dry run passes
- [x] `ganda repo audit` clean; build, test green

## Session

- Created: cockpit (2026-10-31)
- 2026-11-06 implementer (Claude, ganda task work): found that `dev release` ships from
  TimeWarp.Nuru.DevCli, and 4.0.0-beta.11 (pinned since task 134) already has the kanban-only walk (Nuru
  task 543). Added the same walk to the repo-local locate-run, with tests. Wrote the releasing guide.
  Ran a live dry run on origin-home master.

## Notes

- **Direction: option 1** (accept an ancestor run when the commits after it are kanban-only).
  - The `dev release` guard is not repo code. It is `release-command.cs` in TimeWarp.Nuru.DevCli, and
    4.0.0-beta.11 already does the walk (Nuru task 543). It walks first-parent history from HEAD (at most
    50 commits) to the newest commit with a green `workflow.yml` run. It refuses at the first commit that
    changes a path outside `kanban/`. It re-reads `<Version>` at the target. `--dry-run` prints
    `HEAD is N kanban-only commit(s) past <sha>` and `Target commit:`. No change was needed there.
  - **Deviation from the requirement text: the tag goes on the release target, not on HEAD.** That is
    the upstream choice. The tag names the commit whose artifact was tested and shipped. The release run
    checks out the tag, so its locate-run finds the run directly, and the tag-pin check (tag == HEAD)
    holds without a change. `--generate-notes` builds the Release notes from the tag. Because the tag
    is the artifact commit, the notes body does not need a separate line naming it.
  - Repo-local locate-run (`dev workflow`, release mode) now runs the same walk
    (`tools/dev-cli/endpoints/workflow/artifact-source-walk.cs`). It reuses the package's
    `CiRunPromotion.OrderCandidateRuns`, `ReleaseGuard.FindShippingPath`, and
    `ReleaseGuard.SelectReleaseTarget`. This covers a break-glass dispatch from master when HEAD is a
    land-kanban commit. When the target is not HEAD it prints `HEAD <sha> is N kanban-only commit(s)
    past <sha>; promoting the artifact built at <sha>`. The download and verify refusals name that commit.
  - Option 2 (a CI run per board move) was not needed.
- The guide lives at `documentation/dev-ops/releasing.md`, not `documentation/developer/guides/`.
  That tree does not exist here, and dev-ops is where this repo keeps its release docs. The stale release
  section in `dev-ops.md` (it named the old workflows) now points to the guide.
- Only `kanban/` is exempt. A commit that touches only `documentation/` or `analysis/` also gets no CI
  run from the push filter, but it still ends the walk. Widening the exemption belongs in TimeWarp.Nuru
  (543 kept it out of scope).

- Related: 119 (previous release flow, before land-kanban existed), timewarp-ganda land-kanban change
  (version `1.0.0-beta.38+a9a702ef`).
- Evidence: `~/.grok/long-running-background-tasks/release-132.log` ("FAILED no master CI run for
  e05df145"); `gh run list --branch master` shows the last push run at `d120f3a0`.

## Results

- `tools/dev-cli/endpoints/workflow/artifact-source-walk.cs` (new): `ArtifactSourceWalk.RunAsync`
  walks the first-parent shas one at a time through `ICommitCiLookup`. It stops at the first green
  commit or the first commit with a path outside `kanban/`. A green HEAD costs one gh call.
- `ci-artifact-service.cs`: `LocateCiRunAsync` runs `git rev-list --first-parent --max-count=50`. It
  then runs the walk with `GhGitCommitCiLookup`, which makes the same `gh run list` and
  `git diff --name-only --no-renames <sha>^1 <sha>` calls as `dev release`. `LocateRunOutcome` now
  carries `TargetSha` and `SkippedCommitCount`. A missing gh binary is still `GhUnavailable`, and a
  failed `gh run list` is still `GhFailed`.
- `release-pipeline.cs`: prints the artifact commit when it is not HEAD. The no-run refusal names the
  commit outside `kanban/` that ended the walk. The download and verify refusals name the artifact commit.
- Tests: `tests/timewarp-state-tests/release-pipeline/artifact-source-walk-tests.cs` has 7 Fixie cases
  that use a fake run list and fake changed paths: green HEAD (one lookup), the 2026-10-31 land-kanban
  shape, several kanban-only commits plus an empty commit, run ordering (push first, pull_request
  dropped, other shas dropped), a refusal on a shipping path, a refusal on a rename out of `source/`, and
  a refusal when the walk runs out of commits. The test project links the walk file and the four pure
  DevCli package files through `GeneratePathProperty` (`ExcludeAssets="all"`). The folder is not named
  `release/` because `.gitignore` ignores `[Rr]elease/`.
- Docs: `documentation/dev-ops/releasing.md` (new, in the dev-ops toc) and `dev-ops.md`.
- Version unchanged (12.0.0-beta.17).
- Gates: `ganda repo audit` passes. Its 2 advisory warnings were already there (claim 098 has no
  worktree; legacy non-kebab wwwroot/targets names), so nothing needed fixing. `./bin/dev build`
  succeeded with 0 errors. `./bin/dev test` is green: every suite passed (3 skips were already there),
  and that run includes the 7 new cases.
- The linked DevCli files raise CS8669 (nullable annotation outside a `#nullable` context) warnings in
  the test project. `dev-cli` already shows the same warnings for those files. They are package code,
  so nothing to fix here.
- Manual (2026-11-06): from the origin-home master (clean and in sync, HEAD `c8908d6` "land kanban
  146"), `dotnet run --file tools/dev-cli/dev.cs -- release --dry-run` passed every guard and printed
  `✓ HEAD is 6 kanban-only commit(s) past 640ea95 (CI run 42162651594)` and `Target commit: 640ea95`.
  A gh/git replay of the walk over that history matches: six no-run commits with no shipping path, then
  green `640ea951`. The original `e05df145` now has the operator's dispatch run, so a replay there stops
  at HEAD.

### How to validate

Smoke:

```bash
# from the task worktree
(cd tests/timewarp-state-tests && dotnet fixie timewarp-state-tests --tests "ArtifactSourceWalkTests*")
# from a clean, synced master checkout whose HEAD is a land-kanban commit
dotnet run --file tools/dev-cli/dev.cs -- release --dry-run
```

Expect:

- Fixie reports `7 passed`.
- The dry run reports `✓ HEAD is N kanban-only commit(s) past <sha> (CI run <id>)` and
  `Target commit: <sha>`, and it ends with `All guards passed.` It needs no `gh workflow run` dispatch.
  (If `<Version>` is already tagged or published, the dry run refuses earlier at the tag or
  publish-state guard. That is expected.)

