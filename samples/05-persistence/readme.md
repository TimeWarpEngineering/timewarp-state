# Persistence sample

Blazor WebAssembly workspace that stores two TimeWarp.State types in the browser.

| State | Attribute | Store | Survives |
| --- | --- | --- | --- |
| `DraftNoteState` | `PersistentStateMethod.SessionStorage` | Blazored session storage | Refresh in the **same tab** |
| `DisplayPreferencesState` | `PersistentStateMethod.LocalStorage` | Blazored local storage | New tabs and a browser restart, until the origin's local storage is cleared |

The attribute chooses the store. The host chooses the implementation by calling `AddBlazoredSessionStorage` and `AddBlazoredLocalStorage`. `PersistenceService` takes both services, so this sample registers both.

## Run

Pack the in-tree packages first when `artifacts/packages` does not already contain this repo's `TimeWarp.State` version:

```bash
./bin/dev pack
dotnet run --project samples/05-persistence/wasm/sample-05-wasm/sample-05-wasm.csproj --launch-profile http
```

NuGet keeps the first copy of `12.0.0-beta.4` it restores. A later pack of that same version does not replace `~/.nuget/packages/timewarp.state/12.0.0-beta.4` or `timewarp.state.plus`. Delete those two version folders and restore again so the sample binds the packed assemblies. The running app then writes `Type.FullName` keys and camelCase JSON.

Open `http://localhost:5295`.

## What to try

1. Type a note and leave the field (the change event dispatches `UpdateText`). In the browser's Application tab, session storage has a key equal to `DraftNoteState`'s full name.
2. Pick an accent and toggle density. Local storage has a key equal to `DisplayPreferencesState`'s full name. The JSON uses camelCase property names and the accent enum as a string, because save and load share `TimeWarpStateOptions.JsonSerializerOptions`.
3. Reload the tab. Both values come back. The page awaits `IStore.StateInitializationTasks` so the first render is the loaded snapshot. That task is `StateInitializedNotification`, and the handler sends `LoadPersistentStateRequest` (not a generated per-state load action).
4. Open a second tab to the same URL. The accent is still there. The draft is empty. Session storage is per tab.
5. **Reload from session storage** / **Reload from local storage** call the generated `Load()` method, which sends `LoadPersistentStateRequest` again.

## Session storage or local storage

Use **session storage** for data that belongs to one tab: an unsaved draft, a wizard step, a filter you do not want in the next tab. The browser drops it when the tab closes.

Use **local storage** for preferences that should stick: theme, density, last-used view. It is shared by every tab on the origin. This library does not push a write from one tab into another; the other tab sees it on its next load.

Both stores are readable by any script on the origin. Do not persist credentials, tokens, or anything you would refuse to put in a non-HttpOnly cookie. Keep the persisted type small: every action on that type serializes the whole state and writes one string.

`PersistentStateMethod.Server` and `PersistentStateMethod.PreRender` are not implemented. This sample does not use them.

## Keys and shape changes

There is no migration framework in TimeWarp.State. Do not add one beside this sample.

- Writes use `Type.FullName` (`PersistentStateStorageKey`).
- Load tries that full name, then the simple `Name`, so an older entry stored under the simple name still hydrates. The next save writes the full name.
- Two types with the same simple name no longer share a write key.
- JSON is `TimeWarpStateOptions.JsonSerializerOptions`. Load uses a case-insensitive copy so a leftover PascalCase payload can still bind.
- A new constructor parameter with a CLR default still deserializes when an older payload omits it. `DisplayPreferencesState` does this for `compact` (`false`). A member the converter cannot read (an unknown accent name) throws `JsonException` from `PersistenceService`. The library does not rewrite the payload.

## Host checklist

- `[PersistentState(PersistentStateMethod.SessionStorage | LocalStorage)]` on a **top-level** state. Nested states are TWSG001.
- `[JsonConstructor]` plus a parameterless constructor (dependency injection and TWS001).
- `[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor<,>), order: 520, Scope = typeof(ClientPipeline))]`.
- `AddGeneratedMediator<ClientPipeline>()`.
- `AddScoped<IPersistenceService, PersistenceService>()`.
- `UseReduxDevTools()` so the linked `CommitHandler` can be constructed when the Development host validates DI. This sample does not render `<ReduxDevTools />`, so the extension is not initialized.
- Alias `PersistentStateAttribute` to `TimeWarp.Features.Persistence.PersistentStateAttribute`. .NET 10 also defines `Microsoft.AspNetCore.Components.PersistentStateAttribute`.
