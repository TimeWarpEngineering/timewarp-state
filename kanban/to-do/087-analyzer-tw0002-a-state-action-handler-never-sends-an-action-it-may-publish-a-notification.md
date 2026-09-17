# Analyzer TW0002: a state action handler never sends an action; it may publish a notification

## Description

Architecture decision (Steve, 2026-09-17): a State is an isolated feature boundary. Its action
handlers do their own work and, if something elsewhere must react, **publish a notification**
that says what happened. A handler never sends an action — not on its own state, not on another
state. Orchestration of multi-step flows lives outside the state (pages/components sequence
actions; notification handlers react). Calling into another state from a handler breaks
encapsulation.

Evidence from the cross-repo review (2026-09-17): the library itself already obeys this — no
`StateActionHandler` in timewarp-state or timewarp-state-plus sends an action; every Send comes
from pipeline behaviors, notification handlers, or components. Apps drifted: timewarp-architecture
had three same-state `HandleSuccess → Fetch` dispatches that deadlocked under its app-level
per-state semaphore (fixed in architecture task 236, which also added a source-scan guard);
COPIC and architecture send a toast action from the API handler base's `HandleError`
(cross-state); Trinsic (BlazorState era) fans out from `PollingHandler`. Notifications are
plumbed everywhere and used by no handler today.

Make the rule declarative: a Roslyn analyzer in `source/timewarp-state-analyzer`, so consumers
get it from the package instead of each app writing a scan test.

## Requirements

- New diagnostic **TW0002** `HandlerMustNotSendAction` (category Design; **Warning** by default
  in the first release, promotable to Error via `.editorconfig`; document the promotion).
  Message: "Action handler '{0}' sends action '{1}'. Handlers must not dispatch actions; publish
  a notification or sequence the action from the caller."
- Scope: any type whose base-type chain includes the library handler bases
  (`StateActionHandler<>` / `BaseHandler<>` and derived app bases resolved by symbol, not name).
  Inspect every method body in the handler (Handle and app-defined hooks such as HandleSuccess /
  HandleError / HandleException; partial classes walked as a group).
- Flag:
  1. invocations of `ISender.Send` / `IMediator.Send` / `IState.Sender.Send` whose argument type
     implements `IBaseAction` (or the library's action marker); and
  2. invocations of source-generated action-set entry methods on any type implementing `IState`
     (e.g. `XState.FetchX(...)`) — architecture has **zero** literal `Send` calls; all dispatch
     goes through these generated statics, so this branch is the one that matters. Resolve the
     invoked method's containing type against `IState` via the semantic model.
- Allow unconditionally: `IPublisher.Publish` / `IMediator.Publish` / anything whose argument
  implements `INotification`; direct mutation of the handler's own state; `NavigationManager`
  calls (out of scope here).
- Escape hatch: `[AllowActionSend("reason")]` on the handler type (or method), so consumers can
  grandfather the `HandleError → Toast` pattern while they convert it to a notification. Emit
  an Info diagnostic TW0003 when the attribute is present so the debt stays visible.
- Tests in `tests/timewarp-state-analyzer-tests` following the TW0001 pattern: same-state
  generated entry → warning; cross-state generated entry → warning; `Sender.Send(IBaseAction)`
  → warning; `Publish(INotification)` → clean; own-state mutation → clean; attribute → TW0003
  only; partial-class split across files → warning.
- Verify analyzer ordering with the source generator (generated entry methods must be visible to
  the analyzer); document any caveat.
- Docs: analyzer README / `documentation` entry for TW0002 and TW0003, and a one-paragraph
  "State is a boundary" rule in the library overview: handlers do their own work, publish
  notifications, never send actions; orchestration is outside the state.
- Follow-ups to file when this lands (not in this task): architecture — convert
  `DefaultApiHandler.HandleError → ToastNotificationState` to a notification and retire the 236
  scan guard in favour of TW0002; COPIC — same conversion.

## Checklist

- [ ] TW0002 analyzer (symbol-based) + TW0003 escape-hatch info diagnostic
- [ ] Tests per the matrix above
- [ ] Generator/analyzer ordering verified and documented
- [ ] README/docs + library overview rule
- [ ] `dev build` 0/0; analyzer tests green; `ganda repo audit` clean
- [ ] Results and How to validate (run the analyzer against timewarp-architecture master and list the hits)

## Session

- Created: cockpit (2026-09-17)
- Claude Code cockpit session: https://claude.ai/code/session_01KPZXyAmA6Vk99W1yUQUn1N

## Notes

- Reference implementation: `source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs`
  (TW0001: syntax-triggered, semantic-verified; `tests/timewarp-state-analyzer-tests/`).
- Policies package (`timewarp-state-policies`, NetArchTest) checks handler nesting/visibility
  only; not the right home for an invocation rule.
- Library precedent: `IState.Sender` is documented for "re-entrant actions" but unused; the
  action-tracking pipeline behavior sends Start/Complete actions — that is a behavior, not a
  handler, and stays allowed.
- Related: architecture 236 (deadlock fix + scan guard), timewarp-state 086 (breadcrumb, open).

## Results

_Pending._

### How to validate

_Pending._
