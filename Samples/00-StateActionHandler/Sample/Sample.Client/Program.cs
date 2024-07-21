namespace Sample.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeWarp.State;

public class Program
{
  static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    ConfigureServices(builder.Services);
    await builder.Build().RunAsync();
  }

  public static void ConfigureServices(IServiceCollection serviceCollection)
  {
    serviceCollection.AddTimeWarpState();
  }
}
