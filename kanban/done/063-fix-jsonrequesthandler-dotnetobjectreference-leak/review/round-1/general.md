# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** branch task/063-fix-jsonrequesthandler-dotnetobjectreference-leak vs origin/master (json-request-handler.cs, json-request-handler-tests.cs, global-usings.cs) plus surrounding call sites TimeWarpJavaScriptInterop.razor and DI registration.

## Summary

`JsonRequestHandler.InitAsync` is now idempotent: it stores one `DotNetObjectReference<JsonRequestHandler>`, guards with `IsInitialized`/`IsDisposed`, and releases the root in `Dispose`/`DisposeAsync` while swallowing `JSDisconnectedException`. `TimeWarpJavaScriptInterop` still gates on `firstRender`, and scoped DI (`TryAddScoped<JsonRequestHandler>`) remains the dispose owner for the circuit. Risk is low; the change matches the 2026-06-11 finding-10 remainder and is covered by five focused tests, including N `InitAsync` calls → one `InitializeJavaScriptInterop` invoke.

## Issues
