namespace BlazorState.Client
{
  using Microsoft.AspNetCore.Blazor.Hosting;

  public class Program
  {
    private static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
        BlazorWebAssemblyHost.CreateDefaultBuilder()
            .UseBlazorStartup<Startup>();
  }
}