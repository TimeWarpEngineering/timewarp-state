# Notifications

TimeWarp.State has no built-in toast component and no message-bar component. User notifications are application state: a normal `State<T>`, action sets, and one host that paints the list. No third-party toast library is needed. Do not add Blazored.Toast or a similar package.

Follow TimeWarp Architecture's [`NotificationState`](https://github.com/TimeWarpEngineering/timewarp-architecture/tree/master/source/container-apps/web/projects/web-spa/features/notification/notification-state) in [`TimeWarpEngineering/timewarp-architecture`](https://github.com/TimeWarpEngineering/timewarp-architecture), under `source/container-apps/web/projects/web-spa`. The links below were checked against `master` at `0500982b7e5d7632aa3442c42cf29259696b1136`.

This directory is the pointer. It contains no sample project.

## Pattern

One shell region paints every operation outcome. Pages, cards, and feature components report messages. They do not render their own success or error bars.

[`NotificationState`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.cs) is a `State<NotificationState>`. Each message has an id, a Fluent UI `MessageBarIntent` (`Microsoft.FluentUI.AspNetCore.Components`), a title, an optional body, and an optional auto-dismiss time.

| Rule | Behavior |
| --- | --- |
| Visible cap | `MaxVisible` is 3. `VisibleMessages` keeps the newest three, oldest first. `HiddenCount` is the rest. The host offers "+N more". |
| Auto-dismiss | `Success` sets `AutoDismissAt` to `UtcNow` plus `SuccessAutoDismissInterval` (6 seconds). `Info`, `Warning`, and `Error` stay until dismiss or navigation. |
| Duplicates | The key is `(Intent, Title, Body)`. The same message refreshes the existing row (same id, new expiry) instead of stacking. |
| Problem shape | Title is the problem title. Body is the detail. A body that repeats the title is dropped. The title is the operation's sentence, never a generic "Error" or "Success". |

## Actions

Each name is a TimeWarp.State action set on `NotificationState`. Components call the generated methods. A handler does not send an action (`TWS0002` in this repository). A handler publishes a Mediator notification.

| Action | What it does |
| --- | --- |
| [`AddNotification`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.add-notification.cs) | Records a bar for a caller-chosen intent, title, and optional body. |
| [`DismissMessage`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.dismiss-message.cs) | Removes one bar by id. |
| [`ExpireMessages`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.expire-messages.cs) | Removes bars whose `AutoDismissAt` is at or before the clock value the caller passes. The state holds no timer. |
| [`ClearOnNavigation`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.clear-on-navigation.cs) | Drops every bar, including success bars. |
| [`ReportProblem`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.report-problem.cs) | Maps `SharedProblemDetails` to an `Error` bar (`FromProblem`). Operation cancelled (HTTP 499) is ignored. |

[`NavigationListener`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.navigation-listener.cs) is resolved once per circuit from [`Routes.razor`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/components/Routes.razor). On `NavigationManager.LocationChanged` it dispatches `ClearOnNavigation`. That covers `RouteState.ChangeRoute` and anchor navigation. `MessageBars` is recreated with the page, so the listener owns the subscription.

## Mediator handlers

These handlers write `NotificationState` and re-render subscribers. They do not send actions, and they do not call Fluent UI `INotificationService` (that service throws unless a provider is in the render tree; headless tests have none).

| Handler | Notification |
| --- | --- |
| [`OutcomeNotificationHandler`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.outcome-notification-handler.cs) | [`OutcomeNotification`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/outcome-notification.cs): intent, title, optional body. A handler publishes this for a sentence it composes. |
| [`ProblemDetailsNotificationHandler`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.problem-details-notification-handler.cs) | [`ProblemDetailsNotification`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/problem-details-notification.cs). API handlers publish this from their error path. The same problem reported with `ReportProblem` dedupes to one bar. |
| [`ExceptionNotificationHandler`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/notification/notification-state/notification-state.exception-notification-handler.cs) | [`ExceptionNotification`](../../source/timewarp-state/features/pipeline/exception-notification.cs) from this repository. `StateTransactionBehavior` publishes it when a handler throws, after restoring state. The exception message is the bar title. |

A handler reports success by publishing, as in [`credentials-state.add-passkey.cs`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/identity/credentials-state/credentials-state.add-passkey.cs):

```csharp
await Publisher.Publish
(
  new OutcomeNotification(MessageBarIntent.Success, "Passkey created."),
  cancellationToken
);
```

A component dispatches the generated methods:

```csharp
await NotificationState.AddNotification(MessageBarIntent.Info, "Heads up.");
await NotificationState.AddNotification(MessageBarIntent.Success, "Saved successfully.");
await NotificationState.AddNotification(MessageBarIntent.Warning, "Careful.");
await NotificationState.AddNotification(MessageBarIntent.Error, "Something went wrong.");
await NotificationState.ReportProblem(problem);
await NotificationState.DismissMessage(id);
```

[`StyleGuidePage.razor`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/style-guide/pages/StyleGuidePage.razor) wires Info, Success, Warning, Error, and a thrown action to that path. The thrown action publishes `ExceptionNotification`, and the exception handler records the bar.

## Host

[`MessageBars.razor`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/components/MessageBars.razor) is the single host. [`TimeWarpPage.razor`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/components/TimeWarpPage.razor) and [`TimeWarpFocusedPage.razor`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/components/TimeWarpFocusedPage.razor) place it under the header and above the first card.

The host renders one `FluentMessageBar` per visible message. `AllowDismiss` is false, because Fluent UI's built-in dismiss does not update `NotificationState` and a later render would show the bar again. A Dismiss `FluentButton` dispatches `DismissMessage`, so dismissal stays on the keyboard. After each interactive render the host waits until `NextExpiry` and dispatches `ExpireMessages`. The "+N more" toggle is view state on the host (`ShowAll`), not store state.

Spacing is [`MessageBars.razor.css`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/components/MessageBars.razor.css).

## Analyzer TWA0025

[`PageLocalMessageBarAnalyzer`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/analyzers/timewarp-architecture-convention-analyzers/page-local-message-bar-analyzer.cs) (`TWA0025`) warns when a Blazor WebAssembly component renders `FluentMessageBar` with `MessageBarIntent.Error` or `MessageBarIntent.Success` outside `MessageBars`. Report those outcomes through `NotificationState`. Static `Info` and `Warning` guidance may stay in the component. An intent the analyzer cannot resolve to Error or Success is not reported.

Opt out only with [`PageLocalMessageBarAttribute`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/analyzers/timewarp-architecture-attributes/page-local-message-bar-attribute.cs) and a non-empty reason:

```csharp
[PageLocalMessageBar("Style guide showcases the FluentMessageBar component itself; these bars are documentation, not operation outcomes.")]
```

An empty or whitespace reason does not opt out. The Style Guide page is that opt-out in the Architecture SPA ([`StyleGuidePage.razor.cs`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/style-guide/pages/StyleGuidePage.razor.cs)): the Fluent UI card documents the component, and the Notifications card dispatches `AddNotification` so the shell paints the outcome.

Analyzer tests: [`page-local-message-bar-analyzer-tests.cs`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/tests/analyzers/timewarp-architecture-analyzers-tests/page-local-message-bar-analyzer-tests.cs).

## Pipeline notifications

Architecture publishes a notification around each request:

- [`PrePipelineNotification`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/pipeline/notification-pre-processor/pre-pipeline-notification.cs) from [`notification-pre-processor`](https://github.com/TimeWarpEngineering/timewarp-architecture/tree/master/source/container-apps/web/projects/web-spa/pipeline/notification-pre-processor)
- [`PostPipelineNotification`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/pipeline/notification-post-processor/post-pipeline-notification.cs) from [`notification-post-processor`](https://github.com/TimeWarpEngineering/timewarp-architecture/tree/master/source/container-apps/web/projects/web-spa/pipeline/notification-post-processor)

[`IncrementCountNotificationHandler`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/source/container-apps/web/projects/web-spa/features/counter/notification/increment-count-notification-handler.cs) handles `PostPipelineNotification`, filters on `IncrementCounterActionSet.Action`, and logs. It does not add a bar. Copy that shape to observe a finished action without editing its handler. To show a bar, publish `OutcomeNotification` from the handler, or record the message inside a notification handler the way `OutcomeNotificationHandler` does.

## Tests in Architecture

[`notification-state-tests.cs`](https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/tests/container-apps/web/web-spa-integration-tests/features/notification/notification-state-tests.cs) drives the SPA pipeline. The tests check problem title and detail, a dropped repeated body, one bar when the page and the handler report the same problem, a visible cap of three with a hidden count, clear when `RouteState.ChangeRoute` runs, success expiry through `ExpireMessages` with a caller-supplied clock (the test does not sleep), an error that remains, and `DismissMessage`.

## What to copy into an app

Copy `features/notification/notification-state/`, `components/MessageBars.razor` and its CSS, the `NavigationListener` inject on the router, and the three notification handlers. Register the state with the app's TimeWarp.State store. Put `<MessageBars />` in the shell once. Components call `AddNotification`, `ReportProblem`, and `DismissMessage`. Handlers call `Publisher.Publish` with `OutcomeNotification` or `ProblemDetailsNotification`.

TimeWarp.State stays free of Fluent UI and of any toast package. The exception path already starts here: `StateTransactionBehavior` publishes `ExceptionNotification`, and the application handler turns that into a bar.
