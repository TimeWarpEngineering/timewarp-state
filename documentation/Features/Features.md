---
uid: TimeWarp.State:Features.md
title: Features
---

* Oneway Data Flow
* Automatic Subscriptions
* Encapsulated State. Each State exposes its API. Like a micro-site
* Extensible Pipeline
* Auto-clone with ability to override
* Precise control of ReRendering
* Async handling of actions
* RouteState management 
* BaseCacheableState to simplify client side cache with ability to NOT reRender if using cache.
* A Clean abstraction for Sending of Actions 
* BaseComponent that provides handling of Blazors RenderModes.

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
