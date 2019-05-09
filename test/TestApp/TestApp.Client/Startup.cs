namespace TestApp.Client
{
  using Blazor.Extensions.Logging;
  using BlazorState;
  using BlazorState.Services;
  using MediatR;
  using Microsoft.AspNetCore.Components.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using TestApp.Client.Features.EventStream;

  public class Startup
  {
    public void Configure(IComponentsApplicationBuilder aComponentsApplicationBuilder) =>
      aComponentsApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      if (new BlazorHostingLocation().IsClientSide)
      {
        aServiceCollection.AddLogging(aLoggingBuilder => aLoggingBuilder
            .AddBrowserConsole()
            .SetMinimumLevel(LogLevel.Trace));
      };
      aServiceCollection.AddBlazorState();
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventStreamBehavior<,>));
    }
  }
}