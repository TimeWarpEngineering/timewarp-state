# Code review cleanup batch

## Description

Low-risk deletions/replacements from code review 2026-06-11 (`code-review-2026-06-11.md`), findings 19 and 21–25, suitable for one housekeeping PR. All verified against the source; none change public behavior (except the small fix noted in item 3).

1. **Delete hand-copied camelCase implementation (finding 21):** `source/timewarp-state/json/camel-case.cs` (oddly in namespace `Microsoft.JSInterop`) duplicates `JsonNamingPolicy.CamelCase`, which `timewarp-state-options.cs:38` already uses — and its callers (Debug-state `Hydrate` methods) exist precisely to reconstruct keys produced by that policy, so drift breaks them. Replace calls with `JsonNamingPolicy.CamelCase.ConvertName(...)`, delete the file.
2. **Deduplicate enclosing-state-type walk (finding 22):** `source/timewarp-state-policies/be-nested-in-state-custom-rule.cs:9–11` is character-identical to `TypeExtensions.GetEnclosingStateType` (`timewarp-state/extensions/type-extensions.cs:8–11`), already referenced by the policies project. Add a bool-returning `TryGetEnclosingStateType` (the extension throws; the rule needs bool) and use it in both.
3. **Replace DI-registration guards with TryAdd\* (finding 23):** `timewarp-state-plus/extensions/service-collection-extensions.cs:26–27` duplicates the private helper in `timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs:150–151`; both reinvent `TryAddScoped`/`TryAddTransient`. Note: the current all-or-nothing early return at line 16 skips *handler* registration whenever `RouteState` is already present — `TryAdd*` per descriptor is also a small behavior fix.
4. **Instance field instead of static RenderCounts (finding 24):** `components/timewarp-state-component.render-mode.cs:7` keeps per-instance render counts in a static `ConcurrentDictionary` keyed by component Id, requiring `TryRemove` in `Dispose` (leak if missed). Nothing uses the static aspect (verified by grep). Replace with a plain instance `int`.
5. **Delete dead field (finding 25):** `components/timewarp-state-component.register-render-trigger.cs:8` — `CompiledPropertyComparisons` is allocated per component and has exactly one repo-wide reference: its declaration.
6. **Cache TrackAction attribute lookup (finding 19):** `timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:23` runs `GetCustomAttributes` per dispatch; replace with `private static readonly bool IsTracked = typeof(TAction).IsDefined(typeof(TrackActionAttribute), false);`.

Not included here: DevTools-related cleanup (`store.redux-dev-tools.cs` unreachable null-check branch, assembly-scan type resolution — report §1) — those die with the ReduxDevTools removal (task 058).

## Checklist

- [x] camel-case.cs replaced and deleted
- [x] TryGetEnclosingStateType shared by extension + policy rule
- [x] TryAdd* in both service-collection extensions
- [x] RenderCounts → instance field (drop Dispose cleanup)
- [x] CompiledPropertyComparisons deleted
- [x] TrackAction lookup cached
- [x] Full test suite green (after mediator migration tasks 040–049 restore the build)

## Session

- Implementer: grok session 01a0c205-98d3-7981-a498-c5044a893756 (2026-09-21)

## Results

Landed findings 19 and 21–25 from `code-review-2026-06-11.md` as one housekeeping batch. Public behavior is unchanged except `AddTimeWarpState` / `AddTimeWarpStateRouting` now `TryAdd*` per descriptor instead of returning early when one service is already present.

**What changed**

1. **Finding 21.** Deleted `source/timewarp-state/json/camel-case.cs` (`Microsoft.JSInterop.CamelCase`). Hydrate callers now use `JsonNamingPolicy.CamelCase.ConvertName(...)`, the same policy `TimeWarpStateOptions` already serializes with.
2. **Finding 22.** Added `TypeExtensions.TryGetEnclosingStateType`. `GetEnclosingStateType` throws through it; `BeNestedInStateCustomRule` uses the bool form.
3. **Finding 23.** `AddTimeWarpState` and `AddTimeWarpStateRouting` use `TryAddScoped` / `TryAddSingleton` / `TryAddTransient`. Removed the all-or-nothing `HasRegistrationFor` early returns. `HasRegistrationFor` remains for `UseReduxDevTools` (task 058).
4. **Finding 24.** `TimeWarpStateComponent.RenderCount` is an instance `int`; `Dispose` no longer `TryRemove`s a static dictionary.
5. **Finding 25.** Deleted unused `CompiledPropertyComparisons`.
6. **Finding 19.** `ActiveActionBehavior<TAction, TResponse>` caches `[TrackAction]` as `private static readonly bool IsTracked`.

**Files changed**

- Deleted `source/timewarp-state/json/camel-case.cs`
- `source/timewarp-state/extensions/type-extensions.cs`
- `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs`
- `source/timewarp-state/components/timewarp-state-component.render-mode.cs`
- `source/timewarp-state/components/timewarp-state-component.cs`
- `source/timewarp-state/components/timewarp-state-component.register-render-trigger.cs`
- `source/timewarp-state-plus/extensions/service-collection-extensions.cs`
- `source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs`
- `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.debug.cs`
- `source/timewarp-state-plus/features/theme/theme-state/theme-state.debug.cs`
- `source/timewarp-state-plus/global-usings.cs`
- `source/timewarp-state-policies/be-nested-in-state-custom-rule.cs`
- `source/timewarp-state-policies/global-usings.cs`
- Test-app `Hydrate` debug files + `tests/test-app/test-app-client/global-usings.cs`
- `tests/timewarp-state-tests/type-extensions-tests.cs` (TryGet cases)

**Decisions / deviations**

- Mediator migration already dropped manual handler registration from `AddTimeWarpStateRouting`, so the finding-23 “skip handlers” bug is now “skip the rest of `AddTimeWarpState` when `Subscriptions` is present” / “skip `RouteState` when it is present”. `TryAdd*` per descriptor is the fix.
- `UseReduxDevTools` still uses `HasRegistrationFor`; DevTools cleanup stays on task 058.
- `JsonNamingPolicy.CamelCase.ConvertName` does not throw on empty names; every remaining caller passes `nameof(...)`.

**Test outcomes**

`dotnet run --file ./scripts/test.cs` exited 0:

| Suite | Result |
|---|---|
| timewarp-state-analyzer-tests | 19 passed |
| timewarp-state-tests | 19 passed, 1 skipped |
| timewarp-state-plus-tests | 19 passed, 1 skipped |
| client-integration-tests | 42 passed, 1 skipped |
| test-app-architecture-tests | 7 passed, 1 skipped |

### How to validate

**Smoke**

```bash
test ! -f source/timewarp-state/json/camel-case.cs && echo camel-case-gone
rg -n 'MemberNameToCamelCase|class CamelCase' --glob '*.cs' || echo no-hand-copied-camelcase
rg -n 'TryGetEnclosingStateType' source/timewarp-state/extensions/type-extensions.cs source/timewarp-state-policies/be-nested-in-state-custom-rule.cs
rg -n 'TryAddScoped|HasRegistrationFor' source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs source/timewarp-state-plus/extensions/service-collection-extensions.cs
rg -n 'RenderCounts|CompiledPropertyComparisons' --glob '*.cs' || echo no-static-render-or-dead-field
rg -n 'IsTracked' source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs
dotnet build source/timewarp-state/timewarp-state.csproj --nologo -v q
```

**Expect**

- `camel-case-gone` and `no-hand-copied-camelcase`
- `TryGetEnclosingStateType` defined on `TypeExtensions` and called from `BeNestedInStateCustomRule`
- `AddTimeWarpState` / `AddTimeWarpStateRouting` use `TryAddScoped` (and related `TryAdd*`); plus file has no `HasRegistrationFor`
- `no-static-render-or-dead-field`
- `private static readonly bool IsTracked` in `ActiveActionBehavior`
- Build: **0 Error(s)** (pre-existing `warning TW0007` / `RS0030` may print)

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; analyzer 19 passed; state 19 passed, 1 skipped; plus 19 passed, 1 skipped; client-integration 42 passed, 1 skipped; architecture 7 passed, 1 skipped

dotnet fixie timewarp-state-tests
# expect: 19 passed, 1 skipped (includes Should_TryGet_* cases)
```

**Not in scope:** ReduxDevTools unreachable-null / assembly-scan cleanup (task 058); Playwright e2e (`scripts/e2e.cs`).
