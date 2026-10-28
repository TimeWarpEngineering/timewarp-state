#region Purpose
// Test.App.Server entry point: hosts the test app in Server, WebAssembly and Auto render modes plus the weather API.
#endregion

#region Design
// Reuses Client.Program.ConfigureServices so server-rendered pages get the same store, then registers the
// ServerPipeline mediator with AddGeneratedMediator. Generated extensions are internal to each host, so the
// client's extension is not in scope here. The weather endpoint passes the Days query value to the handler;
// Days defaults to 5 so a plain api/weather request still works. The throw-server-side-exception endpoint
// throws InvalidOperationException with the query message.
#endregion

namespace Test.App.Server;

using Components;
using Contracts.Features.WeatherForecast;
using Features.WeatherForecast;

internal class Program
{
  private static void Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    Client.Program.ConfigureServices(builder.Services, builder.Configuration);

    // Server-scoped pipeline: only handlers/behaviors marked ServerPipeline. The client store
    // pipeline was already registered above by Client.Program.ConfigureServices as ISender<ClientPipeline>.
    // AddGeneratedMediator is internal to this assembly (TimeWarp.Mediator 15), so the
    // client's copy is not visible and the call is not ambiguous. The client's MediatorManifest
    // is internal too, so this log lists the server pipeline only.
    builder.Services.AddGeneratedMediator<ServerPipeline>();

    builder.Logging.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug);

    WebApplication app = builder.Build();

    ILogger<Program> logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
    builder.Services.LogTimeWarpStateMiddleware(logger, MediatorManifest.Json);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseWebAssemblyDebugging();
    }
    else
    {
      app.UseExceptionHandler("/Error", createScopeForErrors: true);
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    // app.UseHttpsRedirection();

    app.MapStaticAssets();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Test.App.Client.AssemblyMarker).Assembly);

    app.MapGet
    (
      GetWeatherForecasts.Query.RouteTemplate,
      async (ISender<ServerPipeline> sender, CancellationToken cancellationToken, int days = 5) =>
        Results.Ok(await sender.Send(new GetWeatherForecastsRequest { Days = days }, cancellationToken))
    );

    app.MapGet
    (
      ThrowServerSideExceptionRequest.RouteTemplate,
      (HttpContext httpContext) =>
      {
        string sampleProperty = httpContext.Request.Query[nameof(ThrowServerSideExceptionRequest.SampleProperty)].ToString();
        string message = string.IsNullOrEmpty(sampleProperty) ? "Server-side exception" : sampleProperty;
        throw new InvalidOperationException(message);
      }
    );

    app.Run();

  }
}
