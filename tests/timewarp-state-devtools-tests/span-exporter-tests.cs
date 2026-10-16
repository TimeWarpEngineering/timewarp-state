#region Purpose
// Proves the exporter copies inspected-app activities and drops the inspector's own.
#endregion

#region Design
// BuildItem and ExportActivities are the channel before HTTP ingest. A down inspector returns
// Failure and does not throw, so the inspected app keeps running. The endpoint variable gates
// registration: unset is a no-op, a non-URI throws.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class DevToolsSpanExporter_Should
{
  public static void ExportATimeWarpStateActivity()
  {
    using Activity activity = StartActivity("CounterState", "IncrementCount");
    DevToolsSpanExporter.ShouldExport(activity).ShouldBeTrue();
    SpanIngestItem? item = DevToolsSpanExporter.BuildItem(activity);
    item.ShouldNotBeNull();
    item.Attributes[InspectorCapture.StateTypeTagName].ShouldBe("CounterState");
    item.Attributes[InspectorCapture.ActionTagName].ShouldBe("IncrementCount");
    item.Attributes[InspectorCapture.RuntimeTagName].ShouldBe(InspectorCapture.ServerRuntime);
  }

  public static void KeepAnExistingRuntimeTag()
  {
    using Activity activity = StartActivity("CounterState", "IncrementCount", runtime: "custom");
    SpanIngestItem? item = DevToolsSpanExporter.BuildItem(activity);
    item.ShouldNotBeNull();
    item.Attributes[InspectorCapture.RuntimeTagName].ShouldBe("custom");
  }

  public static void CopyStoreAndRenderTagsOntoTheIngestItem()
  {
    const string storeId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    using Activity activity = StartActivity
    (
      "CounterState",
      "IncrementCount",
      storeId: storeId,
      render: InspectorCapture.InteractiveRender
    );
    SpanIngestItem? item = DevToolsSpanExporter.BuildItem(activity);
    item.ShouldNotBeNull();
    item.Attributes[InspectorCapture.StoreTagName].ShouldBe(storeId);
    item.Attributes[InspectorCapture.RenderTagName].ShouldBe(InspectorCapture.InteractiveRender);
  }

  public static void DropInspectorStateActivities()
  {
    using Activity inspectorState = StartActivity(InspectorCapture.InspectorStateTypeName, "SetFilter");
    DevToolsSpanExporter.ShouldExport(inspectorState).ShouldBeFalse();
    DevToolsSpanExporter.BuildItem(inspectorState).ShouldBeNull();

    using Activity tagged = StartActivity("CounterState", "IncrementCount", inspector: true);
    DevToolsSpanExporter.ShouldExport(tagged).ShouldBeFalse();
    DevToolsSpanExporter.BuildItem(tagged).ShouldBeNull();
  }

  public static void IgnoreOtherActivitySources()
  {
    using ActivityListener listener = Listen("Other.Source");
    using ActivitySource source = new("Other.Source");
    using Activity? activity = source.StartActivity("Other");
    activity.ShouldNotBeNull();
    DevToolsSpanExporter.ShouldExport(activity).ShouldBeFalse();
  }

  public static void PostOnlyCapturedSpans()
  {
    using HttpListener listener = new();
    int port = FreePort();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();

    using Activity app = StartActivity("CounterState", "IncrementCount");
    using Activity inspector = StartActivity(InspectorCapture.InspectorStateTypeName, "SetFilter");
    DevToolsSpanExporter exporter = new(new Uri($"http://127.0.0.1:{port}/ingest/spans"));
    string body = "";
    Task serve = Task.Run(() =>
    {
      HttpListenerContext context = listener.GetContext();
      using StreamReader reader = new(context.Request.InputStream);
      body = reader.ReadToEnd();
      context.Response.StatusCode = 200;
      context.Response.Close();
    });
    try
    {
      ExportResult result = exporter.ExportActivities([app, inspector]);
      result.ShouldBe(ExportResult.Success);
      serve.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
      body.ShouldContain("CounterState");
      body.ShouldNotContain(InspectorCapture.InspectorStateTypeName);
    }
    finally
    {
      exporter.Dispose();
      listener.Stop();
    }
  }

  public static void ReturnFailureWhenTheInspectorIsDown()
  {
    using Activity app = StartActivity("CounterState", "IncrementCount");
    using DevToolsSpanExporter exporter = new(new Uri($"http://127.0.0.1:{FreePort()}/ingest/spans"));
    ExportResult result = exporter.ExportActivities([app]);
    result.ShouldBe(ExportResult.Failure);
  }

  public static void SkipRegistrationWhenTheEndpointIsUnset()
  {
    WithEndpoint(null, () =>
    {
      using TracerProvider? provider = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
        .AddTimeWarpStateDevToolsExporter()
        .Build();
      provider.ShouldNotBeNull();
    });
  }

  public static void RejectANonAbsoluteEndpoint()
  {
    WithEndpoint("not a uri", () =>
    {
      Should.Throw<InvalidOperationException>
      (
        () => OpenTelemetry.Sdk.CreateTracerProviderBuilder().AddTimeWarpStateDevToolsExporter()
      );
    });
  }

  private static void WithEndpoint(string? value, Action action)
  {
    string? previous = Environment.GetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable);
    try
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable, value);
      action();
    }
    finally
    {
      Environment.SetEnvironmentVariable(TimeWarpStateDevToolsExport.EndpointEnvironmentVariable, previous);
    }
  }

  private static Activity StartActivity
  (
    string stateTypeName,
    string actionName,
    bool inspector = false,
    string? storeId = null,
    string? render = null,
    string? runtime = null
  )
  {
    ActivityListener listener = Listen(InspectorCapture.ActivitySourceName);
    ActivitySource source = new(InspectorCapture.ActivitySourceName);
    Activity? activity = source.StartActivity("App.Action");
    activity.ShouldNotBeNull();
    activity.SetTag(InspectorCapture.StateTypeTagName, stateTypeName);
    activity.SetTag(InspectorCapture.ActionTagName, actionName);
    if (inspector)
    {
      activity.SetTag(InspectorCapture.InspectorTagName, "true");
    }

    if (storeId is not null)
    {
      activity.SetTag(InspectorCapture.StoreTagName, storeId);
    }

    if (render is not null)
    {
      activity.SetTag(InspectorCapture.RenderTagName, render);
    }

    if (runtime is not null)
    {
      activity.SetTag(InspectorCapture.RuntimeTagName, runtime);
    }

    activity.Stop();
    // The listener and source must outlive the activity's tags. The caller disposes the activity,
    // which does not dispose these. Keep them rooted on the activity via a custom property bag
    // by disposing them when the activity is disposed through a wrapper. Callers use `using Activity`,
    // so dispose the listener and source when the activity is stopped: they are only needed to create it.
    listener.Dispose();
    source.Dispose();
    return activity;
  }

  private static ActivityListener Listen(string sourceName)
  {
    ActivityListener listener = new()
    {
      ShouldListenTo = activitySource => activitySource.Name == sourceName,
      Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };
    ActivitySource.AddActivityListener(listener);
    return listener;
  }

  private static int FreePort()
  {
    using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
    return ((IPEndPoint)socket.LocalEndPoint!).Port;
  }
}
