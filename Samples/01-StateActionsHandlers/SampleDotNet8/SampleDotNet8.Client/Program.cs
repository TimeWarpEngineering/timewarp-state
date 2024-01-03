namespace SampleDotNet8.Client;

using Blazored.SessionStorage;
using BlazorState;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SampleDotNet8.Client.Pipeline.PersistentState;
using System.Reflection;

public class Program
{
  private static async Task Main(string[] args)
  {
    Console.WriteLine("Hello, World!");
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    ConfigureServices(builder.Services);

    await builder.Build().RunAsync();

  }
  public static void ConfigureServices(IServiceCollection serviceCollection)
  {
    //serviceCollection.AddSessionStorageServices();
    serviceCollection.AddBlazoredSessionStorage();
    serviceCollection.AddBlazorState
    (
      options =>
      {
        options.UseReduxDevTools();
        options.Assemblies =
        new Assembly[]
        {
          typeof(Program).GetTypeInfo().Assembly,
        };
      }
    );

    serviceCollection.AddTransient(typeof(IRequestPostProcessor<,>), typeof(PersistentStatePostProcessor<,>));
  }
}
