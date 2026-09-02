# Round 2 — general
**Date:** 2026-09-03
**Scope reviewed:** fix commit a0e0d707 (round-1 fixes) on top of 9d05efa5, vs origin/dev

## Summary

All nine `fixed` dispositions (M1–M5, M7–M10) verify against the fix diff and the current tree, and the
M6 `wontfix` rationale is factually accurate: `tests/test-app/test-app-client/generated` has been tracked
since `3deabecc` and was last refreshed in `ff036567`, both ancestors of `origin/dev`, so the tracking
state does predate this branch. I re-measured the two claims that were quantitative: CS1591 is now **0**
across all eight projects that reference `TimeWarp.Mediator.Generators` (clean `--no-incremental`
rebuilds; was 376), and the restored `BePublic()` rule is **not vacuous** — a NetArchTest probe against
the built `timewarp-state-plus` assembly shows `Inherit(TimeWarp.Mediator.ActionHandler<>).AreNotAbstract()`
matches 10 handler types and `BeSealed().And().BePublic()` passes on them, while `test-app-client` has 11
`internal sealed` handlers, so its `requirePublicHandlers: false` opt-out is genuinely required rather
than cosmetic. Full solution build: 0 errors, 2 warnings (pre-existing NU1510). `timewarp-state-plus-tests`
11 passed / 1 skipped; `test-app-architecture-tests` 7 passed / 1 skipped.

I checked the specific regression risks flagged for this round and found none: the two
`CreateActionHandlerPolicy` overloads cannot be ambiguous (`params Assembly[]` vs a leading `bool`, and the
`requirePublicHandlers: true` named argument sits in its own position 0, so the non-trailing-named-argument
rule is satisfied); `IsConstructedGenericType` is strictly narrower than the previous `IsGenericType`, and
the generator's per-request `AddScoped<Behavior<TReq,TResp>>` registrations are closed generics with
`ImplementationType` set, so the log output is unchanged for the real pipeline (the paired
`AddScoped<IPipelineBehavior<…>>(factory)` registrations have a null `ImplementationType` and were already
excluded); the moved null checks only reorder logging ahead of an unchanged `break`; the corrected `nameof`
values introduce no duplicate or colliding event-id names and no test asserts on them; and the `NoWarn`
edits append to `$(NoWarn)` rather than replacing it (the `test-app-client` `CS1591` → `$(NoWarn);CS1591`
change fixes a pre-existing clobber of the repo-wide list). The new `<see cref="TimerState.ResetTimersOnActivity"/>`
resolves — a forced recompile of `timewarp-state-plus` emits 0 warnings, i.e. no CS1574.

## Prior findings

| ID | Round-1 status | Round-2 verdict | Note |
|----|----------------|-----------------|------|
| M1 | fixed | verified | `CreateActionHandlerPolicy(bool, params Assembly[])` added; Plus test keeps the default (public+sealed, 10 handlers matched, non-vacuous), test-app opts out with `requirePublicHandlers: false`; both suites pass. |
| M2 | fixed | verified | No `Order = ` remains anywhere; all four sites (`state-plus/assembly-marker.cs:6`, `action-tracking-behavior.cs:7`, `multi-timer-post-processor.cs:5`, `state/assembly-marker.cs:11` prose) now read `order:`, matching the compiling call sites. |
| M3 | fixed | verified | `testing-convention.cs:29-34` now names exactly the five behaviors in `mediator-behaviors.cs` (500–540) and states persistence is intentionally inert; confirmed no `AddBlazored*` call exists outside `test-app-client/program.cs`. |
| M4 | fixed | verified | Documented-alternative disposition honoured: `task.md:69-71` now clears `~/.nuget/packages/timewarp.state{,.plus}` before the build. `TimeWarpStateVersion` remains `12.0.0-beta.3` as dispositioned (version bump deferred to the release task). |
| M5 | fixed | verified | All six sample hosts gained `<NoWarn>$(NoWarn);1591</NoWarn>` and `test-app-client` now appends instead of replacing; clean rebuilds of all eight generator-referencing projects report 0 CS1591 (was 376) and 0 errors. |
| M6 | wontfix | verified (rationale accurate) | `git log -- tests/test-app/test-app-client/generated` → tracked from `3deabecc`, refreshed in `ff036567`; `git merge-base --is-ancestor` confirms both are on `origin/dev`, so the tracking state predates this branch as claimed. |
| M7 | fixed | verified | `GetComponentOrder` filters on `IsConstructedGenericType`; open-generic `IPipelineBehavior<,>` implementations are excluded while the generator's closed `AddScoped<Behavior<TReq,TResp>>` registrations still match, so the logged order is unchanged for the real pipeline. |
| M8 | fixed | verified | Both storage-null checks now precede the trace, and each `JsonSerializer.Serialize(state)` sits inside `if (Logger.IsEnabled(LogLevel.Trace))`; the missing-storage path serializes nothing. |
| M9 | fixed | verified | `state-transaction-behavior.cs:40` names itself; `redux-dev-tools-behavior.cs:41` uses `EventIds.ReduxDevToolsBehavior_Constructing` (530); event ids 600–603 now carry their own `nameof` values with no duplicates. |
| M10 | fixed | verified | `multi-timer-post-processor.cs:5-10` carries an explicit "do not enable this behavior yet" warning naming the recursion and kanban 066; the cref compiles (0 warnings on a forced rebuild). |

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs:21
- Description: The new comment above `GetComponentOrder` gives the wrong rationale for the filter: "Open-generic registrations (e.g. `IPipelineBehavior<,>`) have no concrete type name to report and are skipped." An open-generic *implementation* type such as `MyBehavior<,>` does have a reportable name (`ImplementationType.Name.Split('`')[0]` yields `MyBehavior`), and `IPipelineBehavior<,>` in a legacy `AddScoped(typeof(IPipelineBehavior<,>), typeof(MyBehavior<,>))` call is the *service* type, not the implementation type, so it was never a candidate for this filter in the first place. The actual reason — correctly stated two lines earlier at line 9 — is that the generated mediator never runs open-generic registrations, so listing them would be misleading. A future maintainer reading only the method-level comment could "fix" the filter back to `IsGenericType` on the (false) grounds that a name is in fact available.
- Suggestion: Reword to the real reason, e.g. "Only closed constructed implementation types are considered: the generated mediator registers each woven behavior as a closed generic, and legacy open-generic `IPipelineBehavior<,>` registrations are inert, so listing them would report behaviors that never run."
- Status: open
