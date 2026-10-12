# Review framework

## Budget (by-diff)

- Lines changed: 1012
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Framework

**Date:** 2026-10-12
**Host task:** kanban/to-do/104-remove-remaining-reflection-from-timewarpstate-ensurestates-redux-devtools-middleware-logging/
**Diff scope:** branch task/104-remove-remaining-reflection-from-timewarpstate-ens vs origin/master (b9288f80; round 2 adds 0f1411f8)
**Plan / brief:** generated `TryAddTransient<T>` state registration replaces `EnsureStates` assembly scan; `PipelineBehaviorRegistrySourceGenerator` replaces `GetComponentOrder`'s `GetInterfaces()`; trimming/AOT suppressions removed from TimeWarp.State.
**Effort:** 3 (by-diff budget), roster axes general
**Reviewer roster:** general (Claude subagent), orchestrator re-verification
**Session IDs:** review oracle (Claude Opus 5.5, ganda task work review node); general reviewer subagent a858e6c69be87338e; fix subagent a5c71a1be880f0083

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
