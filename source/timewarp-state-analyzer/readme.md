# TimeWarp.State Analyzer

Roslyn analyzers packed into `TimeWarp.State` at `analyzers/dotnet/cs`. Consumers get them from the package; apps should not write a scan test for these rules.

## Rules

| Id | Severity | Category | Analyzer |
|----|----------|----------|----------|
| TW0001 | Error | TimeWarp.State | `TimeWarpStateActionAnalyzer` — `IAction` types must be nested in an `IState` |
| TW0002 | Warning | Design | `HandlerMustNotSendActionAnalyzer` — a state action handler must not send an action |
| TW0003 | Info | Design | `HandlerMustNotSendActionAnalyzer` — `[AllowActionSend]` exemption is present |
| TWS001 | Error | Design | `StateImplementationAnalyzer` |
| StateInheritanceTypeArgumentRule | Error | Design | `StateInheritanceAnalyzer` |
| StateSealedClassRule | Warning | Design | `StateInheritanceAnalyzer` |
| StateReadOnlyPublicPropertiesRule | Error | Design | `StateReadOnlyPublicPropertiesAnalyzer` |

## TW0002 — Handler must not send an action

A State is an isolated feature boundary. Handlers do their own work and may **publish a notification**. They never send an action — not on their own state, not on another state. Pages and components sequence actions; notification handlers react.

Default severity is **Warning**. Promote to Error:

```ini
dotnet_diagnostic.TW0002.severity = error
```

TimeWarp.SourceGenerators also emits **TW0002** (`XmlDocsToMarkdownAnalyzer`). An `.editorconfig` that sets `dotnet_diagnostic.TW0002.severity = none` for that rule also silences HandlerMustNotSendAction.

Scope is symbol-based: any type whose base chain includes `TimeWarp.State.StateActionHandler<>`, `TimeWarp.Mediator.ActionHandler<>`, or the legacy `TimeWarp.State.ActionHandler<>`, or that implements `IActionHandler<>`. App-derived bases (`BaseHandler<>`, `DefaultApiHandler<,,>`) are included without name matching. Every method body counts (`Handle`, `HandleSuccess`, `HandleError`, `HandleException`; partials as a group).

**Flagged**

- `ISender.Send` / `IMediator.Send` / `IState.Sender.Send` whose argument implements `IAction`
- ActionSet entry methods on an `IState` (for example `XState.FetchX(...)`), identified by a nested `{MethodName}ActionSet`

**Allowed**

- `IPublisher.Publish` / `IMediator.Publish` / `INotification` arguments
- Direct mutation of the handler's own state
- `NavigationManager`

**Escape hatch:** `[AllowActionSend("reason")]` on the handler type or method. TW0002 is suppressed; **TW0003** (Info) reports the exemption.

## Generator / analyzer ordering

Source generators run before analyzers in the same compilation, so generated ActionSet entry methods are visible to TW0002 when user code calls them. TW0002 uses `GeneratedCodeAnalysisFlags.None`: it does not inspect generated method bodies. The `Sender.Send` inside a generated wrapper is therefore not flagged (the wrapper is a method on the State, not a handler). Pipeline behaviors that send tracking actions are not handlers and stay allowed.

If a consumer disables the ActionSet method generator and hand-writes a wrapper on the State, the call is still flagged when a nested `{MethodName}ActionSet` exists.

See [Analyzers](../../documentation/topics/analyzers.md) for consumer-facing documentation.
