# CI fast-path gate: walk each candidate's first-parent chain through trailing kanban-only commits

## Description

Task 162 made `scripts/ci-fast-path-gate.ps1` look for the `dev ci` note on candidate commits (HEAD, the PR head
from `CI_FAST_PATH_PR_HEAD`, the merge parents) and accept a candidate whose tree equals HEAD's after removing the
top-level `kanban/` entry. It worked once: PR #694 run 44330291964 hit via pr-head in 21 s.

Then the walk's `land-pr` step pushed one more `chore(kanban): land task 162` commit (6a47f0d1, one line in task.md)
on top of the attested commit (97c8816f). That commit became the PR head and the merge's parent 2. The attested
commit is now the PR head's first parent, which is not a candidate, so:

- PR #694 run 44330457502 (head 6a47f0d1): miss, 7m21s. Gate printed
  `HEAD a7b969e5: no CI note for tree 5531df6e; pr-head 6a47f0d1: no CI note for tree 39d510eb; parent 1 0df28905:
  no CI note for tree d21a3971`.
- Master run 44331919992 (merge 4309e3f5): miss, full pipeline.

`ganda task work` always runs localCi at open-pr and then lands kanban commits at land-pr, so every walk's final
PR head is one or more kanban-only commits past the attested commit. The gate must follow that chain.

## Goal

For each candidate (HEAD, pr-head, parent N), after checking the candidate itself, walk its **first-parent chain**
and test each ancestor the same way, stopping when:

- an ancestor's tree has a note and passes the kanban-only rule against HEAD's tree → hit, label it
  `<candidate label> ancestor K <sha>`;
- an ancestor's tree differs from HEAD's outside `kanban/` → stop this chain (anything older cannot match either);
- the chain has gone `$MaxAncestors` (10) commits, or reached a root → stop this chain.

Candidate order and all other checks (stages, SDK, RID, package set and sha256 from
`refs/ci-packages/<attested tree>`) stay as task 162 left them. Keep it PowerShell, a script not a runfile, and keep
the hit/miss lines in the same shape so the existing tests still read them. `dev ci`, ganda, and the note format
are unchanged.

## Goal (implementer restatement)

`ganda task work` runs `dev ci` (which writes the note) at open-pr, then pushes kanban-only commits at land-pr, so
the attested commit ends up one or more commits *behind* every candidate the gate checks. The gate must look past
those commits: for each candidate (HEAD, pr-head, parent N) test the candidate, then walk its first-parent
ancestors with the same note + kanban-only rule. A chain ends on a hit (label `<candidate> ancestor K <sha>`), on
the first ancestor whose tree differs from HEAD's outside `kanban/` (older ones cannot match), on a root, or after
10 ancestors. Trees and commits already tested are not re-tested. Everything after the lookup (stages, SDK, RID,
packages) is unchanged; hit/miss lines keep their shape.

Acceptance:
- The walk lives in `scripts/ci-fast-path-tree.ps1` (so tests run the real code) and the gate calls it.
- Gate header, tree helper header, workflow comment, and `documentation/dev-ops/local-ci.md` explain the walk.
- Tests: note two kanban-only commits behind the candidate → hit `pr-head ancestor 2 …`; a non-kanban commit in
  between → miss naming "differs outside kanban/"; a note 11 commits back → miss at the 10-commit bound.
- `dev build` 0 warnings, the touched test file green, `ganda repo audit` clean.
- The live proof (PR run after land-pr, master push after merge) happens after this oracle, on the PR the host
  opens; those run ids get recorded then.

## Requirements

- `scripts/ci-fast-path-gate.ps1` (and `scripts/ci-fast-path-tree.ps1` if a helper fits there): the ancestor walk
  above, with dedupe of already-tested trees across candidates and chains.
- Script header Purpose/Design and the workflow step comment updated to describe the ancestor walk and why
  (land-pr commits after localCi).
- Tests in `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs` (or a sibling file) cover:
  attested commit two kanban-only commits behind the PR head → hit; a non-kanban commit between → miss with the
  "differs outside kanban/" reason; the 10-commit bound.
- Prove it on this task's own PR: the pull_request run after land-pr must print
  `CI fast path: hit (pr-head ancestor 1 …)` (or similar) and skip `Run CI Pipeline`; the master push run after
  merge must hit via `parent 2 ancestor …`. Record both run ids and wall times under `## Results`, next to the
  controls: 44330291964 (hit, 21 s), 44330457502 (miss, 7m21s), 44331919992 (miss).
- `ganda repo audit --fix` run and its changes committed (audit fixes are policy). Keep the `.editorconfig` pin
  holds if any remain after task 161.

## Checklist

- [x] First-parent ancestor walk per candidate, bounded, with tree dedupe
- [x] Stops a chain on the first non-kanban difference
- [x] Hit label names the candidate and ancestor depth
- [x] Script header and workflow comment updated
- [x] Tests for hit-through-ancestors, non-kanban stop, and the bound
- [ ] `dev build` 0 warnings, `dev test` green, `ganda repo audit` clean
- [ ] PR run hits after land-pr and finishes under 1 minute (run id recorded)
- [ ] Master push run hits (run id recorded)
- [ ] `## Results` filled in with the controls and the new times

## Session

- Created: 3f3c256d-283c-4d41-ac7b-bb500c1a1415 (2026-11-18)
- Context: Steve 2026-11-18 "Go ahead, create and launch", after the 162 proof hit once (21 s) and then missed on
  the post-land-pr head.
- 2026-11-18 implement oracle: walk in `ci-fast-path-tree.ps1`, gate rewired, docs, 4 tests; dry run on PR #694
  history hits `pr-head ancestor 1 97c8816f`. Note: master push for #694 would still miss (master moved outside
  kanban/ before the merge), so the master-push proof needs a quiet master between dev ci and merge.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session ea93328b-5603-43d0-b2df-380b6df0256c, max-turns 500, Done — 2026-11-18T15:38:13Z
- Reviewer: grok session 01a1ef4f-1de2-71c1-a7ec-9b40e1d53e85 (2026-11-18), effort 2, roster general, round 1, disposition clean
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ef4f-1de2-71c1-a7ec-9b40e1d53e85, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-18T15:48:39Z
- Installed ganda changed during this walk: was 55037800 bytes fa2b7eedb920caa6af321bdc4612189673e7598cd8f65b582e796e6d60cf27c2; now 55037800 bytes 9e42ae6a28d59eb3284c86fc6a2b4aabf6ffa4c31c59d330b27ef51d79e100b6

## Results

What changed:
- `scripts/ci-fast-path-tree.ps1`: new `Find-AttestedTree` (the lookup) and `Invoke-Git` (moved from the gate).
  Per candidate: test the commit, then its first-parent ancestors up to `$MaxAncestors` (10). A chain stops at
  the first tree that differs from HEAD's outside `kanban/` (reason lists the paths), at a root, or at the bound.
  Commits already walked end a chain; trees already tested are not re-tested. Hit label
  `<candidate> ancestor K <sha>`. Header Purpose/Design updated.
- `scripts/ci-fast-path-gate.ps1`: calls `Find-AttestedTree`; header Design updated; all later checks unchanged;
  hit/miss line shape unchanged.
- `.github/workflows/workflow.yml` step comment and `documentation/dev-ops/local-ci.md` describe the walk.
- `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs`: 4 new tests (hit two kanban-only
  commits behind; stop at a non-kanban difference even with a matching note further back; 10-ancestor bound,
  hit at 10 and miss at 11; a commit reached from two candidates is walked once). Test git runs with
  `GIT_CONFIG_GLOBAL=/dev/null`, `GIT_CONFIG_NOSYSTEM=1`, and fixed author/committer env.

Gates:
- `./bin/dev build` — Build succeeded, 0 warnings, 0 errors.
- `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs` — not a standalone runfile (Fixie
  project, no `#if !JARIBU_MULTI`); ran the class alone with
  `dotnet fixie timewarp-state-tests --configuration Release --no-build --tests "DevCli.Commands.CiFastPathTreeTests*"`
  — 8 passed. CI runs it in the full suite.
- `tests/timewarp-state-tests/release-pipeline/ci-note-input-tests.cs` (reads the gate's `$RequiredStages` line) —
  same fixie filter `DevCli.Commands.CiNoteInputTests*` — 6 passed.
- `ganda repo audit` — passes, 1 advisory warning (`vscode-window-icon`: `.vscode/settings.json` missing).
  `ganda repo audit --fix` created the git-ignored `.vscode/settings.json`; audit then passes all checks.
  Nothing tracked changed.

Dry run on real history (live `refs/notes/ganda-ci`):
- PR #694 head 6a47f0d1 as pr-head with HEAD tree = its own tree:
  `hit (pr-head ancestor 1 97c8816f0e3e5b5a2c7bfba899d60d14262911fd) tree 06c7c1b3…` — the commit run
  44330457502 missed.
- Master merge 4309e3f5 (run 44331919992): still a miss, correctly. Parent 2 6a47f0d1 differs from the merge
  tree outside `kanban/` (`.editorconfig`, `Directory.Packages.props`, `scripts/e2e-*.cs`, …): master took
  other PRs between `dev ci` and the merge, so that tree was never attested.

Live proof (pending; happens after this oracle on the PR the host opens):

| Run | Event | Result | Wall time |
|-----|-------|--------|-----------|
| 44330291964 | PR #694 (task 162 control) | hit via pr-head | 21 s |
| 44330457502 | PR #694 after land-pr (control) | miss | 7m21s |
| 44331919992 | master push 4309e3f5 (control) | miss | full pipeline |
| TBD | this PR after land-pr | expect `hit (pr-head ancestor 1 …)` | TBD |
| TBD | master push after merge | expect `hit (parent 2 ancestor …)` if master took no non-kanban commits | TBD |

### How to validate

Smoke:
```bash
dotnet fixie timewarp-state-tests --configuration Release --tests "DevCli.Commands.CiFastPathTreeTests*"
git fetch --no-tags origin '+refs/notes/ganda-ci:refs/notes/ganda-ci'
pwsh -NoProfile -Command '. ./scripts/ci-fast-path-tree.ps1; $f = Find-AttestedTree -HeadTree (git rev-parse "6a47f0d1^{tree}") -Candidates @(@{Label="pr-head";Commit="6a47f0d1"}) -NotesRef refs/notes/ganda-ci; $f.Label'
```

Expect: 8 passed; the pwsh line prints `pr-head ancestor 1 97c8816f0e3e5b5a2c7bfba899d60d14262911fd`. On this
PR's CI after land-pr, the `Local CI fast path` step prints `CI fast path: hit (pr-head ancestor 1 …)` and
`Run CI Pipeline` is skipped.

### Review

- Round 1, effort 2, roster general (grok session 01a1ef4f-1de2-71c1-a7ec-9b40e1d53e85). No specialist axes.
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0; nit 0. Final open count 0.
- Disposition: clean. No wontfix and no escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- The restatement matches the task Goal. No misread under Learnings.
- Gates were not re-run here. Build, the touched Fixie classes, and audit stay on the implementer record above. Live run ids in the table stay TBD until the host opens this PR.

## Review

- Round 1: Done

## Notes

- Alternative considered: move localCi in `ganda task work` to after land-pr so the attested commit is the PR head.
  Rejected for now: it needs a timewarp-ganda change, and a gate that follows kanban-only ancestors is robust to any
  walk ordering. Revisit if the ancestor walk proves fragile.
- Task 162 dry-run evidence and the kanban-only rule live in `kanban/done/162-*/task.md` and
  `scripts/ci-fast-path-tree.ps1`.
