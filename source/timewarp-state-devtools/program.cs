#region Purpose
// Hosts the DevTools Blazor UI and the span ingest endpoint.
#endregion

#region Design
// AddTimeWarpState and AddTimeWarpStateBlazor dogfood the store for InspectorState. This host does
// not reference TimeWarp.State.Telemetry and does not call AddTimeWarpStateDevToolsExporter, so its
// actions are not activities and are not posted back to /ingest/spans. InspectorLog is a singleton
// shared by ingest and the circuit. Inspector actions stay in this process.
// A launch profile sets ASPNETCORE_ENVIRONMENT=Development for dotnet run of this project.
// The packaged tools/ folder has no launch profile. AddTimeWarpStateDevTools sets
// ASPNETCORE_ENVIRONMENT=Development on that executable in run mode. Without the variable the
// host is Production: the exception handler replaces the developer exception page, and ingest
// CORS is not registered. MapStaticAssets serves _framework/blazor.web.js. Outside Development,
// dotnet run does not attach the static web asset file provider, and the asset endpoint then
// looks for that script under wwwroot. UseStaticWebAssets attaches the manifest in every environment.
// Ingest CORS is registered only in Development and only when an origin list is configured.
// RequireCors limits the policy to POST /ingest/spans. The server exporter does not send Origin,
// so it still posts when the policy is absent.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// DevTools host entry point.
/// </summary>
public class Program
{
  /// <summary>
  /// Starts the inspector.
  /// </summary>
  public static void Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents();

    builder.Services.AddSingleton<InspectorLog>();
    builder.Services.AddGeneratedMediator<ClientPipeline>();
    builder.Services.AddTimeWarpState();
    builder.Services.AddTimeWarpStateBlazor();
    bool ingestCors = DevToolsIngestCors.Add(builder);

    WebApplication app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
      app.UseExceptionHandler("/Error");
    }

    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);
    app.MapStaticAssets();
    if (ingestCors)
    {
      DevToolsIngestCors.Use(app);
    }

    app.UseAntiforgery();

    SpanIngestEndpoint.Map(app, ingestCors);

    app.MapRazorComponents<Components.App>()
      .AddInteractiveServerRenderMode();

    app.Run();
  }
}
