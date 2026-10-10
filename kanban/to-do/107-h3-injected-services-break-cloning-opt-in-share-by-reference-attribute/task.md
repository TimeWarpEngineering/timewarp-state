# Task 107: H3: Injected services break cloning (opt-in share-by-reference attribute)

## Description

The generated cloner (task 097) has no share-by-reference opt-in. The documented workaround,
`[IgnoreDataMember]` on a service field, leaves the service null in the clone, which becomes the live state,
so the next action crashes with a `NullReferenceException`. Third in Fable's order.

Filed 2026-10-10 at Steven's request (relayed by Amina) from Claude Fable's full codebase review,
task 102 (`kanban/done/102-full-codebase-review-by-claude-fable-review-only/`, PR #629, `00ade373`).
Not launched.

## Finding H3 (Fable review)

Verbatim from `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md` lines 41-46 (PR #629, merged as `00ade373`):

> ### H3. Generated cloner has no "share by reference" opt-in, and the documented workaround nulls injected services on the live state
>
> - `source/timewarp-state-source-generator/state-clone-planner.cs:1082` fails the build (TWSG002) for a member whose type is an interface with no visible implementation, which is every injected service (`ILogger<T>`, `HttpClient` fails the metadata check instead, `NavigationManager`, `IJSRuntime`). The only escape hatches are `ICloneable` on the whole state or `[IgnoreDataMember]` on the member.
> - `documentation/topics/cloning.md` ("Constructors") and `documentation/migrations/migration12.0.0-beta.11.md` both tell authors to mark service fields `[IgnoreDataMember]` or `[JsonIgnore]` and make the constructor accept `null`. The clone is constructed with `default!` arguments (`state-clone-planner.cs:1237`, `TryEmitConstruction`), ignored members "keep the value the constructor set", which is null, and `StateTransactionBehavior` makes that clone the live state. The next handler on that state dereferences a null service.
> - Why it matters: states that hold an injected `HttpClient` or `NavigationManager` are the common pre-beta.11 pattern (the cacheable weather state in the test app would be one if it stored the client). Following the migration guide moves the failure from compile time to the first action after the first action, with a `NullReferenceException` whose stack does not mention cloning. The `InvalidCloneException` message (`invalid-clone-exception.cs`) points authors at the same attributes.
> - Suggestion: add an explicit per-member opt-in to copy by reference, for example `[CloneShared]` in `TimeWarp.State` (or treat `[IgnoreDataMember]` as "copy by reference" for reference-typed members, which is closer to what AnyClone users expect). Alternatively have `StateTransactionBehavior` copy every `[IgnoreDataMember]` reference member from the original after cloning, the way it already copies `Sender`. Update the two docs and the exception message to the chosen rule, and add a shape test with an `ILogger` field.

## Order

Fable's recommended order (verbatim, `kanban/done/102-full-codebase-review-by-claude-fable-review-only/review-findings.md`, Overall assessment):

> Recommended order of attack: the package dependency (one line), the transaction rollback guard (one `ReferenceEquals` check), a `[CloneShared]`-style attribute or equivalent for injected members, then the DevTools and persistence dead paths, then `IsAotCompatible` on the two remaining runtime packages.

Filed tasks in that order: 105 (H1, package dependency) -> 106 (H2, rollback guard) -> 107 (H3, share-by-reference attribute) -> 104 and 109 M1/M9 (dead DevTools and persistence paths) -> 109 M2 (AOT checks on Blazor and Plus). 108 (H4) and the remaining Medium items are not in Fable's ordered list; schedule them after 107.

## Requirements

- An explicit per-member opt-in to copy by reference, e.g. `[CloneShared]` in `TimeWarp.State`, or one of
  Fable's alternatives (treat `[IgnoreDataMember]` on reference members as copy-by-reference, or have
  `StateTransactionBehavior` copy `[IgnoreDataMember]` reference members from the original after cloning).
- Update `documentation/topics/cloning.md` ("Constructors"), `documentation/migrations/migration12.0.0-beta.11.md`
  and the `InvalidCloneException` message to the chosen rule.
- Shape test with an `ILogger` field.

## Checklist

- [x] Choose the rule (attribute vs alternatives) and record it here
- [x] Generator support (`source/timewarp-state-source-generator/state-clone-planner.cs`, TWSG002 path `:1082`)
- [x] Docs: cloning topic + beta.11 migration guide
- [x] `invalid-clone-exception.cs` message
- [x] Tests: `ILogger<T>` field shape test; injected-service state survives two consecutive actions
- [x] Code review

### Rule

`[CloneShared]` (`TimeWarp.State.CloneSharedAttribute`) on a field or auto-property assigns that member from the source. The generator does not walk the member type, so an injected `ILogger<T>`, `HttpClient`, or `NavigationManager` compiles without TWSG002 and stays the same instance on the live state. `[IgnoreDataMember]`, `[NonSerialized]`, and `[JsonIgnore]` keep the constructor value. `[CloneShared]` wins when a member has both. The ignore attribute applies to serialization. Construction passes `default` for service parameters, so the constructor must accept null.

## Acceptance criteria

- A state with an injected `ILogger<T>` (and e.g. `NavigationManager`) field marked with the opt-in compiles
  without TWSG002, and after an action the live state's service member is the same non-null instance.
- Docs and exception message describe the same rule.
- Generator and state tests green; `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)
- Implementation: 2026-10-11 (implementer oracle, `[CloneShared]`)
- Review: 2026-10-11 (review oracle Claude Opus 5.5; general reviewer subagent aeae3ae5348e3b8b8)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-10T17:54:36Z

## Results

`[CloneShared]` is the share-by-reference opt-in. The generated cloner assigns a marked field or auto-property from the source and does not classify that type, so `ILogger<T>`, `HttpClient`, and an abstract navigation type no longer produce TWSG002. After each action, `StateTransactionBehavior` installs a new state whose service members are the original instances. Ignored members keep the constructor value. `ClockState` proves that `[IgnoreDataMember]` leaves the service null.

### How to validate

Smoke:

```bash
dotnet test tests/timewarp-state-source-generator-tests/timewarp-state-source-generator-tests.csproj --nologo
dotnet test tests/timewarp-state-tests/timewarp-state-tests.csproj --nologo
```

Expect:

- `StateCloneSourceGenerator_.Should_Compile_Supported_Shape.Given_CloneShared_Assigns_The_Source_Reference` passes. The generated source contains `F_Logger_`, `F_HttpClient_`, and `F_Navigation_`, and it does not contain `Clone_ILogger`, `Clone_HttpClient`, or `Clone_AppNavigation`.
- `ServiceWithoutCloneShared` is TWSG002 at the source member.
- `GeneratedCloneShapeTests.Should_.Share_Injected_Services_By_Reference` passes. The clone's `ILogger<ServiceState>`, `HttpClient`, and `NavigationManager` are the same non-null instances, and `Guid` differs.
- `StateTransactionBehaviorTests.Should_.Keep_Injected_Services_Across_Two_Actions` passes. After two actions the live state is a new instance, `Count` is 2, and the three services are the originals.
- Both suites pass (generator 78, state 106 passed and 1 skipped on 2026-10-11). `./bin/dev check-version` is clean at `12.0.0-beta.11` (ahead of published `12.0.0-beta.10`).

### Review disposition

- Rounds: 1. Effort 2, roster: general.
- Final counts: bug 0. Suggestion 2 fixed. Nit 1 fixed and 2 wontfix. 0 open.
- Disposition: **accepted-exceptions**. M4 (redundant struct assignment) and M5 (simple-name attribute match, consistent with the ignore attributes) are wontfix.
- Fixes: `CloneSharedPrecedence` shape test. Hidden metadata private fields marked `[CloneShared]` now get an accurate TWSG002 reason. cloning.md notes that non-auto properties need the attribute on the backing field.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Notes

- Source: 102 review-findings.md finding H3. Related batch: 109 (Medium findings).
