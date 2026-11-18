# Review framework — task 163

**Date:** 2026-11-18
**Host task:** kanban/in-progress/163-ci-fast-path-gate-walk-each-candidates-first-parent-chain-through-trailing-kanban-only-commits/
**Diff scope:** branch `task/163-ci-fast-path-gate-walk-each-candidates-first` vs merge-base `1602f7c4` (local `master`). Implementation commit `a07b0a90`. Product files: `scripts/ci-fast-path-gate.ps1`, `scripts/ci-fast-path-tree.ps1`, `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs`, `.github/workflows/workflow.yml` (step comment), `documentation/dev-ops/local-ci.md`.
**Plan / brief:** For each fast-path candidate (HEAD, pr-head, parent N), test the commit, then walk its first-parent ancestors (up to 10) with the task 162 note + kanban-only rule. Stop a chain on a hit (`<candidate> ancestor K <sha>`), on the first tree that differs outside `kanban/`, at a root, or at the bound. Dedupe commits and trees. Later gate checks stay as task 162 left them.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** grok 01a1ef4f-1de2-71c1-a7ec-9b40e1d53e85 (review); implementer claude ea93328b-5603-43d0-b2df-380b6df0256c

## Budget (by-diff)

- Lines changed: 512
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
