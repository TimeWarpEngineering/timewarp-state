namespace TestApp.Client;

using BlazorState;
using MediatR;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

public class Program
{
  public static Task Main(string[] args)
  {
    SelfLog.Enable(m => Console.Error.WriteLine(m));

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.BrowserConsole()
        .CreateLogger();

    Log.Debug("Hello, browser!");
    Log.Warning("Received strange response {@Response} from server", new { Username = "example", Cats = 7 });

    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    builder.RootComponents.Add<App>("#app");
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
        aOptions.UseReduxDevTools();
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
