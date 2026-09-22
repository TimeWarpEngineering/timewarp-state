---
uid: TimeWarpState:Persistence.md
title: Persistence
---

# Persist state in the browser

`[PersistentState]` on a top-level state tells TimeWarp.State.Plus to save that state after each of its actions and to load it when the store creates the state.

## Register

```csharp
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddGeneratedMediator<ClientPipeline>();
builder.Services.AddTimeWarpState(options =>
{
  options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddScoped<IPersistenceService, PersistenceService>();
```

```csharp
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor<,>), order: 520, Scope = typeof(ClientPipeline))]
```

`PersistentStatePostProcessor` is opt-in. Blazored is how the host selects session storage versus local storage. `PersistenceService` depends on both services. On .NET 10, alias `PersistentStateAttribute` to `TimeWarp.Features.Persistence.PersistentStateAttribute` so it does not collide with `Microsoft.AspNetCore.Components.PersistentStateAttribute`.

```csharp
[PersistentState(PersistentStateMethod.LocalStorage)]
public sealed partial class DisplayPreferencesState : State<DisplayPreferencesState>
{
}
```

Use `PersistentStateMethod.SessionStorage` for tab-scoped data and `LocalStorage` for values that should survive a new tab. `Server` and `PreRender` are not implemented.

## Load

The first `GetState` publishes `StateInitializedNotification`. `StateInitializedNotificationHandler` sends `LoadPersistentStateRequest` for types marked `[PersistentState]`. The generated `Load()` method on the state sends that same request when a page needs to read storage again.

The load replaces the store entry. It is not an action, so it does not go through render subscriptions. A page that needs the snapshot on first paint can await `IStore.StateInitializationTasks` for that state's full name.

## JSON and keys

Save and load use `TimeWarpStateOptions.JsonSerializerOptions`. Put converters there. Load deserializes with a case-insensitive copy of those options.

The write key is `Type.FullName`. Load tries the full name, then the simple name. There is no migration framework: a new constructor parameter needs a CLR default if older payloads omit it, and a payload that does not bind throws `JsonException`.

Neither store is a secret store. Any script on the origin can read it. Each action rewrites the whole state, so keep the persisted type small.

## Sample

[05-Persistence](../../samples/05-persistence/readme.md) is a WebAssembly workspace: a draft in session storage and display preferences in local storage.
