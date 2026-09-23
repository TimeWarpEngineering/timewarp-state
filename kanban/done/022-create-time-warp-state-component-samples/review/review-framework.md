# Review framework — task 022

**Date:** 2026-09-22
**Host task:** kanban/to-do/022-create-time-warp-state-component-samples/
**Diff scope:** local uncommitted work on branch `task/022-create-time-warp-state-component-samples` (ahead of `origin/master` at merge-base via prior brief commit `6cc1cbbe`). Product paths: `samples/06-render-control/`, `documentation/topics/render-control.md`, `documentation/topics/toc.yml`, `documentation/features/features.md`, `samples/overview.md`, `readme.md`, `source/timewarp-state/components/timewarp-state-component.md`, `timewarp-state.slnx`, kitchen `kanban/to-do/022-…/`.
**Plan / brief:** One Blazor WebAssembly sample teaching `TimeWarpStateComponent` render decisions (`RenderMode` / `RendererInfo`, `RenderReason`, parameter checks, `HandleUnregisteredParameter`, `RegisterRenderTrigger`, diagnostic `CaptureRenderCaller`). Match sample 05 layout; current API only; no Server/Auto farm.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** cursor implementer-cursor (review oracle, ganda task-work, 2026-09-22)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
