# Review framework

## Budget (by-diff)

- Lines changed: 106
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

# Review framework — task 101

**Date:** 2026-10-09
**Host task:** kanban/to-do/101-update-all-nuget-packages-to-latest-incl-timewarpamuru-200-beta2/
**Diff scope:** branch task/101-update-all-nuget-packages-to-latest-incl-timewarpa vs master (ead1433b)
**Plan / brief:** Bump NuGet pins to latest incl. TimeWarp.Amuru 2.0.0-beta.2; MSTest 4 + Playwright.MSTest.v4; Aspire 13.6.1; coverlet 10.1.0
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** claude review oracle (ganda task work), 2026-10-09

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
