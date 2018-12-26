namespace BlazorHosted_CSharp.Client
{
  using Blazor.Extensions.Logging;
  using BlazorState;
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