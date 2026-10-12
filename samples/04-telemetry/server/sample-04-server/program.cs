#region Purpose
// Entry point for the telemetry sample: a Blazor Server app that emits TimeWarp.State action spans.
#endregion

#region Design
// Adds AddTimeWarpStateBlazor(), AddTimeWarpStateTelemetry() and OpenTelemetry tracing for the TimeWarp.State ActivitySource and
// ASP.NET Core. The OTLP exporter is added only when OTEL_EXPORTER_OTLP_ENDPOINT is set.
// AddTimeWarpStateDevToolsExporter copies the same activities to DevTools when
// TIMEWARP_STATE_DEVTOOLS_ENDPOINT is set, and turns snapshots on for that run.
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
    builder.Services.AddTimeWarpState();
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
