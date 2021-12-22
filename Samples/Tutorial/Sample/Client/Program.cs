namespace Sample.Client;

using BlazorState;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    ConfigureServices(builder.Services);

    await builder.Build().RunAsync();
  }

  public static void ConfigureServices(IServiceCollection aServiceCollection)
  {
    aServiceCollection.AddBlazorState
    (
      (aOptions) =>
      {
        aOptions.UseReduxDevToolsBehavior = true;
        aOptions.Assemblies =
          new Assembly[]
          {
              typeof(Program).GetTypeInfo().Assembly,
          };
      }
    );
  }
}
