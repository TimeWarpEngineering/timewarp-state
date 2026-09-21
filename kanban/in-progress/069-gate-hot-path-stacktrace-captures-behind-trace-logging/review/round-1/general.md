# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** branch `task/069-gate-hot-path-stacktrace-captures-behind-trace-log` vs `origin/master`

## Summary

Gates the three hot-path `new StackTrace()` captures on `TimeWarpStateComponent` behind `TimeWarpStateOptions.CaptureRenderCaller` (default false), with a shared `FormatRenderCaller` helper so all three sites record `Class.Method` via `GetFrame(1)`. Test-app opts in via `ConfigureServices` (server reuses that), and DI already registers `TimeWarpStateOptions` as a singleton so the new `[Inject]` resolves. Low risk; requirements and surrounding call sites (reset, diagnostic pages, tests) check out.

## Issues
