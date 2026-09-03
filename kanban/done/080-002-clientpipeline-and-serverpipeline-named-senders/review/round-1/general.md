# Round 1 — general
**Date:** 2026-09-04
**Scope reviewed:** commit f58795e0 vs origin/feature/080-timewarp-mediator-14-beta (excluding kanban/)

## Summary

Named `ClientPipeline` / `ServerPipeline` markers split the Blazor store from server API handlers: library and hosts inject `ISender<ClientPipeline>` / `IPublisher<ClientPipeline>`, state behaviors declare `Scope = typeof(ClientPipeline)` and close onto `IAction` via generic constraints (no shared-pipeline `if (request is IAction)` filter), and re-entrant Sends stay on ClientPipeline. Risk is low—the change is mostly mechanical scoping plus a documented CS0121 workaround on test-app-server. Re-verified with product greps, `dotnet build` of test-app-server (0 errors), decompile of generated `AddGeneratedMediator_ServerPipeline` (exact match to `AddServerPipelineMediator`), client HTTP-GET of `Query.RouteTemplate` (not pipeline Send of the contracts Query), sample-00-server as Interactive Server client-store host (ClientPipeline correct), and `dotnet fixie timewarp-state-analyzer-tests` (10 passed).

## Issues
