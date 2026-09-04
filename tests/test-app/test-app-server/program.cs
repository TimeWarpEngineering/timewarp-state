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
    // Use AddServerPipelineMediator (not AddGeneratedMediator<ServerPipeline>) to avoid CS0121 with
    // the client's identically named DI extension in Microsoft.Extensions.DependencyInjection.
    builder.Services.AddServerPipelineMediator();

    builder.Logging.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug);

    WebApplication app = builder.Build();

    ILogger<Program> logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
    builder.Services.LogTimeWarpStateMiddleware(logger);

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
      async (ISender<ServerPipeline> sender, CancellationToken cancellationToken) =>
        Results.Ok(await sender.Send(new GetWeatherForecastsRequest { Days = 5 }, cancellationToken))
    );

    app.Run();

  }
}
