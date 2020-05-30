namespace TestApp.Client
{
  using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
  using Microsoft.Extensions.DependencyInjection;
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;
  using BlazorState;
  using System.Reflection;
  using MediatR;

  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebAssemblyHostBuilder.CreateDefault(args);
      builder.RootComponents.Add<App>("app");
      builder.Services.AddSingleton
      (
        new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
      );

      ConfigureServices(builder.Services);

      await builder.Build().RunAsync();
    }

    public static void ConfigureServices(IServiceCollection aServiceCollection)
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
            typeof(Program).GetTypeInfo().Assembly,
          }
      );
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(Features.EventStream.EventStreamBehavior<,>));

    }
  }
}