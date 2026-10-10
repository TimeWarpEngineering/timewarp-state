---
uid: TimeWarpState:Release.12.0.0-beta.11.md
title: Release 12.0.0-beta.11
---

## Release 12.0.0-beta.11

### Breaking changes

- **State clones are generated. The reflection cloner is removed.** `StateCloneSourceGenerator` emits the clone for each concrete `State<T>` and for types marked `[GenerateClone]`. `StateTransactionBehavior` calls `ICloneable.Clone` when the state implements it, and otherwise `StateCloneRegistry.Clone`. `DeepCloner`, `CloneExtensions.Clone<T>()`, and `CloneErrorHandler` are gone. A member the generator cannot clone is build error **TWSG002**. See the [12.0.0-beta.11 migration](xref:TimeWarpState:Migration12.0.0-beta.11.md) and [Cloning](xref:TimeWarpState:Cloning.md).

- **`TWS0009` no longer requires a parameterless constructor.** A concrete state that derives directly from `State<T>` must implement `ICloneable` or have a constructor the generator can call (public, or internal in the same assembly). Abstract states are exempt.

- **`IStore` no longer exposes `GetSemaphore` or `StateInitializationTasks`.** `GetState(Type)` returns `IState`. Wait with `WaitForInitializationAsync<TState>()`. `FindInitializationTask(Type)` looks up a state that already exists and does not create one. `Store.Reset` removes every state the way `RemoveState` does (cancel, drop the previous state, drop the initialization task).

- **`OperationCanceledException` propagates after a transaction rollback.** It is not published as `ExceptionNotification`. Other handler exceptions are still published and, by default, swallowed so `Send` returns the default response. Set `TimeWarpStateOptions.RethrowHandlerExceptions` to rethrow them after the notification. See [State transactions](xref:TimeWarpState:StateTransactions.md).

- **`PersistentStateMethod` is `SessionStorage` and `LocalStorage` only.** `PreRender` and `Server` are removed. Register persistence with `AddTimeWarpStatePersistence()`. `ISessionStorageService` and `ILocalStorageService` are optional constructor dependencies. A missing store logs a warning and that path is skipped.

- **`AddTimeWarpStateBlazor` does not register `HttpClient`.** A server host that injects one registers it, for example with `AddHttpClient` or a scoped `HttpClient` whose `BaseAddress` is `NavigationManager.BaseUri`.

- **Test-only state members require `StateTestOptions.Enable()`.** An assembly name that contains "test" is no longer enough.

- **Analyzer ids `TWS0009` through `TWS0012`.** `TWS001` is `TWS0009`. The three prose ids are `TWS0010` (inheritance type argument), `TWS0011` (sealed state), and `TWS0012` (read-only public properties, now symbol-based). They stay in `AnalyzerReleases.Unshipped.md` until the 12.0 release.

- **`PushRouteInfo` reads `document.title` from `document-title.js`.** It no longer calls JavaScript `eval`. A disconnected circuit records an empty title and still pushes the URL.

### Other changes

- `State<T>.Guid`, `Sender`, and `CancellationTokenSource` stay unshared. Ignored members are `IgnoreDataMember`, `NonSerialized`, and `JsonIgnore`, including on backing fields.
- `[CloneShared]` on a field or auto-property copies that member by reference. Use it for an injected service (`ILogger<T>`, `HttpClient`, `NavigationManager`, `IJSRuntime`). An ignore attribute leaves the constructor value, which is null for a service parameter. `[CloneShared]` wins when a member has both.
- `TimeWarp.State` builds with `IsAotCompatible=true`, and trim/AOT warnings (IL2xxx/IL3xxx) are build errors in that project. The remaining reflection is suppressed in place with a justification and is a follow-up: the state assembly scan in `AddTimeWarpState` (`EnsureStates`, IL2026 and IL2072), Redux DevTools time travel in `Store.LoadStatesFromJson` (IL2026, IL3050) and `LoadStateFromJson` (IL2026, IL2070, IL2072, IL2075, IL3050), and `GetInterfaces` in `LogTimeWarpStateMiddleware` (no warning). `MethodInfoExtensions.InvokeAsync` is removed.
- The generated clone covers fields declared on generic types, tuples, `KeyValuePair`, `Nullable<T>` structs, sorted, linked, concurrent, and read-only collections, `StringBuilder`, and BCL values held behind collection interfaces (copied into `List<T>`, `HashSet<T>`, or `Dictionary<TKey,TValue>` when the type is unknown). Polymorphic members keep their runtime type. See [Cloning](xref:TimeWarpState:Cloning.md).
- A type from another assembly is cloned only when the generator can see all of its fields. DTOs from a sibling project that are not plain auto-property types need `ProduceReferenceAssembly=false` on that project, or `ICloneable`.

### Fixes

- Clones of a state that derives from a base in another assembly copy that base's public auto-properties. `TimeWarpCacheableState<T>.CacheKey` and `TimeStamp` stay intact across actions, so a fresh cache is not treated as a miss.
