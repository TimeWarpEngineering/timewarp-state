# Review framework — task 162

**Date:** 2026-11-18
**Host task:** kanban/in-progress/162-ci-fast-path-gate-accept-the-attested-pr-head-tree-when-it-differs-from-head-only-under-kanban/
**Diff scope:** Round 1 reviewed the branch vs `master`. Round 2 reviews the fix delta only: `7a984d93` (round 1 `base.sha`) .. `HEAD` (`57c98e15`). Product files in that delta: `.github/workflows/workflow.yml` (banner), `documentation/dev-ops/local-ci.md`, `scripts/ci-fast-path-gate.ps1` (Purpose), `scripts/ci-fast-path-tree.ps1` (`--no-renames`), `tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs` (move-into-kanban case).
**Plan / brief:** Teach the CI fast-path gate to accept an attested tree when the checked-out tree differs only by the top-level `kanban` entry. Round 2 checks that M1 and M2 from round 1 are fixed and that the fix delta adds no new defect.
**Effort:** round 1 was 2 (general only, 120 turns). Round 2 is 1 (general only, 80 turns) on the 177-line delta.
**Reviewer roster:** general
**Session IDs:** round 1 grok `01a1ef0a-4a93-77b2-83e2-f0a05143f29f`. Round 2 grok `01a1ef18-ce82-7111-b9f5-499860aaa744` (2026-11-18). Implementer session `e165382e-6a76-4746-90b0-1508ca64a8f4`.

## Budget (by-diff)

- Lines changed: 177
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
