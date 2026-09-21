# TimeWarp.State Analyzer

Roslyn analyzers packed into `TimeWarp.State` at `analyzers/dotnet/cs`. Consumers get them from the package; apps should not write a scan test for these rules.

> **Breaking change:** diagnostic IDs were renamed to the `TWS` prefix: `TW0001` → `TWS0001`, `TW0002` → `TWS0002`, `TW0003` → `TWS0003`, to stop colliding with TimeWarp.SourceGenerators (which owns the bare `TW0001`–`TW0006` range). Update `.editorconfig` severities accordingly.

## Rules

| Id | Severity | Category | Analyzer |
|----|----------|----------|----------|
| TWS0001 | Error | TimeWarp.State | `TimeWarpStateActionAnalyzer` — `IAction` types must be nested in an `IState` |
| TWS0002 | Warning | Design | `HandlerMustNotSendActionAnalyzer` — a state action handler must not send an action |
| TWS0003 | Info | Design | `HandlerMustNotSendActionAnalyzer` — `[AllowActionSend]` exemption is present |
| TWS001 | Error | Design | `StateImplementationAnalyzer` |
| TWSG001 | Error | Persistence | `PersistenceStateSourceGenerator` — `[PersistentState]` is not supported on nested classes |
| StateInheritanceTypeArgumentRule | Error | Design | `StateInheritanceAnalyzer` |
| StateSealedClassRule | Warning | Design | `StateInheritanceAnalyzer` |
| StateReadOnlyPublicPropertiesRule | Error | Design | `StateReadOnlyPublicPropertiesAnalyzer` |

## TWS0002 — Handler must not send an action

A State is an isolated feature boundary. Handlers do their own work and may **publish a notification**. They never send an action — not on their own state, not on another state. Pages and components sequence actions; notification handlers react.

Default severity is **Warning**. Promote to Error:

```ini
dotnet_diagnostic.TWS0002.severity = error
```

TimeWarp.SourceGenerators ships an unrelated **TW0002** (`XmlDocsToMarkdownAnalyzer`) under the bare `TW` prefix. This rule used to share that same bare `TW0002` id, so an `.editorconfig` that set `dotnet_diagnostic.TW0002.severity = none` for that rule also silenced HandlerMustNotSendAction; the `TWS` prefix removes that collision.

Scope is symbol-based: any type whose base chain includes `TimeWarp.State.StateActionHandler<>`, `TimeWarp.Mediator.ActionHandler<>`, or the legacy `TimeWarp.State.ActionHandler<>`, or that implements `IActionHandler<>`. App-derived bases (`BaseHandler<>`, `DefaultApiHandler<,,>`) are included without name matching. Every method body counts (`Handle`, `HandleSuccess`, `HandleError`, `HandleException`; partials as a group).

**Flagged**

- `ISender.Send` / `IMediator.Send` / `IState.Sender.Send` whose argument implements `IAction`
- ActionSet entry methods on an `IState` (for example `XState.FetchX(...)`), identified by a nested `{MethodName}ActionSet`

**Allowed**

- `IPublisher.Publish` / `IMediator.Publish` / `INotification` arguments
- Direct mutation of the handler's own state
- `NavigationManager`

**Escape hatch:** `[AllowActionSend("reason")]` on the handler type or method. TWS0002 is suppressed; **TWS0003** (Info) reports the exemption.

## Generator / analyzer ordering

Source generators run before analyzers in the same compilation, so generated ActionSet entry methods are visible to TWS0002 when user code calls them. TWS0002 uses `GeneratedCodeAnalysisFlags.None`: it does not inspect generated method bodies. The `Sender.Send` inside a generated wrapper is therefore not flagged (the wrapper is a method on the State, not a handler). Pipeline behaviors that send tracking actions are not handlers and stay allowed.

If a consumer disables the ActionSet method generator and hand-writes a wrapper on the State, the call is still flagged when a nested `{MethodName}ActionSet` exists.

See [Analyzers](../../documentation/topics/analyzers.md) for consumer-facing documentation.
