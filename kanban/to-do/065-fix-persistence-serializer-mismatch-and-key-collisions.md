# Fix persistence serializer mismatch and key collisions

## Description

Code review 2026-06-11, findings 5 and 18 (`code-review-2026-06-11.md`).

Three defects in the `[PersistentState]` save/load round-trip in `source/timewarp-state-plus/features/persistence/`:

1. **Serializer options mismatch:** Save goes through Blazored `SetItemAsync` (`persistent-state-post-processor.cs:53,63`), serialized with whatever options `AddBlazoredSessionStorage()`/`AddBlazoredLocalStorage()` were given (typically defaults). Load deserializes with a locally constructed `private readonly JsonSerializerOptions JsonSerializerOptions = new();` (`persistence-service.cs:5,52`). Neither side honors the user-configured `TimeWarpStateOptions.JsonSerializerOptions` used by `Store` (`store.cs:37`) and `JsonRequestHandler` (`json-request-handler.cs:22`). States needing custom converters or enum-as-string fail to round-trip; any Blazored-side customization breaks loading outright.
2. **Key collision:** the storage key is the state's *simple* type name (`stateType.Name` in `persistence-service.cs:25–26`, `currentType.Name` in the post-processor). Two `[PersistentState]` classes with the same simple name in different namespaces share one storage slot — last write wins, the other cross-hydrates from foreign JSON or throws `JsonException`.
3. **Eager serialization in log arguments:** `persistent-state-post-processor.cs:46–62` passes `JsonSerializer.Serialize(state)` as a `LogTrace` template argument with no `IsEnabled` guard — a full extra serialization of the state on every persisted action even when Trace is off. Lines 29–32 also run `GetEnclosingStateType()`/`GetCustomAttribute<PersistentStateAttribute>()` reflection on every action of every type.

## Checklist

- [ ] Inject `TimeWarpStateOptions` and use its `JsonSerializerOptions` on both serialize and deserialize sides (serialize to string manually instead of Blazored's object overload)
- [ ] Key storage entries by `FullName` (decide migration story for entries already stored under simple names)
- [ ] Guard the LogTrace serializations with `Logger.IsEnabled(LogLevel.Trace)`
- [ ] Cache `GetEnclosingStateType()` + attribute lookup in a `static readonly` field of the closed generic post-processor
- [ ] Round-trip test with a custom converter / enum-as-string state
