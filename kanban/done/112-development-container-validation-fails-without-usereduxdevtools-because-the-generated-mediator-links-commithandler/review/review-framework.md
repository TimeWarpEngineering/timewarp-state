# Review framework

## Budget (by-diff)

- Lines changed: 2719
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 112

**Date:** 2026-10-12
**Host task:** kanban/to-do/112-development-container-validation-fails-without-usereduxdevtools-because-the-generated-mediator-links-commithandler/
**Diff scope:** branch `task/112-…` vs `origin/master` (commits ceaa0c36, 612bc27c; 126 files, +234/−2485)
**Plan / brief:** task.md Decision "Direction 3" — delete the Redux DevTools feature (Blazor package, core store/Hydrate, samples, test app, docs) and add a ValidateOnBuild + ValidateScopes regression test.
**Effort:** 3 (by-diff budget), roster axes: general
**Reviewer roster:** general
**Session IDs:** review oracle (Claude Opus 5.5, ganda task work headless); general reviewer = Claude subagent

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
