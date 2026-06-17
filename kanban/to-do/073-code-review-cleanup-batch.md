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

- [ ] camel-case.cs replaced and deleted
- [ ] TryGetEnclosingStateType shared by extension + policy rule
- [ ] TryAdd* in both service-collection extensions
- [ ] RenderCounts → instance field (drop Dispose cleanup)
- [ ] CompiledPropertyComparisons deleted
- [ ] TrackAction lookup cached
- [ ] Full test suite green (after mediator migration tasks 040–049 restore the build)
