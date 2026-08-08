# Fix RouteState GoBack off-by-one stack clamp

## Description

Code review 2026-06-11, finding 3 (`code-review-2026-06-11.md`) — **reproduced by failing unit test**.

The route stack always holds the *current* page on top (`PushRouteInfo` fires after every page render and pushes `NavigationManager.Uri`), so going back *n* pages costs *n* pops and the destination must remain on the stack to be `Peek()`ed. The maximum valid amount is therefore `Count - 1` — exactly what `CanGoBack => RouteStack.Count > 1` encodes. The handler's clamp in `source/timewarp-state-plus/features/routing/route-state/route-state.go-back.cs:37` is:

```csharp
int amountToGoBack = Math.Min(action.Amount, RouteState.RouteStack.Count);
```

which allows one pop too many: with stack depth 1 (user landed directly on a page, no history), `GoBack()` passes the `IsRouteStackEmpty` guard, pops the current page, and `Peek()` at line 45 throws `InvalidOperationException: Stack empty.` In a running app `StateTransactionBehavior` catches it, so the symptom is a navigation that silently does nothing.

## Fix

```csharp
int amountToGoBack = Math.Min(action.Amount, RouteState.RouteStack.Count - 1);
if (amountToGoBack <= 0) return Task.CompletedTask;
```

This also gives sensible "go back as far as possible" behavior for `GoBack(5)` with only 3 entries (lands on the oldest route) and makes the depth-1 case a no-op, matching `CanGoBack`.

## Checklist

- [x] Apply the clamp fix in `route-state.go-back.cs` — `Math.Min(action.Amount, RouteStack.Count - 1)` + `if (amountToGoBack <= 0) return;`
- [x] Adopt the repro tests as regression tests; corrected the depth-1 assertion (it's a no-op at the root, not a navigation — my original repro over-asserted `ShouldNotBeEmpty`)
- [x] Verify tests pass — `timewarp-state-plus-tests` is fully green (11 passed, 0 failed, 1 skipped) on the migrated .NET 10 / Mediator build

## Done 2026-06-16 (during task 049 convergence)

## Notes

The repro tests were verified to fail with `Stack empty` at commit `ff291b92` (last compiling commit before the Mediator migration). The `dev` branch does not compile until kanban tasks 040–049 land, so the tests cannot run on `dev` yet.
