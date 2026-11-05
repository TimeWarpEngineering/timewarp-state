# Split devtools Inspector.razor into components and move logic out of handlers
## Description

`source/timewarp-state-devtools/components/pages/Inspector.razor` (551 LOC) has query logic, pure helpers and five panels in one component, and several event handlers compute or branch before dispatching, against the handlers-only-dispatch rule.

## Requirements

Bugs:
- Fire-and-forget task at ~line 47: `SyncLogSafeAsync` catches only `ObjectDisposedException` and `InvalidOperationException`. Anything else is an unobserved task exception. Catch, log, and keep the component alive.
- `Select` (~72-79) and `OnStore` (~85-98) dispatch twice in sequence, which is not atomic. Fold into one action each.
- Dead branch at ~447-450 (`diff` is null only when `selected` is null, already excluded at ~444). `selected!` at ~197 is redundant.

Conventions (tw-blazor):
- `Select`, `OnStore`, `CycleTheme`, `ToggleNode`, `ToggleDiffNode` compute or read state before dispatching. Move that logic into the action handlers so the component only dispatches.
- Group `private const` and the `Disposed` field at the top of `@code`.
- Confirm whether `@implements IDisposable` is redundant with the base class.

Split:
- Query logic (~163-261: `VisibleEntries`, `FoldedStates`, `VisibleStates`, `DiffFor`) into `InspectorState` queries or a view model.
- Pure helpers (~137-143, 230-247, 263-280) into a static view helper.
- Child components: toolbar (~310-325), `ActionLogList` (~328-387), `ActionDetail` (~396-437), `DiffPanel` (~438-483), `StatePanel` incl. `StoreSelect` (~484-548). Split CSS isolation files alongside.

Dedupe: badge/tooltip locals (~361-363 and ~489-490); copy-pasted tab buttons (~391-393, ~515-516); duplicate "Select an action to diff" message (~442, ~449); `@(attribute.Key)` redundant parentheses.

## Checklist

- [ ] Unobserved exception path fixed
- [ ] Double dispatches folded into single actions
- [ ] Handlers only dispatch
- [ ] Queries and helpers extracted
- [ ] Five child components with their own CSS isolation
- [ ] Devtools sample app renders and behaves identically

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. Subscription/Dispose handling is already correct; keep it.

## Session

- Created: claude code review session (2026-11-04)
