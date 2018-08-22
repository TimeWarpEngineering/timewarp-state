namespace BlazorState.Client
{
  using System.Reflection;
  using Blazor.Extensions.Logging;
  using Microsoft.AspNetCore.Blazor.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  public class Startup
  {
    public void Configure(IBlazorApplicationBuilder aBlazorApplicationBuilder) =>
      aBlazorApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      aServiceCollection.AddLogging(aLoggingBuilder => aLoggingBuilder
          .AddBrowserConsole()
          .SetMinimumLevel(LogLevel.Trace)
      );
      aServiceCollection.AddBlazorState(null, typeof(Startup).GetTypeInfo().Assembly);
    }
  }
}