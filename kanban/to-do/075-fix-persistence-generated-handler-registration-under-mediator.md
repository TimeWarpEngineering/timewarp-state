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

## Checklist

- [ ] Decide the approach (recommend Option 2: load directly via PersistenceService, no generated IAction/handler through Mediator)
- [ ] Implement; remove reliance on a source-generated handler being registered by Mediator
- [ ] Verify in the test app: BlueState/PurpleState persist + auto-load across reloads (PersistenceTestPage / ServerSidePersistenceTestPage)
- [ ] Coordinate with task 072 (typed load contract) and 065 (serializer/key fixes)

## Notes

Discovered 2026-06-16/17 during task 049 (run tests and fix issues) — the test app builds and the counter pipeline works, but persistence is the one feature broken at runtime, and it's a structural generator-interaction issue rather than a mechanical fix.
