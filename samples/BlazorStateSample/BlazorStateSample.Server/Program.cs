namespace BlazorStateSample.Server
{
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;

  public class Program
  {
    public static IWebHost BuildWebHost(string[] args) =>
      WebHost
        .CreateDefaultBuilder(args)
        .UseConfiguration
          (
            new ConfigurationBuilder()
              .AddCommandLine(args)
              .Build()
          )
          .UseStartup<Startup>()
          .Build();

    public static void Main(string[] args) => BuildWebHost(args).Run();
  }
}