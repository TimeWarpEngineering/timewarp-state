namespace TestApp.Client
{
  using Microsoft.AspNetCore.Blazor.Hosting;

  public class Program
  {
    private static IWebAssemblyHostBuilder CreateHostBuilder(string[] aArgumentArray) =>
        BlazorWebAssemblyHost.CreateDefaultBuilder()
        .UseBlazorStartup<Startup>();

    private static void Main(string[] aArgumentArray) => CreateHostBuilder(aArgumentArray).Build().Run();
  }
}