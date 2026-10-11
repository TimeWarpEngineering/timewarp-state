#region Purpose
// Hosts the DevTools Blazor UI and the span ingest endpoint.
#endregion

#region Design
// AddTimeWarpState and AddTimeWarpStateBlazor dogfood the store for InspectorState. This host does
// not reference TimeWarp.State.Telemetry and does not call AddTimeWarpStateDevToolsExporter, so its
// actions are not activities and are not posted back to /ingest/spans. InspectorLog is a singleton
// shared by ingest and the circuit. Referencing TimeWarp.State.Blazor links CommitHandler, whose
// constructor needs IReduxDevToolsStore. UseReduxDevTools supplies that registration. This host does
// not render the Redux DevTools component, so InitAsync never runs and inspector actions stay here.
// MapStaticAssets serves _framework/blazor.web.js. Outside Development, dotnet run does not attach the
// static web asset file provider, and the asset endpoint then looks for that script under wwwroot.
// UseStaticWebAssets attaches the manifest in every environment.
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
    builder.Services.AddTimeWarpState(options => options.UseReduxDevTools());
    builder.Services.AddTimeWarpStateBlazor();

    WebApplication app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
      app.UseExceptionHandler("/Error");
    }

    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);
    app.MapStaticAssets();
    app.UseAntiforgery();

    SpanIngestEndpoint.Map(app);

    app.MapRazorComponents<Components.App>()
      .AddInteractiveServerRenderMode();

    app.Run();
  }
}
