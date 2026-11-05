# Split devtools Inspector.razor into components and move logic out of handlers
## Description

`source/timewarp-state-devtools/components/pages/Inspector.razor` (551 LOC) has query logic, pure helpers and five panels in one component, and several event handlers compute or branch before dispatching, against the handlers-only-dispatch rule.

## Goal (implementer restatement)

Make the DevTools Inspector page small and convention-clean without changing what it shows.

- The log-changed fire-and-forget path must never leave an unobserved task exception: expected
  circuit-teardown exceptions stay silent, anything else is logged and the page keeps running.
- Row selection and the store picker each become one action (no two-dispatch sequence).
- Every component event handler only dispatches; theme cycling, node toggles, store-option
  decoding, and row-store pinning live in action handlers.
- Queries (visible rows, selected row, store, fold, filtered states, diff) move into a view model,
  and pure formatting helpers into a static view helper.
- The page is split into toolbar, action log list, action detail, diff panel, and state panel
  (with a store select), each with its own isolated CSS; badge and tab markup are deduplicated,
  the dead diff branch and redundant `!` / parentheses / `@implements IDisposable` go.
- Done when the DevTools app builds, its tests pass (source-shape test updated for the split, new
  tests for the new actions and view model), the prerendered page renders, and `ganda repo audit`
  is clean.

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

- [x] Unobserved exception path fixed
- [x] Double dispatches folded into single actions
- [x] Handlers only dispatch
- [x] Queries and helpers extracted
- [x] Five child components with their own CSS isolation
- [x] Devtools sample app renders and behaves identically

## Notes

Found by a code review on 2026-11-04 of all files over 500 LOC. Subscription/Dispose handling is already correct; keep it.

## Session

- Created: claude code review session (2026-11-04)
- Implemented (implementer oracle, 2026-11-05): actions, view model, view helper, eight child
  components with isolated CSS, tests. Audit: passes with two pre-existing advisory warnings
  (`kebab-path-names` is deliberately `warning` in `.editorconfig`; `kanban-landed` is task 098's
  claim, not this task), so no audit fix was needed.

## Results

### What changed

- **Unobserved exception:** `SyncLogSafeAsync` still ignores circuit-teardown
  `ObjectDisposedException` / `InvalidOperationException`, and now catches and logs (via
  `ILogger<Inspector>`) any other exception, so the fire-and-forget task never faults unobserved and
  the page keeps running.
- **Single dispatches:** `SelectEntry` now also pins the selected row's store (handler reads
  `InspectorLog`). The store picker dispatches one new `SelectStoreOption(option)` action that decodes
  follow / untagged / store id and clears the selection.
- **Handlers only dispatch:** new `CycleTheme`, `ToggleNode`, `ToggleDiffNode` actions hold the logic
  the page used to run. `SetTheme` and `CycleTheme` share `ApplyThemeAsync`. The first-render theme
  re-apply dispatches `SetTheme(Theme)` instead of calling JS from the page, so the page no longer
  injects `IJSRuntime`.
- **Queries and helpers extracted:** `features/inspector/inspector-view.cs` (`InspectorView.Create`:
  filtered rows, selection, `SelectedDiff` pairing the row and its diff, store, fold, filtered states)
  and `components/inspector/inspector-view-helper.cs` (labels, CSS classes, tab lists, store option text).
- **Split:** `InspectorToolbar`, `ActionLogList`, `ActionDetail`, `DiffPanel`, `StatePanel` (with
  `StoreSelect`), each with its own `.razor.css`. `Inspector.razor` dropped from 551 to ~110 lines.
- **Dedupe:** `StoreBadgeChip` replaces the two badge/tooltip local blocks; `TabStrip` replaces both
  copy-pasted tab button groups; the dead `diff is null` branch and its duplicate message are gone
  (`SelectedDiff` makes it unrepresentable); `selected!` is gone.
- **Conventions:** `@implements IDisposable` removed (`TimeWarpStateComponent` already implements
  it); store option constants moved to `InspectorState`, so `@code` opens with injections and the
  `Disposed` field.
- **Finding on `@(attribute.Key)`:** the parentheses were *not* redundant. `@attribute` is a Razor
  directive, so `@attribute.Key` fails with RZ2005. The loop variable is now `tag` (`@tag.Key`).

### Tests and gates

- `./bin/dev test`: all suites pass (DevTools: 108 passed, including the new
  `InspectorActions_Should` and `InspectorView_Should`).
- `ActionLogColumns_Should` now reads `ActionLogList.razor(.css)`, `StoreBadgeChip.razor`, and
  `StoreSelect.razor`; same assertions, pointed at where the markup lives after the split.
- `ganda repo audit`: passes (2 pre-existing advisory warnings, see Session).
- Manual browser smoke (playwright-cli against a local DevTools app with ingested spans): theme
  cycle system → dark → light → system updates `<html data-theme>`; row click selects and pins its
  store; Action tab shows facts; Diff tab renders and collapses on toggle; store picker pins and
  clears the row selection, Follow live unpins; tree toggle, Raw view, filter
  ("No state matches the filter."), live ingest refresh, Pause/Resume, and Clear all behave as before.
  No server or browser console errors.

### How to validate

**Smoke:**

```bash
dotnet build tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj
dotnet fixie timewarp-state-devtools-tests
dotnet run --project source/timewarp-state-devtools/timewarp-state-devtools.csproj --urls http://127.0.0.1:5917
# in another shell, ingest one span, then open http://127.0.0.1:5917/
printf '{"Spans":[{"Name":"CounterState.IncrementCount","Status":"Ok","DurationMilliseconds":1.5,"StartTimeUnixMilliseconds":1762300000000,"Attributes":{"timewarp.state.action":"IncrementCount","timewarp.state.state_type":"CounterState","timewarp.state.store":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","timewarp.state.render":"interactive"},"Events":[{"Name":"state.snapshot","Attributes":{"snapshot.json":"{\\"Count\\":4}"}}]}]}' \
  | curl -s -X POST http://127.0.0.1:5917/ingest/spans -H 'content-type: application/json' -d @-
```

**Expect:** the Fixie run reports `108 passed`; the ingest returns `1`; the page shows one row
with a `live` store badge and an empty runtime cell, the State tab shows the CounterState tree, and
clicking the row, the Action / Diff tabs, the Theme button, the store picker, Pause, and Clear all
behave as they did before the split.

