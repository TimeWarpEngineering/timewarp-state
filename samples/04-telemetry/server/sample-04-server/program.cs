#region Purpose
// Entry point for the telemetry sample: a Blazor Server app that emits TimeWarp.State action spans.
#endregion

#region Design
// Adds AddTimeWarpStateBlazor(), AddTimeWarpStateTelemetry() and OpenTelemetry tracing for the TimeWarp.State ActivitySource and
// ASP.NET Core. The OTLP exporter is added only when OTEL_EXPORTER_OTLP_ENDPOINT is set.
// AddTimeWarpStateDevToolsExporter copies the same activities to DevTools when
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT is set, and turns snapshots on for that run.
// UseReduxDevTools registers IReduxDevToolsStore. The generated mediator links CommitHandler, and
// Development validates the container. This sample does not render ReduxDevTools, so InitAsync never runs.
#endregion

namespace Sample04Server;

public class Program
{
  public static void Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents();

    builder.Services.AddGeneratedMediator<ClientPipeline>();
    builder.Services.AddTimeWarpState
    (
      options =>
      {
        // CommitHandler is linked by the generated mediator and its constructor needs the
        // redux services. Development validates the container, so those services must exist.
        // This sample does not render <ReduxDevTools />, so InitAsync never runs and actions
        // are not forwarded to the browser extension.
        options.UseReduxDevTools();
      }
    );
    builder.Services.AddTimeWarpStateBlazor();

    string? devToolsEndpoint = Environment.GetEnvironmentVariable
    (
      TimeWarpStateDevToolsExport.EndpointEnvironmentVariable
    );
    builder.Services.AddTimeWarpStateTelemetry
    (
      options =>
      {
        if (string.IsNullOrWhiteSpace(devToolsEndpoint))
        {
          return;
        }

        options.IncludeSnapshots = true;
        options.JsonSerializerOptions = SampleJsonContext.Default.Options;
      }
    );

    builder.Services.AddOpenTelemetry()
      .WithTracing
      (
        tracing =>
        {
          tracing.AddSource(TimeWarpStateTelemetry.ActivitySourceName);
          tracing.AddAspNetCoreInstrumentation();
          if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")))
          {
            tracing.AddOtlpExporter();
          }

          tracing.AddTimeWarpStateDevToolsExporter();
        }
      );

    WebApplication app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
      app.UseExceptionHandler("/Error");
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
      .AddInteractiveServerRenderMode();

    app.Run();
  }
}
