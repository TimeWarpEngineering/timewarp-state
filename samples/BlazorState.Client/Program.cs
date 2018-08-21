namespace BlazorState.Client
{
  using System.Reflection;
  using Blazor.Extensions.Logging;
  using BlazorState;
  using Microsoft.AspNetCore.Blazor.Browser.Rendering;
  using Microsoft.AspNetCore.Blazor.Browser.Services;
  using Microsoft.AspNetCore.Blazor.Hosting;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  public class Program
  {
    private static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
        BlazorWebAssemblyHost.CreateDefaultBuilder()
            .UseBlazorStartup<Startup>();
  }
}