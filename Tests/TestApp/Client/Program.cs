namespace TestApp.Client
{
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
  using Microsoft.Extensions.DependencyInjection;
  using System;
  using System.Net.Http;
  using System.Reflection;
  using System.Threading.Tasks;

  public class Program
  {
    public static Task Main(string[] args)
    {
      var builder = WebAssemblyHostBuilder.CreateDefault(args);
      builder.RootComponents.Add<App>("app");
      builder.Services.AddSingleton
      (
        new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
      );

      ConfigureServices(builder.Services);

      return builder.Build().RunAsync();
    }

    public static void ConfigureServices(IServiceCollection aServiceCollection)
    {

      aServiceCollection.AddBlazorState
      (
        (aOptions) =>
        {
#if ReduxDevToolsEnabled
          aOptions.UseReduxDevToolsBehavior = true;
#endif
          aOptions.Assemblies =
          new Assembly[]
          {
            typeof(Program).GetTypeInfo().Assembly,
          };
        }
      );
      aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(Features.EventStream.EventStreamBehavior<,>));

    }
  }
}