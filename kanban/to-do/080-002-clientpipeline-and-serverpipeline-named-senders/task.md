# ClientPipeline and ServerPipeline named senders

## Description

Parent: **080**. Split the Blazor client store pipeline from server handlers. Marker types, not strings.

## Depends on

- 080-001

## Requirements

- `ISender<ClientPipeline>` (and `IPublisher<ClientPipeline>` if used) in WASM/Blazor client code
- `ISender<ServerPipeline>` on the server / API handlers
- `[MediatorScope(typeof(ClientPipeline))]` / `ServerPipeline` on the corresponding handlers and behaviors
- State pipeline behaviors (transaction, render subscriptions, action tracking, …) are **client-scope**, not on server commands
- No `if (request is IAction)` filtering inside a shared pipeline
- Re-entrant `Send` stays on the same scope

## Checklist

- [ ] Marker types
- [ ] `AddGeneratedMediator<ClientPipeline>()` / `<ServerPipeline>()`
- [ ] Behaviors assigned to the right scope
- [ ] TWM004 (wrong-scope send) does not fire on legitimate client actions

## Out of scope

- Package swap (080-001)
- Full test-app/e2e (080-003)

## Session

- Created: 154892 (2026-09-01)
