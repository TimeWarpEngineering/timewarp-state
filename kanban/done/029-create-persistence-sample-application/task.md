# Task 029: Create Persistence Sample Application

## Description

- Create a sample application demonstrating the Persistence features of TimeWarp.State.
- This sample will showcase how to implement and use state persistence in a TimeWarp.State application.
- Demonstrate both Session and Local storage options for state persistence.

## Requirements

- Create a new sample project in the Samples directory.
- Implement a simple application state that can be persisted.
- Demonstrate how to save state to both Session storage and Local storage.
- Demonstrate how to load persisted state from both storage options on application startup.
- Showcase how to handle state migrations if the state structure changes.
- Include clear comments and documentation within the sample code.
- Provide examples of when to use Session storage vs Local storage.

## Current API (065 / 075 — do not invent another stack)

- `[PersistentState]` on the state type. Auto-load is `LoadPersistentStateRequest` (not `LoadActionSet` string dispatch).
- Save/load use `TimeWarpStateOptions.JsonSerializerOptions`. Keys are `Type.FullName` (load falls back to simple name).
- Blazored session vs local is how the host registers storage. Show both.
- There is **no** library migration framework. Do not build one. README may note the FullName/Name fallback only.
- Mediator is generated `AddGeneratedMediator` / named pipelines (080), not reflection `AddMediator`.
- Match existing sample layout under `samples/`. PackageReference the local version, not a made-up NuGet.

## Session

- 2026-09-22: cockpit — next after 058. Dispatch implementer-grok.
- 2026-09-22: implementer-grok — sample `samples/05-persistence` (session draft, local display preferences).
- Reviewer: grok session 01a0c894-383b-72e3-8da6-8487256b705e (2026-09-22) — effort 1, general. Artifacts under `review/`.

## Checklist

### Design
- [x] Design the sample application structure
- [x] Define the state model to be persisted
- [x] Plan the persistence logic for both Session and Local storage
- [x] Design state loading/saving flow for both storage options

### Implementation
- [x] Create new sample project in the Samples directory
- [x] Implement the application state
- [x] Add persistence logic using TimeWarp.State features for Session storage
- [x] Add persistence logic using TimeWarp.State features for Local storage
- [x] Implement state loading on application startup for both storage options
- [x] Add example of state migration handling
- [x] Ensure all persistence features are properly demonstrated for both storage types

### Documentation
- [x] Add inline comments explaining key concepts and implementation details
- [x] Create a README.md file for the sample project explaining how to run and use the sample
- [x] Update main documentation to reference the new Persistence sample
- [x] Document the differences and use cases for Session vs Local storage

### Review
- [x] Verify that all persistence features are correctly implemented for both storage options
- [x] Ensure the sample is easy to understand and follow
- [x] Consider performance implications of persistence implementation
- [x] Consider security implications of persisting state in different storage options
- [x] Conduct code review

## Notes

- Refer to the existing TimeWarp.State documentation on persistence features.
- Consider common use cases for state persistence in Blazor applications.
- Highlight scenarios where Session storage might be preferred over Local storage and vice versa.
- Review kitchen: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`. Effort 1 (general). Outcome clean.

## Implementation Notes

- WebAssembly sample `samples/05-persistence/wasm/sample-05-wasm`. `DraftNoteState` uses `PersistentStateMethod.SessionStorage`. `DisplayPreferencesState` uses `LocalStorage`.
- The host registers both `AddBlazoredSessionStorage` and `AddBlazoredLocalStorage`. `PersistenceService` takes both services.
- Startup waits on `IStore.StateInitializationTasks` so the first paint is the loaded snapshot. `LoadPersistentStateRequest` is the load contract. The page's Reload buttons call the generated `Load()`.
- Shape change: `compact` has a CLR default on the JSON constructor. Key fallback is FullName then simple name, documented in the sample README and on the page. No migration type was added.
- Each action rewrites the whole state. The README says to keep the persisted type small and to keep credentials out of both stores.
- PackageReference is `TimeWarp.State` / `TimeWarp.State.Plus` at `TimeWarpStateVersion` (`12.0.0-beta.4`), restored from `artifacts/packages` after pack.

## Results

Blazor WebAssembly sample under `samples/05-persistence`. One tab-scoped draft in session storage and origin-scoped accent plus density in local storage. Writes use `Type.FullName` and `TimeWarpStateOptions.JsonSerializerOptions` (camelCase, accent as a string). Load tries that full name, then the simple name. A payload that omits `compact` deserializes as `false`.

`dotnet build samples/05-persistence/wasm/sample-05-wasm/sample-05-wasm.csproj` succeeded with 0 warnings. Browser check against the packed `12.0.0-beta.4` Plus assembly: a simple-name camelCase draft and an Ember payload without `compact` hydrated on startup; editing wrote the full-name camelCase keys (`"text":"tab draft"`, `"accent":"Sea"`, `"compact":true`); reload kept both; a second tab had an empty draft and the same accent; Clear and Reload from session storage round-tripped; no page or console errors; layout fit at 390px and 1280px.

### How to validate

**Smoke**

```bash
./bin/dev pack
rm -rf ~/.nuget/packages/timewarp.state/12.0.0-beta.4 ~/.nuget/packages/timewarp.state.plus/12.0.0-beta.4
dotnet build samples/05-persistence/wasm/sample-05-wasm/sample-05-wasm.csproj
dotnet run --project samples/05-persistence/wasm/sample-05-wasm/sample-05-wasm.csproj --launch-profile http
```

Open `http://localhost:5295`.

**Expect**

- Build exits 0.
- The page shows a Draft note panel (session storage) and a Display preferences panel (local storage). Each key line is the state's full name.
- Type a note and leave the field. Session storage gains `Sample05Wasm.Features.DraftNote.DraftNoteState` with camelCase `text`.
- Choose Sea and compact density. Local storage gains `Sample05Wasm.Features.DisplayPreferences.DisplayPreferencesState` with `"accent":"Sea"` and `"compact":true`.
- Reload the tab. The note, Sea, and compact density are still there.
- Open a second tab to the same URL. The accent and density match. The draft is empty.
- Clear empties the note and the stored `text`. Reload from session storage reads whatever is stored under the full name.
- DevTools may show an older entry under the simple name `DraftNoteState` or `DisplayPreferencesState`. Load uses that only when the full-name entry is absent. A preferences payload that omits `compact` comes back as comfortable density.

### Review

Round 1, effort 1, reviewer `general` (grok session `01a0c894-383b-72e3-8da6-8487256b705e`).

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

Disposition: **clean**. No wontfix items and no escalations.

- Framework: `review/review-framework.md`
- Last merged: `review/round-1/merged.md`
- Disposition: `review/disposition.md`
