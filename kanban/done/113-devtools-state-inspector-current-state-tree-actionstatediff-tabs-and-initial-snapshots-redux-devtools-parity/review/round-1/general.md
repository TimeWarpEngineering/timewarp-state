# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** source/timewarp-state-devtools + devtools tests

## Summary
Fold, diff and tree logic is sound for objects, arrays, nulls and truncated or invalid JSON, and the Fixie suite passes (45). All requirements in task.md are met, and no `<style>` element was found in any component. Remaining defects: trimming the log drops an idle state from the "current state" fold, the pause/resume sequence can leave `Entries` out of order, and the tree is re-parsed on every render.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-state-devtools/inspector-log.cs:213 (RebuildLatest / AppendVisible / FlushPending, ~line 269)
- Description: `LatestByStateType` is rebuilt from the trimmed `Entries` list whenever `Trim` evicts anything. A state whose only snapshot (for example its `init` row) is older than the last 500 entries disappears from the Current state tree, although it is still the current value. With a chatty state, the idle state's node vanishes after about 500 spans. This contradicts the "latest per state" index in task.md and the "panel complete on connect" goal. The Design region acknowledges the eviction but does not treat it as a problem.
- Suggestion: Keep the per-state latest index independent of the display cap. Do not rebuild it on trim; clear it only in `Clear`. Alternatively, retain the last snapshot entry per state when trimming. Add a test that adds more than `MaxEntries` for state B after one snapshot of state A, then expects `FoldCurrent` to still contain A.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state-devtools/features/inspector/inspector-state.toggle-pause.cs:32 with source/timewarp-state-devtools/inspector-log.cs:200
- Description: Resume calls `SetPaused(false)` and `FlushPending()` as two separate lock acquisitions. An ingest `Add` between them goes straight to `Entries` with a higher sequence. `FlushPending` then appends the older pending entries after it. `Entries` is no longer sorted by `Sequence`, but `FoldThrough` and `TryGetPreviousSnapshot` rely on that order (`break` on `Sequence >`). `FlushPending` also overwrites `LatestByStateType` with the older snapshot, so the current tree can show a stale state until the next snapshot. The window is narrow, and the race predates this task, but the new fold logic now depends on the ordering.
- Suggestion: Make resume atomic, for example `SetPaused(false)` flushes `Pending` inside the same lock. Alternatively, have `FlushPending` insert in sequence order.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-state-devtools/components/inspector/FoldedStateNode.razor:11 (and components/pages/Inspector.razor `DiffFor` / `VisibleStates`)
- Description: Every render re-parses the full snapshot JSON of every state with `StateTree.Parse`, and re-parses both sides for the diff. Each ingested span raises `Changed` and re-renders the page. With snapshots up to 64 Ki per state and bursts of spans, this is repeated allocation and parse work per span per circuit, and the log filter and fold also run each time. This misses the "O(states)" intent. `StateTree.Parse` and `StateDiff.Compare` also catch only `JsonException`, and the parse happens in markup, so any other exception thrown while parsing would take down the circuit.
- Suggestion: Cache the parsed `Document` on `FoldedState`, or key a small cache by (state type, sequence). Optionally coalesce re-renders.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-state-devtools/features/inspector/state-diff.cs:36
- Description: When only the previous snapshot is truncated, `Compare` returns `Status.Truncated` with the hint "Snapshot truncated … Showing raw text" and the raw after-JSON. This hides a perfectly parseable selected snapshot. The message also implies the selected snapshot is the truncated one.
- Suggestion: Say which side is truncated. Optionally, when only `beforeTruncated` is true, show the after-JSON as an initial/added view with a note that the earlier snapshot was truncated.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-state-devtools/components/inspector/StateDiffView.razor:7 and source/timewarp-state-devtools/features/inspector/inspector-state.cs:~75
- Description: The diff view shares expansion paths with the State tree, and only root paths default to open. Nested change nodes (`Root.Child.Leaf`) start collapsed, so a diff that changes a deep value shows only the collapsed top-level branch. The user must click through to see any change. Because the paths are shared, expanding a node in Diff also expands it in the State tree, and the reverse. This may be intended. Separately, `ExpandedNodes` only grows (stale paths from array indices are never pruned).
- Suggestion: Default diff branches to open. Use a distinct key prefix for diff paths, or accept the sharing and document it. Optionally cap or prune `ExpandedNodes`.
- Status: open

### Issue 6 — Severity: nit
- File: source/timewarp-state-devtools/components/pages/Inspector.razor.css:49,93,126,188,243,248
- Description: Duplicate rule blocks (`.tab` and `.tab:hover` twice, `.detail` twice, `.hint` twice) that likely override each other. `InspectorLog.TryGetPreviousSnapshot` does not skip entries with an empty state type name, whereas `Note` ignores them, so the fold and the diff handle the empty state type differently. Theme state resets to "system" on circuit reconnect while `<html data-theme>` keeps the old value, so the toggle label can disagree with what is shown.
- Suggestion: Merge the duplicate CSS blocks. Guard the empty state type name in `TryGetPreviousSnapshot`. Re-apply the theme in `OnAfterRender(firstRender)` or read it back from the document.
- Status: open
