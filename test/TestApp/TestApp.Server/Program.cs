namespace TestApp.Server
{
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;

  public class Program
  {
    public static IWebHost BuildWebHost(string[] aArgumentArray) =>
        WebHost.CreateDefaultBuilder(aArgumentArray)
            .UseConfiguration(new ConfigurationBuilder()
              .AddCommandLine(aArgumentArray)
              .Build())
            .UseStartup<Startup>()
            .Build();

    public static void Main(string[] aArgumentArray) => BuildWebHost(aArgumentArray).Run();
  }
}