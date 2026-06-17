# Fix persistence generated-handler registration under Mediator

## Description

Persistence (`[PersistentState]` auto-load) is **broken at runtime** after the Mediator migration. Root cause: **two source generators can't see each other's output.**

- TimeWarp's `PersistenceStateSourceGenerator` emits, per `[PersistentState]` state, a `LoadActionSet.Action` (an `IAction`) and its `Handler` (verified: `*_Persistence.g.cs` files are generated for BlueState/PurpleState in the test app).
- martinothamar/Mediator's generator scans the **original** syntax trees in the same compilation. It never sees the generated `LoadActionSet.Handler`, so it does not register it (verified: `Mediator.g.cs` has **zero** `LoadActionSet` references).
- At runtime, `StateInitializedNotificationHandler` does `Sender.Send(loadAction)` → Mediator has no handler for `LoadActionSet.Action` → throws / load silently fails.

This affects any feature whose **handlers are themselves source-generated** — currently persistence.

## Options (needs a design decision)

1. **Have the persistence generator emit Mediator-compatible registration.** Mediator resolves handlers it knows about; a generated handler it can't see won't be in its switch. Could the persistence generator instead emit a normal hand-written-style handler in *source* form the user includes? (defeats the generator's purpose.)
2. **Bypass Mediator for Load.** Don't dispatch load as an `IAction` through `ISender`; have `StateInitializedNotificationHandler` (or the post-processor) call `PersistenceService.LoadState(...)` directly and `Store.SetState(...)`. Removes the generated handler entirely — simplest, and aligns with code-review task 072 (replace string-based auto-load with a typed contract).
3. **Register the generated handler in DI manually** and dispatch via a path that resolves from DI rather than Mediator's compile-time switch (if Mediator supports a DI fallback for unknown request types — investigate; it may not).
4. **Make the persistence generator depend on Mediator's generator** ordering — not supported; generators are unordered and don't compose.

Recommendation: **Option 2** — it's the deepest fix, removes the source-generator-interaction problem entirely, and converges with tasks 072 (typed load contract) and 028/029 (persistence samples).

## Resolution (the registration gap is FIXED)

Took a refined Option 2: a **single hand-written `LoadPersistentStateRequest` + handler** in Plus (`source/timewarp-state-plus/features/persistence/load-persistent-state-request.cs`). Because it's hand-written (in source, not generator output), Mediator's generator sees and registers it — unlike the per-state generated handler. Both triggers dispatch it:
- `StateInitializedNotificationHandler` (auto-load on init) — simplified: removed the `AssemblyQualifiedName` string-mangling + `Type.GetType` + `Activator`; now just checks `[PersistentState]` and `Sender.Send(new LoadPersistentStateRequest(stateType))`.
- The source-generated `Load()` method — now sends the same request (generator no longer emits `LoadActionSet.Action`/`Handler`/`StateLoadedNotification`, only a thin `Load()` wrapper).

### Checklist
- [x] Approach decided (refined Option 2: hand-written request/handler, dispatched by both auto-load and `Load()`)
- [x] Implemented; removed reliance on a source-generated handler being registered by Mediator
- [x] **Verified the registration gap is gone**: in the running test app (Playwright, `/PersistenceTestPage`), the WASM console shows `StateInitializedNotificationHandler: PurpleState` → `PersistenceService Loading State for PurpleState` with **no `MissingMessageHandlerException`**. The load handler is registered (`Mediator.g.cs` has 9 `LoadPersistentStateRequest` refs) and runs.
- [x] Mediator `Send` confirmed to be a compile-time switch that throws `MissingMessageHandlerException` for unknown types (no DI fallback) — so registering the generated handler in DI could never have worked; the hand-written handler is required.

## ⚠️ Persistence still does NOT round-trip end-to-end — separate bug → task 065

The crash is fixed and the load now *runs*, but state is **not restored** on reload (PurpleState count 16 → 1 = its `Initialize()` value; new Guid). The data IS saved correctly (`localStorage["PurpleState"] = {"Count":6,"Guid":"..."}`, same key load uses), and `PersistenceService.LoadState` is invoked — so the remaining failure is on the **deserialize/apply** side: the load returns null or a defaulted object and never `SetState`s the restored value. That is **task 065** (serializer-options mismatch: save goes through Blazored's options, load uses a parallel `new JsonSerializerOptions()`; neither uses the configured `TimeWarpStateOptions.JsonSerializerOptions`). Full working persistence needs 065 (and possibly a prerender-timing pass for InteractiveAuto). This task (075) only owned the Mediator-registration gap, which is done.

## Notes

Discovered 2026-06-16/17 during task 049. The fix also simplifies the code (deletes the string-mangling auto-load and the dead generated LoadActionSet/StateLoadedNotification), converging with tasks 072 (typed load contract) and 071 (the generator's now-unused `ToCamelCase`/`persistentStateMethod` param can be removed there).
