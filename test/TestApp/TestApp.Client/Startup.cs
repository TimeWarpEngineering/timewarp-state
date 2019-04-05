namespace TestApp.Client
{
  using BlazorState;
  using BlazorState.Services;
  using MediatR;
  using Microsoft.AspNetCore.Components.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using TestApp.Client.Features.EventStream;

  public class Startup
  {
    public void Configure(IComponentsApplicationBuilder aBlazorApplicationBuilder) =>
      aBlazorApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      if (new BlazorHostingLocation().IsClientSide)
      {
        // TODO add this back once Blazor.Extentions.Logging is updated to 0.8.0
        //aServiceCollection.AddLogging(aLoggingBuilder => aLoggingBuilder
        //    .AddBrowserConsole()
        //    .SetMinimumLevel(LogLevel.Trace));
      };
      aServiceCollection.AddBlazorState();
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventStreamBehavior<,>));
    }
  }
}