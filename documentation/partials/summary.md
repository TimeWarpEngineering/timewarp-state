---
uid: TimeWarp.State.Summary.md
title: Summary
---

# TimeWarp.State

**TimeWarp.State** (previously known as Blazor-State) 
is a fully asynchronous state management library for Blazor applications,
leveraging the TimeWarp.Mediator 15.0.0-beta.5 generated pipeline (`AddGeneratedMediator<ClientPipeline>()` / named pipelines) to implement the Flux pattern. 
It handles both Reducers and Effects consistently using async Handlers,
simplifying the management of asynchronous operations throughout your app.

For a plain state change you do not write the handler at all.
Mark the action with `[Reducer]` and declare one `static void Reduce(TState state, Action action)`,
and the source generator emits the handler, the dispatch method and the catalog and manifest entries.
Hand-write the handler when the action does async work or needs injected services.
See [Actions](xref:TimeWarpState:Actions.md).

By utilizing that TimeWarp.Mediator pipeline, TimeWarp.State enables a flexible, 
middleware-driven architecture for managing state, 
similar to the request-processing pipeline in ASP.NET. 
This approach allows developers to inject custom behaviors, such as logging,
validation, and caching, directly into the state management flow.

In addition to the core library, we offer **[TimeWarp.State.Plus](../../source/timewarp-state-plus/readme.md)**, 
which extends the functionality with enhanced middleware, components, 
and tools to further streamline state management in complex Blazor applications.
