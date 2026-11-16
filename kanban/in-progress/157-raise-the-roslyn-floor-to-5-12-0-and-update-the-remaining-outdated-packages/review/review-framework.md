# Review framework — task 157

**Date:** 2026-11-16
**Host task:** kanban/in-progress/157-raise-the-roslyn-floor-to-5-12-0-and-update-the-remaining-outdated-packages/
**Diff scope:** round 2 is the delta since round-1 base `87667996` (commit `70df11fc`): `readme.md` and `documentation/partials/installation.md`. Round 1 was the branch vs `master` (merge-base `48f40451`, product commit `e4c304a8`, 306 lines).
**Plan / brief:** Raise the Roslyn consumer compiler floor from 5.9.0 to 5.12.0 and bring the other seven outdated pins current. Round 2 checks the M1 wording fix. Goal restatement matches the task requirements.
**Effort:** round 2 is 1 (general only; host Budget.ByDiff). Round 1 was 2.
**Reviewer roster:** general
**Session IDs:** round 1 review oracle grok session 01a1e2db-8067-7281-92b8-7658e3944f4d; round 2 review oracle grok session 01a1e2e9-eebb-7cd2-afee-a90c0f0436c4; implementer session not reported

## Budget (by-diff)

- Lines changed: 105 (round 2 delta; round 1 was 306)
- Effort: 1 (round 2; round 1 was 2)
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped). Round 1 cap was 120.

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
