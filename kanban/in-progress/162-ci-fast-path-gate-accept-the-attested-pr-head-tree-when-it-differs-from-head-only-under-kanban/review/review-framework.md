# Review framework — task 162

**Date:** 2026-11-18
**Host task:** kanban/in-progress/162-ci-fast-path-gate-accept-the-attested-pr-head-tree-when-it-differs-from-head-only-under-kanban/
**Diff scope:** branch `task/162-ci-fast-path-gate-accept-the-attested-pr-head-tree` vs `master` (three-dot). Product files: `.github/workflows/workflow.yml`, `scripts/ci-fast-path-gate.ps1`, `scripts/ci-fast-path-tree.ps1`, `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs`, `tests/timewarp-state-tests/global-usings.cs`, `tests/timewarp-state-tests/timewarp-state-tests.csproj`. Kitchen `task.md` is on the branch and is not product behavior.
**Plan / brief:** Teach the CI fast-path gate to accept an attested tree when the checked-out tree differs only by the top-level `kanban` entry. Candidates: HEAD, the PR head sha from `CI_FAST_PATH_PR_HEAD`, then each parent when HEAD is a merge.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle grok session `01a1ef0a-4a93-77b2-83e2-f0a05143f29f` (2026-11-18). Implementer session `e165382e-6a76-4746-90b0-1508ca64a8f4`.

## Budget (by-diff)

- Lines changed: 395
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
