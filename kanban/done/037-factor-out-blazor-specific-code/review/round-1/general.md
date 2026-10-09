# Round 1 — general
**Date:** 2026-10-10
**Scope reviewed:** `git diff master...HEAD -M` (commit f27db723): service-registration split, assembly-marker behavior ordering, both csprojs (PackageId, StaticWebAssetBasePath, InternalsVisibleTo, analyzer packing), core Blazor/JSInterop references, tests, samples, docs/migration guide, slnx. Built timewarp-state-blazor with `-c ReduxDevToolsEnabled` (0 errors).

## Summary
The split is clean: core has no Blazor/JSInterop usings or references, behavior orders (100/200/300/400) are consistent across the two assembly markers, `StaticWebAssetBasePath=TimeWarp.State` preserves `_content/TimeWarp.State/`, registration is TryAdd/GetOrAdd based and order-independent, and migration-doc API names match the code. Samples, test app, integration test convention and slnx were all updated. Only minor robustness and doc-consistency points remain.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-state-blazor/extensions/service-collection-extensions.add-timewarp-state-blazor.cs:25
- Description: A Blazor host that references the package but forgets `AddTimeWarpStateBlazor()` gets the behaviors woven (assembly attributes) but `RenderSubscriptionContext` is never registered. The first action then fails with a generic DI "unable to resolve RenderSubscriptionContext" from the generated mediator. Likewise `UseReduxDevTools` (which no longer implies `JsonRequestHandler`) silently leaves DevTools JS requests with no handler. This is a new failure mode introduced by the split and is only documented in an XML remark.
- Suggestion: Optional fail-fast, e.g. have `UseReduxDevTools` / `AddJavaScriptDispatch` also `TryAddScoped<JsonRequestHandler>()`, or have `RenderSubscriptionsPostProcessor` throw a message naming `AddTimeWarpStateBlazor()`. At minimum, keep the migration guide's "Registration" section prominent (it is).
- Status: open

### Issue 2 — Severity: nit
- File: samples/00-state-action-handler/wasm/overview.md:94 (also auto/overview.md:114, server/overview.md:98, 01-redux-dev-tools/wasm/overview.md:49, 02-action-tracking/wasm/overview.md:91)
- Description: Sample walkthrough snippets show `AddTimeWarpState(...)` without `AddTimeWarpStateBlazor()` and the Blazor PackageReference, although the sample `program.cs` files were updated. Following the docs verbatim yields the missing-registration failure in Issue 1.
- Suggestion: Add the `AddTimeWarpStateBlazor();` line (and `dotnet add package TimeWarp.State.Blazor`) to those snippets.
- Status: open

### Issue 3 — Severity: nit
- File: tests/timewarp-state-tests/architecture/no-blazor-dependency-tests.cs:1
- Description: Coverage proves the assembly reference graph, but nothing asserts `AddTimeWarpStateBlazor()` registers `RenderSubscriptionContext`/`JsonRequestHandler`/registry, is idempotent, or is order-independent relative to `AddTimeWarpState` (the code comments claim this). Only `json-request-handler-dispatch-tests` exercises it indirectly.
- Suggestion: Add a small test resolving those services after calling the two methods in both orders and twice.
- Status: open
