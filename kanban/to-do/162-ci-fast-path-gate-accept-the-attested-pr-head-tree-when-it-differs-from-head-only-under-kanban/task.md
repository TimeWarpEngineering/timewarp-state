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

- [ ] Gate walks candidates: HEAD, PR head (pull_request), merge parents (push)
- [ ] Stripped-tree equivalence (kanban/ only) implemented in pwsh with a printed reason on miss
- [ ] Workflow passes the PR head sha to the gate via env
- [ ] Script header and workflow comment updated
- [ ] Unit test pins the kanban-only rule
- [ ] `dev build` 0 warnings, `dev test` green, `ganda repo audit` clean
- [ ] PR run shows a hit and finishes under 1 minute (run id recorded)
- [ ] Master push run shows a hit (run id recorded)
- [ ] `## Results` filled in with before/after times

## Session

- Created: 3f3c256d-283c-4d41-ac7b-bb500c1a1415 (2026-11-18)
- Context: Steve 2026-11-18 "create the kanban-ignore task and launch it", after PR #692 proved the plumbing but
  missed on both runs.

## Notes

- Evidence: PR #692 run 44318421138 (pull_request, 6m39s, gate printed `miss (no CI note for tree 059ffb62…)`),
  run 44319655507 (master push, full pipeline). Notes on `refs/notes/ganda-ci`: trees `40f17485…` and `5b2d62f8…`.
- Task 150 RFC decision 4 (branch and merge policy) said tree-hash lookup; this task relaxes it to
  "equal after removing kanban/". The 150 implementer suggested exactly this follow-up under `## Results`.
- `ganda repo ci-note status --tree` exists for local checks; `write` has no `--tree`.
- Related: task 161 (Amuru/Nuru migration) is independent.
