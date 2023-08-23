namespace TestApp.Client;

using BlazorState;
using BlazorState.Pipeline.ReduxDevTools;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TestApp.Client.Pipeline.NotificationPostProcessor;
using TestApp.Client.Pipeline.NotificationPreProcessor;

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
      aOptions =>
      {
#if ReduxDevToolsEnabled
        aOptions
        .UseReduxDevTools
        (
          aReduxDevToolsOptions => 
            {
              aReduxDevToolsOptions.Name = "Test App";
              aReduxDevToolsOptions.Trace = true; 
            }
        );
#endif
        aOptions.Assemblies =
          new Assembly[]
          {
                typeof(Program).GetTypeInfo().Assembly,
          };
      }
    );
    aServiceCollection.AddTransient(typeof(IRequestPreProcessor<>), typeof(PrePipelineNotificationRequestPreProcessor<>));
    aServiceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PostPipelineNotificationRequestPostProcessor<,>));
    aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(Features.EventStream.EventStreamBehavior<,>));
    aServiceCollection.AddSingleton(aServiceCollection);

  }
}
