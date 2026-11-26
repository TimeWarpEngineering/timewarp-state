# Review framework — task 167

**Date:** 2026-11-26
**Host task:** kanban/in-progress/167-refresh-outdated-nu-get-packages-in-time-warp-engineering-timewarp-state/
**Diff scope:** branch `task/167-refresh-outdated-nu-get-packages-in-time-warp` vs merge-base `0f1ef971` (`master`). Five commits ahead. The same 15 paths are the only tree delta against current `master` (two `land kanban 167` commits on `master` do not carry a different product change).
**Plan / brief:** Bump dev-only `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` in `Directory.Packages.props` from 4.0.0-beta.16 to 4.0.0-beta.17. Do not change `<Version>` in `source/Directory.Build.props`. The commit also adopts the `region-annotations` audit on the JS/TS files that check now judges.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** implementer-claude session 20778681-61f0-40fe-9eff-9c0afd6168d3; review oracle grok session 01a215db-16ec-7071-bf9a-2820b95d2245

## Budget (by-diff)

- Lines changed: 97
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
