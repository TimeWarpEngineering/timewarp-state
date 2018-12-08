namespace ServerSideSample.App
{
  using Blazor.Extensions.Storage;
  using BlazorState;
  using Microsoft.AspNetCore.Blazor.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using ServerSideSample.App.Services;

  public class Startup
  {
    public void Configure(IBlazorApplicationBuilder aBlazorApplicationBuilder) => 
      aBlazorApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      // Since Blazor is running on the server, we can use an application service
      // to read the forecast data.
      aServiceCollection.AddSingleton<WeatherForecastService>();
      aServiceCollection.AddStorage();
      aServiceCollection.AddBlazorState();
    }
  }
}