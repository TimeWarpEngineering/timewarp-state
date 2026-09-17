---
uid: TimeWarpState:Analyzers.md
title: Analyzers
---

# Analyzers

TimeWarp.State ships Roslyn analyzers inside the `TimeWarp.State` package (`analyzers/dotnet/cs`). Consuming projects get them automatically; there is no separate analyzer package.

## TW0001 — Action must be nested in State

- **Severity:** Error
- **Category:** TimeWarp.State
- **Message:** The Action '{0}' is not a nested type of its State

Any non-abstract type that implements `IAction` must be nested in a type that implements `IState`.

## TW0002 — Handler must not send an action

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
dotnet_diagnostic.TW0002.severity = error
```

TimeWarp.SourceGenerators also ships a **TW0002** (`XmlDocsToMarkdownAnalyzer`). Diagnostic IDs are compilation-global: an `.editorconfig` that sets `dotnet_diagnostic.TW0002.severity = none` to silence XML-docs-to-markdown also silences this handler rule. Re-enable or scope that suppression when consuming TimeWarp.State 12.0.0-beta.3+.

### Generator ordering

Roslyn runs source generators before analyzers in the same compilation. Generated ActionSet entry methods are therefore visible to TW0002 when user code calls them. TW0002 does not analyze generated method bodies (`GeneratedCodeAnalysisFlags.None`), so the `Sender.Send` inside the generated wrapper is not flagged — that wrapper is not a handler. Pipeline behaviors that send Start/Complete tracking actions are not handlers and stay allowed.

## TW0003 — AllowActionSend exemption

- **Severity:** Info
- **Category:** Design
- **Message:** Action handler '{0}' is exempt from TW0002 ({1}). Convert the dispatch to a notification and remove AllowActionSend.

`[AllowActionSend("reason")]` on a handler type or method suppresses TW0002 so consumers can grandfather patterns such as `HandleError` → toast while converting them to a notification. The reason is required. TW0003 keeps the debt visible.

```csharp
[AllowActionSend("grandfather HandleError toast until it publishes a notification")]
internal abstract class DefaultApiHandler<TAction, TRequest, TResponse> : ApiHandler<TAction, TRequest, TResponse>
{
}
```
