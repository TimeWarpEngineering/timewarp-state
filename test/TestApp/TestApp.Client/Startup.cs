namespace TestApp.Client
{
  //using Blazor.Extensions.Logging;
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Components.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using System;
  using System.Reflection;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.Counter;
  //using Microsoft.Extensions.Logging;
  using TestApp.Client.Features.EventStream;
  using TestApp.Client.Features.WeatherForecast;

  public class Startup
  {
    public void Configure(IComponentsApplicationBuilder aComponentsApplicationBuilder) =>
      aComponentsApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      //if (new BlazorHostingLocation().IsClientSide)
      //{
      //  aServiceCollection.AddLogging(aLoggingBuilder => aLoggingBuilder
      //      .AddBrowserConsole()
      //      .SetMinimumLevel(LogLevel.Trace));
      //};
      //aServiceCollection.AddBlazorState();
      aServiceCollection.AddBlazorState
      (
        (aOptions) => aOptions.Assemblies =
          new Assembly[] 
          {
            typeof(Startup).GetTypeInfo().Assembly,
          }
      );
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventStreamBehavior<,>));
      aServiceCollection.AddTransient<ApplicationState>();
      aServiceCollection.AddTransient<CounterState>();
      aServiceCollection.AddTransient<EventStreamState>();
      aServiceCollection.AddTransient<WeatherForecastsState>();
    }
  }
}