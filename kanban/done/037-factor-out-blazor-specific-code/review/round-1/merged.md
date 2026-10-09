# Round 1 — merged findings
**Date:** 2026-10-10
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 1 | 1 |
| nit | 0 | 2 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-blazor/extensions/service-collection-extensions.use-redux-dev-tools.cs:40
- Description: `UseReduxDevTools` (and `AddJavaScriptDispatch`) no longer imply `JsonRequestHandler`. A host that skips `AddTimeWarpStateBlazor()` gets no handler for DevTools or allow-listed JS requests.
- Suggestion: `TryAddScoped<JsonRequestHandler>()` from `UseReduxDevTools` and `AddJavaScriptDispatch`.
- Source: general (Issue 1, first half)
- Disposition notes: Fixed. Both methods now `TryAddScoped<JsonRequestHandler>()`. Design regions updated. Covered by the new tests in `tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs`.

### M2 — Severity: suggestion — Status: wontfix
- File: source/timewarp-state-blazor/extensions/service-collection-extensions.add-timewarp-state-blazor.cs:25
- Description: A Blazor host that references the package but never calls `AddTimeWarpStateBlazor()` fails on the first action with a generic DI error for `RenderSubscriptionContext`.
- Suggestion: Fail fast with a message that names `AddTimeWarpStateBlazor()`.
- Source: general (Issue 1, second half)
- Disposition notes: wontfix (orchestrator). `RenderSubscriptionsPostProcessor` is constructed by DI inside the generated mediator, so a custom message would need a startup validation hook that does not exist in this design. The DI error names `RenderSubscriptionContext`. The migration guide, the XML remarks, every sample `program.cs` and (after M3) every sample overview show `AddTimeWarpStateBlazor()`. It fails on the first dispatch rather than silently, which is acceptable for a documented breaking change.

### M3 — Severity: nit — Status: fixed
- File: samples/00-state-action-handler/wasm/overview.md:94 (also auto, server, 01-redux-dev-tools/wasm, 02-action-tracking/wasm overviews)
- Description: The walkthrough snippets omit `AddTimeWarpStateBlazor()` and the TimeWarp.State.Blazor package.
- Suggestion: Add both.
- Source: general (Issue 2)
- Disposition notes: Fixed. `AddTimeWarpStateBlazor()` is added to all five overviews, plus `dotnet add package TimeWarp.State.Blazor` in the 00 wasm overview. The 01 and 02 overviews install TimeWarp.State.Plus, which depends on TimeWarp.State.Blazor. CRLF line endings are preserved.

### M4 — Severity: nit — Status: fixed
- File: tests/timewarp-state-tests/architecture/no-blazor-dependency-tests.cs:1
- Description: Nothing tests what `AddTimeWarpStateBlazor()` registers, or its idempotency and order-independence.
- Suggestion: Add registration tests.
- Source: general (Issue 3)
- Disposition notes: Fixed. New `tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs` has four tests: after AddTimeWarpState; before it and called twice; UseReduxDevTools alone; AddJavaScriptDispatch. `dev test` passes 84 tests with 1 skipped in timewarp-state-tests.

## Duplicates / conflicts

- General Issue 1 was split into M1 (fixed) and M2 (wontfix) because the two halves needed different dispositions.
