namespace TestApp.Client
{
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Components.Builder;
  using Microsoft.Extensions.DependencyInjection;
  using System.Reflection;
  using System.Text.Json;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.CloneTest;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Features.EventStream;
  using TestApp.Client.Features.WeatherForecast;

  public class Startup
  {
    public void Configure(IComponentsApplicationBuilder aComponentsApplicationBuilder) =>
      aComponentsApplicationBuilder.AddComponent<App>("app");

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {

      aServiceCollection.AddBlazorState
      (
        (aOptions) =>

#if ReduxDevToolsEnabled
          aOptions.UseReduxDevToolsBehavior = true;
#endif
          aOptions.Assemblies =
            new Assembly[]
            {
                typeof(Startup).GetTypeInfo().Assembly,
            }
      );
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(EventStreamBehavior<,>));

    }
  }
}