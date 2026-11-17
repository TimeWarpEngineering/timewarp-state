# Review framework — task 160

**Date:** 2026-11-17
**Host task:** kanban/in-progress/160-honor-mediator-declared-generator-emitted-requests-manifest-catalog-dispatch-and-analyzers-include-actions-another-generator-emits/
**Diff scope:** branch `task/160-honor-mediator-declared-generator-emitted-requests` vs `master` (`master...HEAD`). Product commit `a3e5420b`. 44 files, +1877/−116. Untracked kitchen file at review start: this framework (host budget stub).
**Plan / brief:** Honor Mediator `GeneratedRequestPatternAttribute` (15.0.0-beta.10, task 043). For each declared request placed as `<State>.<Name>ActionSet.Action`, emit the dispatch method, catalog entry, and manifest row (`origin` `declared`, handler, response). `StateManifest.Parse` and the DevTools manifest tab read `origin`. TWS0015 reports a declared action missing from the final compilation. Per-rule TWS disposition lives in the analyzer Design region. Tests use a sibling generator. Sample `samples/09-declared-actions`.
**Effort:** 3
**Reviewer roster:** general
**Session IDs:** review oracle, ganda task work, print mode, conversation `01a1e93e-424a-79e3-bf47-0f521c2737eb` (2026-11-17). Implementer: implementer-claude, session not reported, max-turns 500.

## Budget (by-diff)

- Lines changed: 1993
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Goal check

The implementer restatement matches the original task. The ActionSet placement rule is the shape a source action must already have (`ActionSetShape`), which is what "exactly as if it were in source" requires. No misread recorded.
