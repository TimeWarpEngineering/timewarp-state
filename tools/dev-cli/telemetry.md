# dev CLI telemetry

`dev` calls `UseTelemetry()` with the service name `timewarp-state-dev`. The OTLP endpoint is not set in code. A run exports traces, metrics, and logs only when the endpoint variable below is set. An unset variable exports nothing, so CI without a collector is unaffected.

Every route, including the routes that come from `TimeWarp.Nuru.DevCli` (`clean`, `check-version`, `self-install`), gets one command span on the `TimeWarp.Nuru.Behavior` activity source. The span name is the route pattern. The same source's meter records `nuru.behavior.commands.invoked`, `nuru.behavior.commands.errors`, and the `nuru.behavior.commands.duration` histogram (ms), each tagged with `command`.

The dev CLI has no `ActivitySource` of its own, so `UseTelemetry` registers no extra source. The sources under `source/` belong to the TimeWarp.State libraries, not to `dev`. `dotnet build` and `dotnet test` child processes are not instrumented.

## Environment

Set these on the `dev` process (the developer shell, a GitHub Actions job, or a ganda walk):

| Variable | Required | Meaning |
|----------|----------|---------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | to export | OTLP collector base URL, for example `https://otlp.timewarp.work`. Unset means `dev` exports nothing. |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | no | `grpc` (the SDK default) or `http/protobuf`. Use `http/protobuf` with `https://otlp.timewarp.work`. |
| `OTEL_SERVICE_NAME` | no | Overrides `timewarp-state-dev`. |
| `TRACEPARENT` | no | W3C trace context (`00-{trace-id}-{span-id}-{flags}`) when ganda, or another parent process, is the caller. |
| `TRACESTATE` | no | Optional W3C tracestate that accompanies `TRACEPARENT`. |

`TelemetryBehavior` parents the command span to `Activity.Current`. When `TRACEPARENT` is valid, `dev` restores that context onto `Activity.Current` before `RunAsync` (see `endpoints/incoming-trace-context.cs`), using the caller's trace id and span id, so the command span joins the caller's trace. The restored activity is not itself exported. An invalid `TRACEPARENT` is ignored.

Collector trust: anything that receives the export can see the spans. Span attributes carry the route pattern and command type, not argv, API keys, or exception messages.
