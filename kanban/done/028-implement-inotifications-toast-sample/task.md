# Task 028: Implement INotifications Toast Sample

## Description

**Scope reduced 2026-10-10 (Steven): README only.** The whole deliverable is one README in the
samples directory, `samples/08-notifications/readme.md` (next number in the samples layout). It
points to TimeWarp Architecture's NotificationState implementation as the example to follow and
briefly explains the pattern. No new sample project, no code, and no third-party toast library
(no Blazored.Toast or similar).

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

Verify each path against the current timewarp-architecture master before linking it.

## Requirements

- One README at `samples/08-notifications/readme.md` that:
  - says TimeWarp.State has no built-in toast component and points to TimeWarp Architecture's
    NotificationState implementation as the example to follow, with GitHub links/paths into
    `TimeWarpEngineering/timewarp-architecture`;
  - briefly explains the pattern: `NotificationState` plus the `AddNotification`,
    `DismissMessage`, `ExpireMessages`, `ClearOnNavigation` and `ReportProblem` actions; the
    Mediator notification handlers; the `MessageBars` host component; and analyzer rule `TWA0025`;
  - states that no third-party toast library is needed.
- The PR contains only the README and the kanban task files. No sample project, no code, no tests,
  no other files.

~~Old requirements (superseded 2026-10-10):~~

- ~~A TimeWarp.State sample (or sample feature) that follows the Architecture pattern above:
  notification state + actions, a single host component, Mediator handlers for outcomes/exceptions.~~
- ~~Show success, error, warning and info; auto-dismiss vs sticky; dismissal; clear on navigation.~~
- ~~Accessible (ARIA live region, keyboard dismiss).~~
- ~~README for the sample that points at the Architecture implementation and explains the pattern.~~
- ~~Tests for the state/actions.~~

## Checklist

- [x] Write `samples/08-notifications/readme.md` (paths verified against timewarp-architecture)
- [x] Code review (review/disposition.md: clean)

~~Old checklist (superseded 2026-10-10):~~

- ~~Design: decide sample location and how much of Architecture's code to port~~
- ~~Implement notification state, actions and handlers~~
- ~~Implement host component~~
- ~~Example triggers from several parts of the app~~
- ~~README~~
- ~~Tests~~
- ~~Code review~~

## Notes

- Rewritten 2026-10-10 (Steven): replaced the Blazored.Toast suggestion with a pointer to
  Architecture's NotificationState implementation.
- 2026-10-10: Steven reduced the scope to README-only (one README in `samples/` pointing to
  Architecture's NotificationState). The first walk (log
  `~/logs/task-work-timewarp-state-028-20261010-143412.log`) was stopped during the implement
  oracle, before any code was written, and re-run from implement with this scope.

## Implementation Notes

- 2026-10-10: README only, per the reduced scope. No sample project, no code, no tests.
- Paths checked with `git ls-tree` on timewarp-architecture `origin/master` at
  `0500982b7e5d7632aa3442c42cf29259696b1136` (local master matched).
- `samples/overview.md` and the repo `readme.md` were left unchanged. The brief limits this
  PR to `samples/08-notifications/readme.md` and the kanban task files.
- Code review stays open for the host review node.

## Session

- Implementer: Grok session 01a124c0-171f-7072-a4b3-39810d0c890e (2026-10-10)
- Review oracle: Claude Opus 5.5 (2026-10-10). Effort 2, roster: general.
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-10T07:46:22Z

## Results

`samples/08-notifications/readme.md` is the whole product change. It states that TimeWarp.State
has no built-in toast component, that no third-party toast library is needed, and that apps
should follow TimeWarp Architecture's `NotificationState`.

The README explains `NotificationState` (id, `MessageBarIntent`, title, optional body, optional
auto-dismiss time; max 3 visible; Success auto-dismisses after 6 seconds; Info, Warning, and
Error stay; duplicates refresh), the action sets `AddNotification`, `DismissMessage`,
`ExpireMessages`, `ClearOnNavigation`, and `ReportProblem`, the `NavigationListener`, the
Mediator handlers for `OutcomeNotification`, `ProblemDetailsNotification`, and
`ExceptionNotification`, the `MessageBars` host, pipeline pre/post notifications (Counter
handler), and analyzer `TWA0025` with `[PageLocalMessageBar("reason")]`.

GitHub links point at `TimeWarpEngineering/timewarp-architecture` `master`. Each path was
present on `origin/master` at `0500982b7e5d7632aa3442c42cf29259696b1136`.

**Files**

- `samples/08-notifications/readme.md` (added)
- `kanban/to-do/028-implement-inotifications-toast-sample/task.md` (this kitchen; folderized)

**Decisions**

- No port of Architecture's notification feature into this repo. The reduced brief forbids a
  sample project, code, and tests.
- `samples/overview.md` does not list sample 08, so the samples index will not link here
  until a later change is allowed to edit it.

**Review**

- Rounds: 1. Effort 2 (by-diff, 349 lines). Roster: general.
- Final counts: bug 0, suggestion 0, nit 0. Open 0, fixed 0, wontfix 0.
- Disposition: **clean**. No findings. Every Architecture link resolves at `0500982b`, and the
  behavior the README describes matches the source.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`,
  `review/round-1/merged.md`, `review/disposition.md`.

**Tests:** none in this repo. The README points at Architecture's
`notification-state-tests.cs`. No build is required for a markdown pointer.

### How to validate

**Smoke**

```bash
find samples/08-notifications -type f
rg -n -e 'no built-in toast' -e 'No third-party toast library is needed' \
  -e 'AddNotification' -e 'DismissMessage' -e 'ExpireMessages' \
  -e 'ClearOnNavigation' -e 'ReportProblem' -e 'MessageBars' -e 'TWA0025' \
  -e 'OutcomeNotification' -e 'ExceptionNotification' \
  samples/08-notifications/readme.md
```

**Expect:** `find` prints only `samples/08-notifications/readme.md`. `rg` prints a hit for each pattern.

**Architecture paths** (clone or worktree of `TimeWarpEngineering/timewarp-architecture`, on `master`):

```bash
git rev-parse HEAD
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.add-notification.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.dismiss-message.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.expire-messages.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.clear-on-navigation.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.report-problem.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.navigation-listener.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.outcome-notification-handler.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.problem-details-notification-handler.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.exception-notification-handler.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/outcome-notification.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/notification/notification-state/problem-details-notification.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/components/MessageBars.razor
git cat-file -e master:source/container-apps/web/projects/web-spa/components/MessageBars.razor.css
git cat-file -e master:source/container-apps/web/projects/web-spa/components/TimeWarpPage.razor
git cat-file -e master:source/container-apps/web/projects/web-spa/components/TimeWarpFocusedPage.razor
git cat-file -e master:source/container-apps/web/projects/web-spa/components/Routes.razor
git cat-file -e master:source/container-apps/web/projects/web-spa/pipeline/notification-pre-processor/pre-pipeline-notification.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/pipeline/notification-post-processor/post-pipeline-notification.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/counter/notification/increment-count-notification-handler.cs
git cat-file -e master:source/container-apps/web/projects/web-spa/features/style-guide/pages/StyleGuidePage.razor
git cat-file -e master:source/analyzers/timewarp-architecture-convention-analyzers/page-local-message-bar-analyzer.cs
git cat-file -e master:source/analyzers/timewarp-architecture-attributes/page-local-message-bar-attribute.cs
git cat-file -e master:tests/analyzers/timewarp-architecture-analyzers-tests/page-local-message-bar-analyzer-tests.cs
git cat-file -e master:tests/container-apps/web/web-spa-integration-tests/features/notification/notification-state-tests.cs
```

**Expect:** `git rev-parse` prints `0500982b7e5d7632aa3442c42cf29259696b1136` or a descendant that still contains those paths. Every `git cat-file -e` exits 0.

**Diff scope** (this repo):

```bash
git diff --stat origin/master...HEAD
```

**Expect:** only `samples/08-notifications/readme.md` and kanban files for task 028. No `.cs`, `.csproj`, or test project.

**Automated gate**

```bash
ganda repo audit
```

**Expect:** exit 0.

**Not in scope:** a runnable notification sample, Fluent UI in TimeWarp.State, Blazored.Toast, and the Architecture integration tests (they live in timewarp-architecture).
