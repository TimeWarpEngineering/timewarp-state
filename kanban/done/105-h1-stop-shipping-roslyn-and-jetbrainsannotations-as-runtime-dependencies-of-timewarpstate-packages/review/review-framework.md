# Review framework

## Budget (by-diff)

- Lines changed: 317
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

# Review framework — task 105

**Date:** 2026-10-10
**Host task:** kanban/to-do/105-h1-stop-shipping-roslyn-and-jetbrainsannotations-as-runtime-dependencies-of-timewarpstate-packages/
**Diff scope:** branch `task/105-h1-stop-shipping-roslyn-and-jetbrainsannotations-a` vs `master` (commit 14e4f49f)
**Plan / brief:** Finding H1 from task 102. Keep Roslyn and JetBrains.Annotations out of packed nuspecs, and add a nuspec dependency allow-list check to `dev pack`.
**Effort:** 2 (by-diff budget). Roster axes: general only.
**Reviewer roster:** general
**Session IDs:** claude review oracle (ganda task work, 2026-10-10)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit. Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
