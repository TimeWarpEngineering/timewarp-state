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

- [ ] Choose the rule (attribute vs alternatives) and record it here
- [ ] Generator support (`source/timewarp-state-source-generator/state-clone-planner.cs`, TWSG002 path `:1082`)
- [ ] Docs: cloning topic + beta.11 migration guide
- [ ] `invalid-clone-exception.cs` message
- [ ] Tests: `ILogger<T>` field shape test; injected-service state survives two consecutive actions
- [ ] Code review

## Acceptance criteria

- A state with an injected `ILogger<T>` (and e.g. `NavigationManager`) field marked with the opt-in compiles
  without TWSG002, and after an action the live state's service member is the same non-null instance.
- Docs and exception message describe the same rule.
- Generator and state tests green; `ganda repo audit` clean.

## Session

- Created: 2026-10-10 (Grok Bot, at Steven's request via Amina; not launched)

## Notes

- Source: 102 review-findings.md finding H3. Related batch: 109 (Medium findings).
