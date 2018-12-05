namespace ServerSideSample.App
{
  using Microsoft.AspNetCore.Blazor.Hosting;

  public class Program
  {
    public static void Main(string[] aArgumentArray) => CreateHostBuilder(aArgumentArray).Build().Run();

    public static IWebAssemblyHostBuilder CreateHostBuilder(string[] aArgumentArray) =>
      BlazorWebAssemblyHost.CreateDefaultBuilder()
        .UseBlazorStartup<Startup>();
  }
}
