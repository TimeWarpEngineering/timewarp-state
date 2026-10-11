# Task 110: State Plus a-la-carte modules: pluggable action sets + predefined state for any site

## Description

Design TimeWarp.State.Plus as a menu of a-la-carte modules. Each module is a set of actions plus a
predefined state that controls functionality but NOT the UX controls, so it can be plugged into any
site. This fits the TimeWarp template pitch: pick the modules you want.

Filed 2026-10-11 from Steven's voice call. Backlog, design only. Not launched.

## What Plus ships today

- `RouteState` (+ breadcrumb / page title). Used heavily by timewarp-architecture.
- `ThemeState`. Wired in Architecture but a no-op (button always sends Dark; nothing applies it).
- `ActionTrackingState` / `[TrackAction]` (busy indicators). Used.
- Persistence. Registered in Architecture but unused (no state opts in).
- `TimerState`. Unused.
- `TimeWarpCacheableState` base class. Used.

The action catalog (which Ctrl-K and agent tools build on) lives in core TimeWarp.State, not Plus.

## Candidate modules (from timewarp-architecture analysis, 2026-10-11)

1. **NotificationState**: already UI-agnostic; the only tie is problem-details mapping. Best first.
2. **Command palette core** (state, ranker, runner) + `ICommandSource` / `ICommandAuthorizer`,
   with a generic `ModalState` extracted from Architecture's `ApplicationState`.
3. **Feedback** with a host-implemented `IFeedbackSink` (submit / list / get / upload / remove).

Also:

- Finish `ThemeState`: persist the choice, toggle action, apply hook.
- Analytics `[TrackEvent]` with an `IAnalyticsSink` (maybe belongs in State.Telemetry).
- Later: an agent add-on package (catalog -> AIFunction binding + one-pending-approval gate).

Not candidates (host API caches / identity flows): auth/passkeys, profile/settings, admin
(principals/roles), agent links, chat, authorization.

## Open question: backend contract coupling

Module action implementations call a backend, so each carries a contract. Are contracts shared
across modules, or tied to the host app's API? Decide the seam: module-owned models +
host-implemented interfaces/mappers, and which shared types (if any) go in a small common package.

### Coupling findings (2026-10-11 analysis)

- **State Plus modules carry no backend contracts** today (route, theme, action tracking, timers,
  persistence are all client-side).
- **In Architecture web-spa, coupling comes via:**
  - `DefaultApiHandler<Action, Command, Response>`: one endpoint per handler, through `IWebServerApiService`.
  - States holding Response DTOs: Feedback, Credentials, Principals, Roles, AgentLinks.
  - Compile-time `PermissionIds` in `[CatalogAction]`.
  - Chat actions wrapping `SendMessage.Command`.
- **Shared across modules:**
  - `SharedProblemDetails`, `FileResponse`, `IApiService`, `IApiRequest` (Foundation contracts).
  - `ProblemDetailsNotification` / `OutcomeNotification` (web-spa), published by
    `DefaultApiHandler`, so every API module depends on Notification.
  - `AgentCallOutcome`: Feedback depends on the agent layer.
  - `RoleIds`: shared by principals and authorization.
- **Feedback and Notification share no feedback contract types**; they are linked only via problem details.
- **Dependency direction** is one-way: web-spa -> web-contracts -> foundation contracts / TimeWarp.Identity.

### Proposed seams

- Plus-owned models + host-implemented interfaces (`IFeedbackService`, `ICommandSource` /
  `ICommandAuthorizer`, `IAnalyticsSink`).
- Host-supplied permission map instead of compile-time `PermissionIds`.
- A small abstractions package for: problem shape, problem/outcome notifications, an
  `AgentCallOutcome`-like caller result, and permission requirement.

## Requirements

- Decide the module list and order (proposal above: Notification, palette core + ModalState, Feedback, Theme finish).
- Decide the contract seam per module and the contents (if any) of a common abstractions package.
- Each module controls functionality only; no UX controls ship in the module.

## Out of scope

- No implementation now; this is design / backlog.

## Checklist

- [ ] Confirm module list and order with Steven
- [ ] Decide contract seam and common abstractions package
- [ ] Split follow-up implementation tasks per module

## Notes

- Source analysis: timewarp-architecture master `2fd8d3f1c` (2026-10-11), web-spa features.

## Results

### How to validate
