# Review framework

## Budget (by-diff)

- Lines changed: 976
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

# Review framework — task 118

**Date:** 2026-10-16
**Host task:** kanban/to-do/118-package-state-devtools-inspector-as-plugin-1-for-the-timewarp-devtools-shell/
**Diff scope:** branch `task/118-package-state-devtools-inspector-as-plugin-1-for-t`, `99585466..f075d4a3` (merge base = origin/master; one commit "feat: run the DevTools inspector from a NuGet package")
**Plan / brief:** task.md Requirements: ship the inspector as `TimeWarp.State.DevTools.App` (dashboard pattern), Hosting runs the packaged executable, `DevToolsProjectLocator` removed, explicit inner-loop project switch, run-mode only, out-of-tree consumer test, docs.
**Effort:** 3 (by-diff budget above); roster axes: general
**Reviewer roster:** general (review oracle, claude-opus-5-5)
**Session IDs:** review oracle 3bc6725a-883d-4f3b-b451-3c8caedd96e0

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
