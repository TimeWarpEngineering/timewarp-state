#region Purpose
// Entry point for the telemetry sample: a Blazor Server app that emits TimeWarp.State action spans.
#endregion

#region Design
// Adds AddTimeWarpStateBlazor(), AddTimeWarpStateTelemetry() and OpenTelemetry tracing for the TimeWarp.State ActivitySource and
// ASP.NET Core. The OTLP exporter is added only when OTEL_EXPORTER_OTLP_ENDPOINT is set.
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
    builder.Services.AddTimeWarpStateTelemetry();

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
