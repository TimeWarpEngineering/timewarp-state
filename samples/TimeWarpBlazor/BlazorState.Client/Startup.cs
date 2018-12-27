namespace BlazorState.Client
{
  using Blazor.Extensions.Logging;
  using BlazorState.Client.Services;
  using BlazorState.Services;
  using Microsoft.AspNetCore.Blazor.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  public class Startup
  {
    public void Configure(IBlazorApplicationBuilder aBlazorApplicationBuilder) =>
      aBlazorApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      aServiceCollection.AddSingleton<WeatherForecastService>();
      if (new JsRuntimeLocation().IsClientSide)
      {
        aServiceCollection.AddLogging(aLoggingBuilder => aLoggingBuilder
          .AddBrowserConsole()
          .SetMinimumLevel(LogLevel.Trace));
      };

      aServiceCollection.AddBlazorState();
    }
  }
}