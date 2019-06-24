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
      // We could allow for direct injection of the State but we still need subscriptions to be set
      // Thus BaseComponent is still needed so why not just access the State that way.
      //aServiceCollection.AddScoped<IState<ApplicationState>>(GetState<ApplicationState>);
      //aServiceCollection.AddScoped<IState<CounterState>>(GetState<CounterState>);
      //aServiceCollection.AddScoped<IState<EventStreamState>>(GetState<EventStreamState>);
      //aServiceCollection.AddScoped<IState<WeatherForecastsState>>(GetState<WeatherForecastsState>);
      aServiceCollection.AddTransient<ApplicationState>();
      aServiceCollection.AddTransient<CounterState>();
      aServiceCollection.AddTransient<EventStreamState>();
      aServiceCollection.AddTransient<WeatherForecastsState>();
    }

    private TState GetState<TState>(IServiceProvider aServiceProvider)
    {
      IStore store = aServiceProvider.GetService<IStore>();
      return store.GetState<TState>();
    }
  }
}