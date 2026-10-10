# Task 104: Remove remaining reflection from TimeWarp.State (EnsureStates, Redux DevTools, middleware logging)

## Description

Task 097 (PR #626, merged as `69dd6d58`) replaced the reflection cloner with a source generator but
left the other reflection sites in `TimeWarp.State` suppressed and listed them as a follow-up. This
task removes them, so `TimeWarp.State` (already `<IsAotCompatible>true</IsAotCompatible>` in
`source/timewarp-state/timewarp-state.csproj:24`) builds with no IL2xxx/IL3xxx warnings and no
`UnconditionalSuppressMessage` for trimming/AOT.

Filed 2026-10-10 at Steven's request (relayed by Amina). Not launched.

## Reflection sites

From the 097 Results table (`kanban/done/097-proposal-source-generator-for-state-cloning-aot-friendly/task.md`, lines 373-382):

| Site | Reflection | Warnings | Replace with |
|------|------------|----------|--------------|
| `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs` `EnsureStates` (`:85`) | `Assembly.GetTypes()`, `TryAddTransient(Type)` | IL2026, IL2072 (suppressed `:79-80`) | generated state registration |
| `source/timewarp-state/store/store.redux-dev-tools.cs` `LoadStatesFromJson` | `JsonSerializer.Deserialize<Dictionary<string, object>>` | IL2026, IL3050 (suppressed `:43-44`) | generated DevTools hydration |
| `source/timewarp-state/store/store.redux-dev-tools.cs` `LoadStateFromJson` | `AppDomain.CurrentDomain.GetAssemblies()` / `GetTypes` / `GetMethod` / `Invoke` (`:90-114`) | IL2026, IL2070, IL2072, IL2075, IL3050 (suppressed `:67-71`) | generated DevTools hydration |
| `source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs` `GetComponentOrder` (`:40`) | `Type.GetInterfaces()` | none reported | non-reflection pipeline listing |

097's follow-up text (line 380): "Remove remaining reflection from TimeWarp.State (EnsureStates, redux
devtools, middleware logging)" — generated state registration, generated DevTools hydration, and a
non-reflection pipeline listing.

## Fable review finding M1 (task 102)

Task 102: `kanban/done/102-full-codebase-review-by-claude-fable-review-only/` (merged in PR #629, `00ade373`).

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 57-62 (PR #629, merged as `00ade373`):

> ### M1. Redux DevTools time travel is unreachable from the shipped JavaScript, but keeps the core package's only trim-unsafe reflection
>
> - `source/timewarp-state-blazor/wwwroot/typescript/redux-dev-tools.ts:61` onward maps every `DISPATCH` payload (`COMMIT`, `JUMP_TO_STATE`, `IMPORT_STATE`, ...) to `undefined`, and `MessageHandler` ignores unmapped types. Only `START` reaches .NET. `CommitHandler` and `IReduxDevToolsStore.LoadStatesFromJson` are therefore dead at runtime.
> - `source/timewarp-state/store/store.redux-dev-tools.cs:43-71` carries seven `UnconditionalSuppressMessage` attributes (IL2026, IL2070, IL2072, IL2075, IL3050) to scan `AppDomain.CurrentDomain.GetAssemblies()` and invoke `Hydrate` through `MethodInfo.Invoke`. Together with `EnsureStates` (`service-collection-extensions.add-timewarp-state.cs:85`) these are the only reflection sites left after task 097, and 097's own requirement was "no reflection in TimeWarp.State at all, used or unused".
> - `State<T>.Hydrate` (`state/state.cs:62`) throws `NotImplementedException` by default; only five states in the repo override it.
> - Suggestion: pick one. Either finish time travel (map `JUMP_TO_STATE`/`JUMP_TO_ACTION`, have the clone generator emit `Hydrate` so the reflection goes away, and test it), or delete `LoadStatesFromJson`, `Hydrate`, `IState<TState>`, `CommitHandler` and the seven suppressions now. The scratch backlog already lists splitting DevTools into its own package; deleting is consistent with that.

Related Low finding L2 (same file) notes `EnsureStates` (`:85`) `assembly.GetTypes()` throws
`ReflectionTypeLoadException` on a partially loadable assembly; generated registration removes that too.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Requirements

- `EnsureStates`: the source generator emits state registration for the states in the compilation
  (same rule as `StateCloneRegistry`) and `AddTimeWarpState` calls it; no `Assembly.GetTypes()` or `TryAddTransient(Type)`.
- Redux DevTools hydration: per M1, Steven picks one before implementation:
  - **Finish:** the generator emits typed hydration (and `Hydrate`) per state, `LoadStatesFromJson`
    dispatches through it with source-generated JSON (`JsonTypeInfo`), no `Dictionary<string, object>`,
    no assembly scan, no `MethodInfo.Invoke`; map `JUMP_TO_STATE`/`JUMP_TO_ACTION` in
    `redux-dev-tools.ts` and test it.
  - **Delete:** remove `LoadStatesFromJson`, `Hydrate`, `IState<TState>`, `CommitHandler` and the
    suppressions (consistent with splitting DevTools out; see task 103 for the replacement inspector).
- `GetComponentOrder`: list pipeline components without `Type.GetInterfaces()` (e.g. generated or
  declared order metadata).
- Remove every trimming/AOT `UnconditionalSuppressMessage` in `TimeWarp.State`.

## Checklist

- [ ] Decide M1: finish time travel with generated hydration, or delete it (record Steven's choice here)
- [ ] Generated state registration replaces `EnsureStates` reflection
- [ ] Generated DevTools hydration replaces `LoadStatesFromJson`/`LoadStateFromJson` reflection (or delete per decision)
- [ ] Non-reflection pipeline listing replaces `GetComponentOrder`'s `Type.GetInterfaces()`
- [ ] Remove the IL2026/IL2070/IL2072/IL2075/IL3050 suppressions
- [ ] Tests: registration of all states (incl. generic/nested), hydration round-trip (if kept), middleware logging output
- [ ] Docs/release notes for any public API removed
- [ ] Code review

## Acceptance criteria

- `rg -n 'UnconditionalSuppressMessage|GetTypes\(|GetAssemblies\(|GetMethod\(|GetInterfaces\(|\.Invoke\(' source/timewarp-state`
  finds no trimming/AOT suppressions or reflection calls (delegate `?.Invoke` excluded).
- A Release build of `source/timewarp-state` with `IsAotCompatible` reports zero IL2xxx/IL3xxx warnings.
- Existing tests green; new tests cover generated registration (and hydration, if kept).
- `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)

## Notes

- Related: 097 (done; source), 102 M1 (done; finding), 103 (companion DevTools app), 109 (Medium
  findings batch; its M1 item points here), 109 M2 (AOT on Blazor and Plus is separate).
