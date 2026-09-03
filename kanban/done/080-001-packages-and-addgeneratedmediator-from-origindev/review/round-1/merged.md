# Round 1 — merged findings
**Date:** 2026-09-03
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 5 | 1 |
| nit | 0 | 4 | 0 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-policies/policies.action-handler-policy.cs:30
- Description: Handler policy dropped `BePublic()`; the Plus library test relies on this policy but library handlers must be public for the host's generated code.
- Suggestion: Keep `BePublic()` for library assemblies via an opt-out parameter.
- Source: general
- Disposition notes: Added `CreateActionHandlerPolicy(bool requirePublicHandlers, params Assembly[])`; default overload requires public sealed; test-app-architecture-tests opts out.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state-plus/features/action-tracking/pipeline/action-tracking-behavior.cs:7 (also multi-timer-post-processor.cs:5, timewarp-state-plus/assembly-marker.cs:6)
- Description: Docs advertise `Order = ...` on `MediatorBehaviorAttribute`, which is get-only (CS0617).
- Suggestion: Use positional `order:`.
- Source: general
- Disposition notes: All three docs plus the prose in timewarp-state/assembly-marker.cs now say `order:`.

### M3 — Severity: suggestion — Status: fixed
- File: tests/client-integration-tests/infrastructure/testing-convention.cs:29
- Description: Comment claims only ActiveActionBehavior is woven; five behaviors are, and PersistentStatePostProcessor runs inert (no Blazored storage).
- Suggestion: Correct the comment; register storage or state persistence is intentionally inert.
- Source: general
- Disposition notes: Comment lists the five behaviors and states persistence is intentionally inert in this host. Registering Blazored storage is out of scope (needs JS interop; 080-003 owns test coverage).

### M4 — Severity: suggestion — Status: fixed
- File: Directory.Build.props:5
- Description: `TimeWarpStateVersion` unchanged at 12.0.0-beta.3 despite incompatible assembly changes; a stale `~/.nuget/packages/timewarp.state/12.0.0-beta.3` silently breaks sample builds.
- Suggestion: Bump prerelease or document the cache-clear step.
- Source: general
- Disposition notes: Documented alternative taken: the task's "How to validate" now clears the cached timewarp.state / timewarp.state.plus packages before build. The major version bump stays deferred to the release task (already listed under Known gaps); version policy is the operator's call.

### M5 — Severity: suggestion — Status: fixed
- File: Directory.Build.props:29
- Description: 376 new CS1591 warnings from TimeWarp.Mediator generated code in hosts.
- Suggestion: Add 1591 to NoWarn on generator hosts.
- Source: general
- Disposition notes: `$(NoWarn);1591` added to every sample host that references TimeWarp.Mediator.Generators; test-app-client's NoWarn now appends to the repo list instead of replacing it.

### M6 — Severity: suggestion — Status: wontfix
- File: .gitignore:315
- Description: `tests/test-app/test-app-client/generated/**` is ignored but 99 files are tracked; this commit had to commit path-churned build output.
- Suggestion: `git rm --cached -r` the folder or make emitted content path-independent.
- Source: general
- Disposition notes: wontfix on this task (decided by review oracle). The tracking state predates this branch (tracked since commit 3deabecc and refreshed in ff036567 on origin/dev); untracking 99 files inside the mediator-swap PR would bury the real diff. Recorded as a follow-up for 080-003 (tests/docs sweep) with the reviewer's two options.

### M7 — Severity: nit — Status: fixed
- File: source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs:22
- Description: `GetComponentOrder` also matches legacy open-generic `IPipelineBehavior<,>` registrations that the generated mediator never runs, so the log can list inert behaviors.
- Suggestion: Restrict to closed constructed types.
- Source: general
- Disposition notes: Matches `IsConstructedGenericType` only; comment explains open-generic registrations are ignored.

### M8 — Severity: nit — Status: fixed
- File: source/timewarp-state-plus/features/persistence/pipeline/persistent-state-post-processor.cs:63
- Description: `JsonSerializer.Serialize(state)` evaluated eagerly for LogTrace, before the null-storage skip.
- Suggestion: Null check first; guard with `IsEnabled(Trace)`.
- Source: general
- Disposition notes: Null-storage check moved above the trace; trace guarded by `Logger.IsEnabled(LogLevel.Trace)`.

### M9 — Severity: nit — Status: fixed
- File: source/timewarp-state/features/pipeline/state-transaction-behavior.cs:40
- Description: StateTransactionBehavior logs ReduxDevToolsBehavior's name; ReduxDevToolsBehavior logs under StateTransactionBehavior_Constructing.
- Suggestion: Use own type name / own event id.
- Source: general
- Disposition notes: Fixed both; also corrected the four RenderSubscriptionsPostProcessor event-id names in event-ids.cs that carried the same copy-paste `nameof`.

### M10 — Severity: nit — Status: fixed
- File: source/timewarp-state-plus/features/timers/multi-timer-post-processor.cs:5
- Description: Doc invites opting in to a behavior with a known unbounded-recursion defect (task 066).
- Suggestion: Add the guard or an explicit warning.
- Source: general
- Disposition notes: Explicit "do not enable, see 066" warning added to the doc comment. The guard and internal-action marker stay with 066 (it owns the tests).

## Duplicates / conflicts

- None (single reviewer).
