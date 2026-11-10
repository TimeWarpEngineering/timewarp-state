# Review framework

## Budget (by-diff)

- Lines changed: 613
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Framework — task 153

**Date:** 2026-11-10
**Host task:** kanban/in-progress/153-drop-timewarparchitectureanalyzers-get-twa0004-from-timewarpsourcegenerators/
**Diff scope:** branch `task/153-drop-timewarparchitectureanalyzers-get-twa0004-fro` vs `master` (merge base), product commit `fb4ca031`. The kanban and `.timewarp/suppression-baseline.json` paths were read but not reviewed line by line (audit output).
**Plan / brief:** `## Goal (implementer restatement)` in task.md: drop `TimeWarp.Architecture.Analyzers`, bump `TimeWarp.SourceGenerators` to 1.0.0-beta.13 (TWA0004/0023/0038), keep the TWA severities, and move the attribution comments.
**Effort:** 2 (by-diff, 613 lines)
**Reviewer roster:** general
**Session IDs:** review oracle claude opus-5-5 (ganda task work review node, headless print mode)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- `ganda repo audit --fix` output (hooks, suppression baseline, pin bumps, .gitignore) is policy baseline, not a finding
