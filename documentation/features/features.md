---
uid: TimeWarp.State:Features.md
title: Features
---

* Oneway Data Flow
* Automatic Subscriptions
* Encapsulated State. Each State exposes its API. Like a micro-site
* Extensible Pipeline
* Auto-clone with ability to override
* Precise control of ReRendering. See [Render control](xref:TimeWarpState:RenderControl.md) and the [sample](../../samples/06-render-control/readme.md).
* Async handling of actions
* RouteState management 
* BaseCacheableState to simplify client side cache with ability to NOT reRender if using cache.
* `[PersistentState]` browser persistence (session or local storage). See [Persistence](xref:TimeWarpState:Persistence.md) and the [sample](../../samples/05-persistence/readme.md).
* A Clean abstraction for Sending of Actions 
* BaseComponent that reads Blazor's `RendererInfo` (name and interactivity) and `IsPreRendering`. The [render control sample](../../samples/06-render-control/readme.md) shows both.

# Roadmap
* Action Based Cloning. To reduce the size of the clone based on the action.
* TimeWarp.State DevTools
  * Event Stream
  * Console and CLI
  * Object Inspector
  * Middleware Viewer
  * Subscriptions Viewer
* IndexedDb/LocalStorage MiddleWare.
* ObjectSpace synchronization with server side Entities.
