# Fix persistence serializer mismatch and key collisions

## Description

Code review 2026-06-11, findings 5 and 18. Still true on master after 080 (only `ISender<ClientPipeline>` + optional Blazored changed). E2E `persistence-test-page-tests` is ignored pending this id.

1. **Serializer mismatch:** Save uses Blazored `SetItemAsync(name, state)` (Blazored’s JSON options). Load uses `new JsonSerializerOptions()` in `PersistenceService`. Neither uses `TimeWarpStateOptions.JsonSerializerOptions` (`Store` / `JsonRequestHandler`).
2. **Key collision:** simple `stateType.Name` / `currentType.Name`. Throw message already says “full name” while using `Name`.
3. **Hot path:** `GetEnclosingStateType()` + `GetCustomAttribute` on **every** `IAction`. `LogTrace` is already `IsEnabled`-guarded; the serialize in that log still uses default JSON options.

## Requirements

- Inject `TimeWarpStateOptions`; serialize/deserialize with `JsonSerializerOptions`. Save via **string** (`SetItemAsStringAsync`), load already `GetItemAsStringAsync`.
- Storage key: `FullName` (throw if null). On **load**, try FullName then **fall back to `Name`** so existing session/local entries are not dropped. Document that new writes use FullName.
- Cache enclosing state type + `[PersistentState]` per closed generic `PersistentStatePostProcessor<TRequest, TResponse>` (`static readonly`).
- Round-trip test: custom converter and/or enum-as-string. Un-ignore E2E persistence only if that path is actually green.
- Keep optional Blazored (skip + warning when storage not registered).
- Disambiguate `PersistentStateAttribute` vs ASP.NET’s (already aliased).

## Out of scope

- PreRender / Server `PersistentStateMethod` TODOs
- Nested `[PersistentState]` generator (task **071**, after this)
- Deleting dead `LoadActionSetNotFound` EventId (may ride 071)

## Checklist

- [ ] Same `JsonSerializerOptions` on save and load
- [ ] Keys: write FullName; load FullName then Name
- [ ] Cache attribute/enclosing-type on the closed generic
- [ ] Round-trip test with custom JSON
- [ ] `dev test` / E2E persistence status recorded in Results

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk brief; dispatch after 072 archived. Then smaller 071.
