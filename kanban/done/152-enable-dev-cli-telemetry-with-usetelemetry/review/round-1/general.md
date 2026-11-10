# Round 1 — general
**Date:** 2026-11-10
**Scope reviewed:** `tools/dev-cli/` on `task/152-enable-dev-cli-telemetry-with-usetelemetry` vs `master` (`dev.cs`, `endpoints/incoming-trace-context.cs`, `global-usings.cs`, `telemetry.md`), checked against the task and `## Goal (implementer restatement)`

## Summary

`dev.cs` calls `.UseTelemetry(options => options.ServiceName = "timewarp-state-dev")` on the builder and restores the caller trace with `IncomingTraceContext.Restore()` before `app.RunAsync(args)`. `incoming-trace-context.cs` matches timewarp-nuru `tools/dev-cli/endpoints/incoming-trace-context.cs`. The expression lambda is the form Nuru's telemetry extractor reads, so the service name is a compile-time string literal. The dev CLI has no `ActivitySource` of its own and does not reference the TimeWarp.State library sources, so skipping `ActivitySources.Add` matches the task's conditional and the restatement. That restatement does not misread the task.

`./bin/dev --capabilities` lists the local routes and the DevCli routes `clean`, `check-version`, `release`, and `self-install`. The published AOT binary contains the service name, `TRACEPARENT` / `TRACESTATE`, `IncomingTraceContext`, `TimeWarp.Nuru.Behavior`, and the `nuru.behavior.commands.*` metric names. `telemetry.md` names the endpoint, protocol, service-name override, and trace-context variables, and it drops Nuru's workflow stage spans, which this repo does not emit.

No findings.
