---
uid: TimeWarp.State.Summary.md
title: Summary
---

# TimeWarp.State

**TimeWarp.State** (previously known as Blazor-State) is a fully asynchronous state management library for Blazor applications, leveraging the MediatR pipeline to implement the Flux pattern. It handles both Reducers and Effects consistently using async Handlers, simplifying the management of asynchronous operations throughout your app.

By utilizing the MediatR pipeline, TimeWarp.State enables a flexible, middleware-driven architecture for managing state, similar to the request-processing pipeline in ASP.NET. This approach allows developers to inject custom behaviors, such as logging, validation, and caching, directly into the state management flow.

In addition to the core library, we offer **[TimeWarp.State.Plus](/Source/TimeWarp.State.Plus)**, which extends the functionality with enhanced middleware, components, and tools to further streamline state management in complex Blazor applications.
