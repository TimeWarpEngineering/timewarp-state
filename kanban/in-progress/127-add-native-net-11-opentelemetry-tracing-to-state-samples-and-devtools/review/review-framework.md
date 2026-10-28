# Review framework

## Budget (by-diff)

- Lines changed: 15
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Framework — task 127

**Date:** 2026-10-29
**Host task:** kanban/in-progress/127-add-native-net-11-opentelemetry-tracing-to-state-samples-and-devtools/
**Diff scope:** the claim worktree's uncommitted working tree compared with `HEAD` (cac4d431). The
implementation is not committed. `git log master..HEAD` holds only kanban commits, which is why the
by-diff budget counted 15 lines. The product diff is 11 modified files (+67 / -9) and one untracked file,
`samples/04-telemetry/server/sample-04-server/native-trace-sources.cs`.
**Plan / brief:** task.md `## Goal (implementer restatement)` and `## Requirements`. Replace
`AddAspNetCoreInstrumentation()` with `AddSource` for the .NET 11 framework ActivitySources, keep DevTools
on `TimeWarp.State` only, and update the docs.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** review oracle claude-opus-5-5 (ganda task work, headless); implementer grok session
01a183d6-7b24-75a3-a0e0-0c049af2e049

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
