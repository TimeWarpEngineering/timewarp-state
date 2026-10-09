# Review framework — task 037

**Date:** 2026-10-10
**Host task:** kanban/to-do/037-factor-out-blazor-specific-code/
**Diff scope:** branch task/037-factor-out-blazor-specific-code vs master (`git diff master...HEAD -M`, commit f27db723)
**Plan / brief:** split Blazor features (components, JS interop, Redux DevTools, render subscriptions, wwwroot) out of TimeWarp.State into TimeWarp.State.Blazor; add AddTimeWarpStateBlazor; console sample; migration guide
**Effort:** 3 (roster axes: general)
**Reviewer roster:** general
**Session IDs:** review oracle (claude-opus-5-5, ganda task work); general reviewer subagent a954005f421aa90d7 (sonnet)

## Budget (by-diff)

- Lines changed: 840
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
