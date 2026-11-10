#region Purpose
// Restores a parent process's W3C trace context so dev's command span joins that trace.
#endregion

#region Design
// Ganda and CI pass TRACEPARENT (and optionally TRACESTATE) on the dev process.
// TelemetryBehavior parents its command span to Activity.Current, and Start()
// would give a new local activity its own span id, so the command span would
// not nest under the caller's span. After Start, the ambient activity's span id
// is replaced with the caller's span id via UnsafeAccessor. The activity is not
// exported: its source name is empty, and the generated provider listens only
// to TimeWarp.Nuru and TimeWarp.Nuru.Behavior. An invalid TRACEPARENT is ignored.
// The header value is not logged and is not copied onto a span tag.
#endregion

namespace DevCli;

internal static class IncomingTraceContext
{
  internal static IDisposable? Restore()
  {
    string? traceparent = Environment.GetEnvironmentVariable("TRACEPARENT");
    if (string.IsNullOrEmpty(traceparent))
    {
      return null;
    }

    string? tracestate = Environment.GetEnvironmentVariable("TRACESTATE");
    if (!ActivityContext.TryParse(traceparent, tracestate, isRemote: true, out ActivityContext context))
    {
      return null;
    }

    if (context.TraceId == default || context.SpanId == default)
    {
      return null;
    }

    Activity activity = new("incoming");
    activity.SetIdFormat(ActivityIdFormat.W3C);
    activity.SetParentId(context.TraceId, context.SpanId, context.TraceFlags);
    if (!string.IsNullOrEmpty(context.TraceState))
    {
      activity.TraceStateString = context.TraceState;
    }

    activity.Start();
    SpanIdField(activity) = context.SpanId.ToHexString();
    return activity;
  }

  [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_spanId")]
  private static extern ref string? SpanIdField(Activity activity);
}
