# CI fast-path gate: accept the attested PR-head tree when it differs from HEAD only under kanban/

## Description

Task 150 shipped the local-CI fast path: `dev ci` runs the pipeline locally, writes an unsigned note keyed by
HEAD^{tree} on `refs/notes/ganda-ci` (via `ganda repo ci-note write`), pushes the nupkgs to
`refs/ci-packages/<tree>`, and `scripts/ci-fast-path-gate.ps1` skips `Run CI Pipeline` when a note exists for the
checked-out tree.

First proof (PR #692, 2026-11-18) missed on both runs and the full pipeline ran (6m23s PR, ~6m master):

- PR run checks out `refs/pull/692/merge`; its tree `059ffb62…` had no note.
- Master push run checks out the merge commit; its tree `1554fc8b…` had no note.
- The note exists for the PR head tree `5b2d62f8…`. `git diff --stat 5b2d62f8 1554fc8b` is 10 files, 43 lines,
  **all under `kanban/`** (the task's rfc/, review/, task.md landed on master by `land kanban` commits).

Every ganda walk lands kanban commits on master before the merge, so the merge tree never equals the attested head
tree. The fast path is correct but never fires. This is the risk the 150 implementer recorded under `## Results`.

`ganda repo ci-note write` has no `--tree` option (it keys HEAD's tree), so fix this on the gate side without a
ganda change.

## Goal

The gate finds the attested tree through the commit graph and accepts it when the only difference from the
checked-out tree is under `kanban/`:

1. Candidate attested commits, in order: HEAD itself; for `pull_request`, the PR head
   (`github.event.pull_request.head.sha`, pass it in via `env`); for `push`, each parent of HEAD when HEAD is a
   merge commit (the PR head is the second parent). Stop at the first candidate whose tree has a note.
2. Equivalence check: compute each side's tree with the top-level `kanban` entry removed (`git ls-tree HEAD^{tree}`,
   drop the `kanban` line, `git mktree`), and require the two stripped trees to be equal. Anything outside `kanban/`
   that differs is a miss with a printed reason.
3. The rest of the gate is unchanged: stages, SDK and RID drift, package set and sha256 from
   `refs/ci-packages/<attested tree>`. Print which candidate hit (`HEAD`, `pr-head`, `parent N`).
4. Keep `dev ci` unchanged (still keyed by HEAD^{tree}). Update the Purpose/Design header of the gate script and the
   workflow step comment to describe the candidate walk and the kanban-only rule. Keep the gate PowerShell (the
   workflow-pwsh-shell audit check forbids bash steps) and keep it a script, not a runfile.
5. Unit tests: the dev CLI test project already has `CiNoteInputTests` that read the gate script; add a test that
   pins the stripped-tree rule (two trees differing only under `kanban/` are equivalent; a one-byte change under
   `source/` is not). A pwsh-level test is fine if the test project can shell out; otherwise test the C# mirror if
   one is extracted.

## Goal (implementer restatement)

The fast-path gate keys on the checked-out tree, but CI never checks out the tree `dev ci` attested: the PR run
checks out `refs/pull/N/merge` and the master run checks out the merge commit, and both carry kanban commits that
master landed after the branch was cut. Teach the gate to find the attested tree and accept it when the only
difference is the top-level `kanban` entry.

Acceptance:

- The gate tries candidates in order: HEAD, the PR head sha (passed in through `env` on `pull_request`), then each
  parent of HEAD when HEAD is a merge. Duplicates are skipped. The first candidate whose tree has a note and whose
  tree equals HEAD's tree once the top-level `kanban` entry is removed from both wins. A candidate with a note but a
  difference outside `kanban/` is rejected with a printed reason (the paths that differ) and the walk goes on, so a
  note on the base parent cannot hide a hit on the PR head. No winner is a miss that lists every candidate's reason.
- Stage, SDK, RID, package set, and sha256 checks are unchanged, and they read the attested tree's note and
  `refs/ci-packages/<attested tree>`. The hit line names the candidate (`HEAD`, `pr-head`, `parent N`).
- The stripped-tree rule lives in a small dot-sourced PowerShell helper so a unit test can run the real rule;
  the gate stays a script. `dev ci`, signing, ganda, and the note format are untouched.
- Script header and workflow step comment describe the candidate walk and the kanban-only rule.
- A unit test runs the helper under pwsh in a throwaway git repo: trees differing only under `kanban/` strip to
  the same tree; a one-byte change under `source/` does not.
- `dev build` clean, the edited test file green, `ganda repo audit` clean. The PR and master-push run evidence
  (hit lines, run ids, wall times) can only exist after open-pr and merge, so later nodes record it.

## Requirements

- Prove it: this task's own PR runs through `ganda task work`, which runs `dev ci` and writes the note before the
  push. The PR run must print `CI fast path: hit (pr-head …)` and skip `Run CI Pipeline`; the master push run must
  print a hit via the merge parent. Record both run ids and wall times under `## Results`. Target under 1 minute
  for each. If the PR run misses, say why and fix before merge.
- Also record one control: the previous miss (run 44318421138, 6m39s) so the saving is visible.
- `ganda repo audit --fix` run in this worktree and its changes committed (audit fixes are policy). If it bumps
  Amuru/Nuru pins again, keep the holds in `.editorconfig` that task 150 added (task 161 owns that migration).
- No changes to signing, to ganda, or to the note format (`{v, tree, stages, tools, packages}`).

## Checklist

- [x] Gate walks candidates: HEAD, PR head (pull_request), merge parents (push)
- [x] Stripped-tree equivalence (kanban/ only) implemented in pwsh with a printed reason on miss
- [x] Workflow passes the PR head sha to the gate via env
- [x] Script header and workflow comment updated
- [x] Unit test pins the kanban-only rule
- [x] `dev build` 0 warnings, `dev test` green, `ganda repo audit` clean
- [ ] PR run shows a hit and finishes under 1 minute (run id recorded)
- [ ] Master push run shows a hit (run id recorded)
- [ ] `## Results` filled in with before/after times

## Session

- Created: 3f3c256d-283c-4d41-ac7b-bb500c1a1415 (2026-11-18)
- Context: Steve 2026-11-18 "create the kanban-ignore task and launch it", after PR #692 proved the plumbing but
  missed on both runs.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session e165382e-6a76-4746-90b0-1508ca64a8f4, max-turns 500, Done — 2026-11-18T14:23:03Z
- Review oracle: general (grok, effort 2, roster general), session 01a1ef0a-4a93-77b2-83e2-f0a05143f29f, max-turns 120, open findings M1 M2 — 2026-11-18
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7), session 01a1ef0a-4a93-77b2-83e2-f0a05143f29f, max-turns 120, artifacts review/round-1/general.md, review/round-1/merged.md — 2026-11-18
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ef0a-4a93-77b2-83e2-f0a05143f29f, max-turns 120, artifacts general.md, merged.md — 2026-11-18T14:35:21Z

## Learnings

- Misread: Goal item 1 says stop at the first candidate whose tree has a note, and it limits merge parents to `push`. The restatement keeps walking after a noted candidate that differs outside `kanban/`, and it walks parents whenever HEAD is a merge, including a pull request. The script follows the restatement. That continuation is what the Requirements need: a note on parent 1 must not hide a hit on the PR head (parent 2) on the master push. The extra parent walk on a pull request still has to pass the same kanban-only check, and a duplicate tree is skipped. Not filed as a defect.

## Review

- Round 1: Blocked — open M1, M2
- Round 1 fix pass (implement-fix-2, 2026-11-18): M1 fixed (`--no-renames` on the reason diff, new test), M2 fixed
  (workflow banner, gate Purpose line, `documentation/dev-ops/local-ci.md`).

## Results

### What changed

- `scripts/ci-fast-path-tree.ps1` (new, dot-sourced): `Get-TreeWithoutKanban` (`git ls-tree`, drop the top-level
  `kanban` line, `git mktree`) and `Get-KanbanOnlyDifference` (returns `$null` when the stripped trees are equal,
  otherwise `differs outside kanban/: <first 5 paths>`).
- `scripts/ci-fast-path-gate.ps1`: walks candidates `HEAD`, `pr-head` (`$env:CI_FAST_PATH_PR_HEAD`), then
  `parent N` when HEAD is a merge; skips repeated trees. A candidate wins when its tree has a note and passes the
  kanban-only rule against HEAD's tree. A noted candidate that differs outside kanban/ is rejected with the paths
  and the walk continues (so a note on parent 1 cannot hide a hit on parent 2). No winner is one miss line listing
  every candidate's reason. Stage, SDK, RID, package set, and sha256 checks are unchanged and read the attested
  tree's note and `refs/ci-packages/<attested tree>`. Hit line: `CI fast path: hit (<label> <sha>). ...`.
  Purpose/Design header rewritten for the walk and the rule.
- `.github/workflows/workflow.yml`: fast-path step gets `env: CI_FAST_PATH_PR_HEAD:
  ${{ github.event.pull_request.head.sha }}` (empty on push); step comment describes the walk and the rule.
- `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs` (new): runs the real helper under pwsh
  in a throwaway git repo. Kanban-only change strips to the same tree with an empty reason; a one-byte change under
  `source/` does not (reason `differs outside kanban/: source/lib/a.cs`); a nested `source/kanban/` change does not.
  Test project gains `TimeWarp.Amuru` (central version, `ProcessStartInfo` is banned) and the global using that
  TW0007 asks for.
- `dev ci`, signing, ganda, and the note format are unchanged.
- Review round 1 fix pass: `scripts/ci-fast-path-tree.ps1` builds the miss reason from
  `git diff --name-only --no-renames`, so a move from `source/` into `kanban/` names the `source/` path (M1; the
  accept or reject decision is still the stripped tree ids). New test
  `Name_The_Source_Path_When_A_File_Moves_Into_Kanban`. The workflow banner, the gate's Purpose line, and
  `documentation/dev-ops/local-ci.md` now describe the candidate walk and the top-level `kanban` rule; the doc keeps
  the exact-tree rule for `dev ci` itself and corrects the SourceLink note (M2).

### Gates

- `./bin/dev build`: exit 0, 0 Warning(s), 0 Error(s).
- `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs`: not a standalone runfile (Fixie
  project, no `#if !JARIBU_MULTI` guard); ran the class alone with the runner `scripts/test.cs` uses:
  `dotnet fixie timewarp-state-tests --configuration Release --no-build --tests "*CiFastPathTreeTests*"` → 3 passed.
- `tests/timewarp-state-tests/release-pipeline/ci-note-input-tests.cs` (reads the gate script's stage line, which
  this change kept): same runner, `--tests "*CiNoteInputTests*"` → 6 passed.
- Fix pass (round 1 M1, M2): `./bin/dev build` → exit 0, 0 Error(s). `dotnet fixie timewarp-state-tests
  --configuration Release --no-build --tests "*CiFastPathTreeTests*"` → 4 passed (adds the rename case).
  `ganda repo audit` → 1 failure, `kanban-landed` (task 161's live kitchen differed from origin-home, not this
  task's change); `ganda repo audit --fix` landed kitchen 161 onto origin-home, no tracked change here; the
  re-audit in that run shows no failures.
- `ganda repo audit`: passes, 1 advisory (`vscode-window-icon`, `.vscode/settings.json` missing).
  `ganda repo audit --fix` wrote only the gitignored `.vscode/settings.json`; no tracked change, no pin bumps.

### Gate dry run on real notes (scratch clone, gate scripts from this branch)

- Detached at `8139649c` (the PR #692 merge; tree `059ffb62…`, the same tree run 44318421138 missed on), no env:
  `attested tree 5b2d62f8… from parent 2 da11d7ff…`, `hit (parent 2 da11d7ff…)`, 9 packages restored and
  sha-checked, `fast_path=true`, 5.4 s wall.
- Same commit with `CI_FAST_PATH_PR_HEAD=da11d7ff…`: `hit (pr-head da11d7ff…)`.
- A commit on top that edits `readme.md`, with the same PR head: `miss (HEAD …: no CI note for tree 0421eb30…;
  pr-head da11d7ff…: attested tree 5b2d62f8… differs outside kanban/: readme.md, scripts/ci-fast-path-gate.ps1)`,
  `artifacts/packages` cleared.

### CI proof (pending, after open-pr and merge)

- Control: PR #692 run 44318421138 (pull_request) missed, 6m39s; master push run 44319655507 ran the full
  pipeline (~6m).
- This PR's pull_request run: expect `CI fast path: hit (pr-head …)` and `Run CI Pipeline` skipped. Run id and wall
  time: to record.
- Master push run after merge: expect `CI fast path: hit (parent 2 …)`. Run id and wall time: to record.

### How to validate

Smoke: in a scratch clone with `origin` set to GitHub, `git checkout --detach 8139649c`, copy
`scripts/ci-fast-path-gate.ps1` and `scripts/ci-fast-path-tree.ps1` from this branch, run
`pwsh -NoProfile -File scripts/ci-fast-path-gate.ps1`. Then check this PR's `CI` job log for the `Local CI fast
path` step.
Expect: `CI fast path: hit (parent 2 da11d7ff…)` and `fast_path=true` with 9 nupkgs in `artifacts/packages`; on the
PR run, `CI fast path: hit (pr-head …)` and `Run CI Pipeline` skipped.

## Notes

- Evidence: PR #692 run 44318421138 (pull_request, 6m39s, gate printed `miss (no CI note for tree 059ffb62…)`),
  run 44319655507 (master push, full pipeline). Notes on `refs/notes/ganda-ci`: trees `40f17485…` and `5b2d62f8…`.
- Task 150 RFC decision 4 (branch and merge policy) said tree-hash lookup; this task relaxes it to
  "equal after removing kanban/". The 150 implementer suggested exactly this follow-up under `## Results`.
- `ganda repo ci-note status --tree` exists for local checks; `write` has no `--tree`.
- Related: task 161 (Amuru/Nuru migration) is independent.
