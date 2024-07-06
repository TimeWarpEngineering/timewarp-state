namespace Test.App.Server;

using Components;
using Contracts.Features.WeatherForecast;

internal class Program
{
  private static void Main(string[] args)
  {
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();
    
    Client.Program.ConfigureServices(builder.Services);
    builder.Logging.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug);
    
    WebApplication app = builder.Build();

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

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Test.App.Client.AssemblyMarker).Assembly);

    // Define the /api/weather endpoint
    app.MapGet("/api/weather", () =>
    {
      var startDate = DateOnly.FromDateTime(DateTime.Now);
      string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
      GetWeatherForecasts.WeatherForecastDto[] forecasts = 
        Enumerable.Range(1, 5)
          .Select
          (
            index => 
              new GetWeatherForecasts.WeatherForecastDto
              (
                startDate.AddDays(index),
                summaries[Random.Shared.Next(summaries.Length)],
                Random.Shared.Next(-20, 55)
              )
          ).ToArray();
        
      return Results.Ok(forecasts);
    });

    app.Run();
    
  }
}
