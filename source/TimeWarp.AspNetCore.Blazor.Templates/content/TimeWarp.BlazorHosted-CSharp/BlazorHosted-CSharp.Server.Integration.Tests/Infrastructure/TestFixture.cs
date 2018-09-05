namespace BlazorHosted_CSharp.Server.Integration.Tests
{
  using System;
  using System.IO;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;

  public class TestFixture
  {
    public TestFixture()
    {
      IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
      ConfigurationRoot = configurationBuilder.Build();

      var startup = new Startup();
      var services = new ServiceCollection();
      startup.ConfigureServices(services);
      ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// This is the ServiceProvider that will be used by the Server
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    private IConfigurationRoot ConfigurationRoot { get; }
  }
}