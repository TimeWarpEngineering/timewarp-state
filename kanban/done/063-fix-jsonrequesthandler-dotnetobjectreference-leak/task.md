# Fix JsonRequestHandler DotNetObjectReference leak (remainder)

## Description

Code review 2026-06-11, finding 10. **Partly done:** `TimeWarpJavaScriptInterop.razor` already has `if (firstRender)` (080-003). Remaining:

`JsonRequestHandler.InitAsync` still `DotNetObjectReference.Create(this)` on every call, no `IsInitialized` guard, no stored reference, no `Dispose`. Contrast `ReduxDevToolsInterop.InitAsync`.

## Requirements

- `IsInitialized` guard in `InitAsync` (no-op if already initialized)
- Hold `DotNetObjectReference<JsonRequestHandler>` in a field; dispose in `Dispose`/`DisposeAsync`; swallow `JSDisconnectedException`
- Keep the existing firstRender guard
- Test or documented proof: N renders → one Create

## Out of scope

- Redux DevTools interop (058)
- Changing the JS `InitializeJavaScriptInterop` contract except as needed for dispose

## Checklist

- [x] firstRender in TimeWarpJavaScriptInterop (080-003)
- [x] IsInitialized in InitAsync
- [x] Store + dispose the reference
- [x] One reference across N renders
- [x] Implementation review (effort 1, general)
- [x] Review disposition recorded (`clean`)

## Notes

- Implementation review kitchen: `review/` (effort 1, general). Round 1 merged empty; disposition `clean`.

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit — firstRender already on master; remainder only. Sequence after this: 062 → 061 → 060 → 066 → 067 (one at a time).
- Implementer: grok session 01a0c453-a806-78c0-83bd-1eb10f98aacf (2026-09-21)
- Review: grok session 01a0c45e-b105-7082-a2de-b6dcc79619dc (2026-09-21); general reviewer 01a0c461-5c80-7772-b4a0-6006d3f1fc15

## Results

`JsonRequestHandler.InitAsync` no longer creates a new `DotNetObjectReference` on every call. One reference is stored for the scoped handler lifetime and disposed with the circuit. `TimeWarpJavaScriptInterop` still gates on `firstRender`.

**What landed**

- `IsInitialized` (and `IsDisposed`) make `InitAsync` a no-op after the first successful create.
- `JsonRequestHandlerReference` holds `DotNetObjectReference<JsonRequestHandler>`; `Dispose` / `DisposeAsync` release it and swallow `JSDisconnectedException`.
- Scoped DI disposes the handler when the circuit/scope ends (`IAsyncDisposable` + `IDisposable`).
- JS `InitializeJavaScriptInterop` contract unchanged.

**Files**

- `source/timewarp-state/features/javascript-interop/json-request-handler.cs`
- `tests/timewarp-state-tests/javascript-interop/json-request-handler-tests.cs` (new)
- `tests/timewarp-state-tests/global-usings.cs`

**Decisions**

- Set `IsInitialized` before the JS invoke so overlapping `InitAsync` calls cannot create a second root.
- `firstRender` in `TimeWarpJavaScriptInterop.razor` is unchanged (080-003). Both guards stay.
- Did not change Redux DevTools interop (058).

**Tests**

`dotnet run --file ./scripts/test.cs` exit 0:

- analyzer 19 passed
- source generator 4 passed
- state 31 passed, 1 skipped (includes five `JsonRequestHandlerTests.Should_` cases)
- plus 25 passed, 1 skipped
- client integration 42 passed, 1 skipped
- architecture 7 passed, 1 skipped

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet fixie timewarp-state-tests
```

**Expect**

- Exit 0.
- `JsonRequestHandlerTests.Should_.InitAsync_Called_Repeatedly_Creates_One_DotNetObjectReference` passes: five `InitAsync` calls produce one `InitializeJavaScriptInterop` invoke with a single `DotNetObjectReference<JsonRequestHandler>`.
- `JsonRequestHandlerTests.Should_.Dispose_Releases_The_DotNetObjectReference` and `DisposeAsync_Releases_The_DotNetObjectReference` pass: after dispose, the captured reference's `Value` throws `ObjectDisposedException`.
- `TimeWarpJavaScriptInterop.razor` still contains `if (firstRender)` around `InitAsync`.

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; analyzer 19, generator 4, state 31+1 skipped, plus 25+1 skipped,
# client integration 42+1 skipped, architecture 7+1 skipped
```

**Not in scope:** Redux DevTools interop; changing the JS `InitializeJavaScriptInterop` contract.

### Review

- Rounds: 1. Effort 1, roster: general.
- Final counts: bug 0 / suggestion 0 / nit 0 (all status empty — no findings).
- Disposition: **clean** (0 open; no wontfix / no escalations).
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
