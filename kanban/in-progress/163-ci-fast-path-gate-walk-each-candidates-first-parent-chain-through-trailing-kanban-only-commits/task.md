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

- [ ] First-parent ancestor walk per candidate, bounded, with tree dedupe
- [ ] Stops a chain on the first non-kanban difference
- [ ] Hit label names the candidate and ancestor depth
- [ ] Script header and workflow comment updated
- [ ] Tests for hit-through-ancestors, non-kanban stop, and the bound
- [ ] `dev build` 0 warnings, `dev test` green, `ganda repo audit` clean
- [ ] PR run hits after land-pr and finishes under 1 minute (run id recorded)
- [ ] Master push run hits (run id recorded)
- [ ] `## Results` filled in with the controls and the new times

## Session

- Created: 3f3c256d-283c-4d41-ac7b-bb500c1a1415 (2026-11-18)
- Context: Steve 2026-11-18 "Go ahead, create and launch", after the 162 proof hit once (21 s) and then missed on
  the post-land-pr head.

## Notes

- Alternative considered: move localCi in `ganda task work` to after land-pr so the attested commit is the PR head.
  Rejected for now: it needs a timewarp-ganda change, and a gate that follows kanban-only ancestors is robust to any
  walk ordering. Revisit if the ancestor walk proves fragile.
- Task 162 dry-run evidence and the kanban-only rule live in `kanban/done/162-*/task.md` and
  `scripts/ci-fast-path-tree.ps1`.
