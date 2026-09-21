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

- [x] Same `JsonSerializerOptions` on save and load
- [x] Keys: write FullName; load FullName then Name
- [x] Cache attribute/enclosing-type on the closed generic
- [x] Round-trip test with custom JSON
- [x] `dev test` / E2E persistence status recorded in Results

## Session

- Created: code review 2026-06-11
- 2026-09-21: cockpit shrunk brief; dispatch after 072 archived. Then smaller 071.
- Implementer: grok session 01a0c305-d2f3-7953-ab87-d9233f168c52 (2026-09-21)

## Results

Save and load now share `TimeWarpStateOptions.JsonSerializerOptions`. The post-processor serializes to a JSON string and writes with `SetItemAsStringAsync` under `Type.FullName`. `PersistenceService` deserializes with the same options, trying FullName then the simple `Name` so leftover browser entries are not dropped. Enclosing state type and `[PersistentState]` are `static readonly` on each closed generic `PersistentStatePostProcessor<TRequest, TResponse>`. Blazored storage stays optional (skip + warning). `PersistentStateAttribute` vs ASP.NET remains aliased.

**Files**

- `source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs`
- `source/timewarp-state-plus/features/persistence/services/persistence-service.cs`
- `source/timewarp-state-plus/features/persistence/persistent-state-storage-key.cs`
- `source/timewarp-state/features/persistence/attributes/persistent-state-attribute.cs`
- `source/timewarp-state-plus/readme.md`
- `tests/timewarp-state-plus-tests/features/persistence/persistence-round-trip-tests.cs`
- `tests/test-app-end-to-end-tests/persistence-test-page-tests.cs` (still `[Ignore]`)

**Tests**

- `dotnet run --file ./scripts/test.cs` — pass (plus-tests 24 passed / 1 skipped, including the five `PersistenceRoundTrip_Should` cases).
- E2E `TestPersistence` left ignored. `UseHttp=true dotnet run --file ./scripts/e2e.cs` could not launch Chromium: Playwright does not support chromium on ubuntu26.04-x64 (`chromium_headless_shell-1187` missing; `playwright.ps1 install chromium` refused). Per the brief, do not un-ignore unless the browser path is actually green.

**Deviations**

- Hot-path cache uses `TryGetEnclosingStateType` (skip non-nested `IAction`) instead of throwing `GetEnclosingStateType` during static init.
- Name-key fallback is for the **storage key** only. New JSON is camelCase from TimeWarp options; leftover PascalCase Blazored payloads under `Name` may not bind unless the host sets `PropertyNameCaseInsensitive`.

### How to validate

**Smoke**

```bash
dotnet fixie timewarp-state-plus-tests
```

**Expect**

- 24 passed, 1 skipped.
- `PersistenceRoundTrip_Should.Round_Trip_Enum_As_String_Under_FullName_Key` writes `"kind":"Beta"` under `typeof(LocalWidgetState).FullName` and reloads `WidgetKind.Beta`.
- `Load_Falls_Back_To_Simple_Name_Key` hydrates from the simple `Name` key when FullName is empty.
- `Skip_Save_When_Storage_Is_Not_Registered` returns without throwing.

**Automated gate**

```bash
dotnet run --file ./scripts/test.cs
# expect: exit 0; plus-tests include PersistenceRoundTrip_Should
```

**Not in scope:** Playwright `/PersistenceTestPage` (Chromium not installable on this host; PreRender/Server TODOs remain). On a Playwright-capable runner, un-ignore `TestPersistence` and run `UseHttp=true dotnet run --file ./scripts/e2e.cs`.
