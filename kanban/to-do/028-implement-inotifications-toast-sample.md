# Task 028: Implement INotifications Toast Sample

## Description

Create a sample showing how to do user notifications (toasts / message bars) in a TimeWarp.State
Blazor app. Do **not** add a third-party toast library (no Blazored.Toast or similar). Use the
pattern TimeWarp Architecture already ships as the reference implementation.

## Reference: TimeWarp Architecture's NotificationState

See `TimeWarpEngineering/timewarp-architecture`, `source/container-apps/web/projects/web-spa/`:

- `features/notification/notification-state/` — `NotificationState`, a normal `State<T>` holding
  messages (id, `MessageBarIntent`, title, optional body, optional auto-dismiss time). Max 3 visible
  with a hidden count; Success auto-dismisses after 6 s, errors stay; duplicates refresh instead of stacking.
- Actions (standard TimeWarp.State action sets): `AddNotification`, `DismissMessage`,
  `ExpireMessages`, `ClearOnNavigation`, `ReportProblem`; plus a navigation listener.
- Mediator notification handlers that turn app events into messages: outcome notifications,
  `ExceptionNotification`, problem-details notifications.
- `components/MessageBars.razor` — the single shell host; renders Fluent UI `FluentMessageBar`s
  and schedules expiry.
- `pipeline/notification-pre-processor` / `notification-post-processor` — pre/post pipeline
  notifications (see the Counter feature's handler).
- Analyzer rule **TWA0025** (page-local message bar): outcome message bars must go through
  `NotificationState`/`MessageBars`; opt-out only via `[PageLocalMessageBar("reason")]`.
- Tests: `tests/container-apps/web/web-spa-integration-tests/features/notification/notification-state-tests.cs`.

## Requirements

- A TimeWarp.State sample (or sample feature) that follows the Architecture pattern above:
  notification state + actions, a single host component, Mediator handlers for outcomes/exceptions.
- Show success, error, warning and info; auto-dismiss vs sticky; dismissal; clear on navigation.
- Accessible (ARIA live region, keyboard dismiss).
- README for the sample that points at the Architecture implementation and explains the pattern.
- Tests for the state/actions.

## Checklist

- [ ] Design: decide sample location and how much of Architecture's code to port
- [ ] Implement notification state, actions and handlers
- [ ] Implement host component
- [ ] Example triggers from several parts of the app
- [ ] README
- [ ] Tests
- [ ] Code review

## Notes

- Rewritten 2026-10-10 (Steven): replaced the Blazored.Toast suggestion with a pointer to
  Architecture's NotificationState implementation.

## Implementation Notes

- To be added during implementation
