---
uid: TimeWarpState:Analyzers.md
title: Analyzers
---

# Analyzers

TimeWarp.State ships Roslyn analyzers inside the `TimeWarp.State` package (`analyzers/dotnet/cs`). Consuming projects get them automatically; there is no separate analyzer package. The persistence source generator in the same package reports **TWSG001** when `[PersistentState]` is applied to a nested class.

> **Breaking change:** analyzer diagnostic IDs were renamed to the `TWS` prefix: `TW0001` → `TWS0001`, `TW0002` → `TWS0002`, `TW0003` → `TWS0003`. This stops colliding with TimeWarp.SourceGenerators, which owns the bare `TW0001`–`TW0006` range (its `TW0002` is the unrelated XML-docs-to-markdown rule). Update `.editorconfig` `dotnet_diagnostic.*.severity` entries to the new ids.

## TWS0001 — Action must be nested in State

- **Severity:** Error
- **Category:** TimeWarp.State
- **Message:** The Action '{0}' is not a nested type of its State

Any non-abstract type that implements `IAction` must be nested in a type that implements `IState`.

## TWS0002 — Handler must not send an action

- **Severity:** Warning (first release; promote to Error in `.editorconfig`)
- **Category:** Design
- **Message:** Action handler '{0}' sends action '{1}'. Handlers must not dispatch actions; publish a notification or sequence the action from the caller.

A State is an isolated feature boundary. Handlers do their own work. If something elsewhere must react, publish a notification. Orchestration lives outside the state (pages and components sequence actions; notification handlers react).

The analyzer flags a type whose base chain includes `StateActionHandler<>` / `TimeWarp.Mediator.ActionHandler<>` (and the legacy `TimeWarp.State.ActionHandler<>`), or that implements `IActionHandler<>`. App bases such as `BaseHandler<>` and `DefaultApiHandler<,,>` are included by symbol, not by name. Every method body in the handler is inspected, including `Handle` and app-defined hooks (`HandleSuccess`, `HandleError`, `HandleException`). Partial classes are walked as one type.

### Flagged

1. `ISender.Send` / `IMediator.Send` / `IState.Sender.Send` whose argument implements `IAction` (including app markers such as `IBaseAction`).
2. Source-generated ActionSet entry methods on a type that implements `IState` (for example `ToastNotificationState.AddProblemDetails(...)`). These are identified by a nested `{MethodName}ActionSet` on the containing state. This is the common dispatch shape in apps that never write a literal `Send`.

### Allowed

- `IPublisher.Publish` / `IMediator.Publish` / an argument that implements `INotification`
- Direct mutation of the handler's own state
- `NavigationManager` (out of scope)

### Promote to Error

```ini
dotnet_diagnostic.TWS0002.severity = error
```

TimeWarp.SourceGenerators ships an unrelated **TW0002** (`XmlDocsToMarkdownAnalyzer`) under the bare `TW` prefix (TW0001–TW0006). This rule used to share that same bare `TW0002` id, so an `.editorconfig` that set `dotnet_diagnostic.TW0002.severity = none` to silence XML-docs-to-markdown also silenced this handler rule. TimeWarp.State's analyzer now uses the `TWS` prefix specifically to avoid that collision; consumers still targeting the old `TW0002` id for this rule should update to `TWS0002`.

### Generator ordering

Roslyn runs source generators before analyzers in the same compilation. Generated ActionSet entry methods are therefore visible to TWS0002 when user code calls them. TWS0002 does not analyze generated method bodies (`GeneratedCodeAnalysisFlags.None`), so the `Sender.Send` inside the generated wrapper is not flagged — that wrapper is not a handler. Pipeline behaviors that send Start/Complete tracking actions are not handlers and stay allowed.

## TWS0003 — AllowActionSend exemption

- **Severity:** Info
- **Category:** Design
- **Message:** Action handler '{0}' is exempt from TWS0002 ({1}). Convert the dispatch to a notification and remove AllowActionSend.

`[AllowActionSend("reason")]` on a handler type or method suppresses TWS0002 so consumers can grandfather patterns such as `HandleError` → toast while converting them to a notification. The reason is required. TWS0003 keeps the debt visible.

```csharp
[AllowActionSend("grandfather HandleError toast until it publishes a notification")]
internal abstract class DefaultApiHandler<TAction, TRequest, TResponse> : ApiHandler<TAction, TRequest, TResponse>
{
}
```

## TWSG001 — Nested PersistentState is not supported

- **Severity:** Error
- **Category:** Persistence
- **Message:** [PersistentState] is not supported on nested class '{0}'. Move the state to a top-level type.

Reported by `PersistenceStateSourceGenerator`. Policies nest **actions** in states, not states in other types. A nested `[PersistentState]` class is skipped (no generated `Load()` partial). Move the state to a top-level type.
