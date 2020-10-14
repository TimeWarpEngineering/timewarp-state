namespace TestApp.EndToEnd.Tests.Infrastructure
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Hosting.Server;
  using Microsoft.AspNetCore.Hosting.Server.Features;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using System;
  using System.IO;
  using System.Linq;
  using System.Threading;

  public class ServerFixture
  {
    private readonly Lazy<Uri> LazyUri;

    public ServerFixture()
    {
      LazyUri = new Lazy<Uri>(() =>
        new Uri(StartAndGetRootUri()));
    }

    public delegate IHostBuilder CreateHostBuilder(string[] aArgumentArray);

    public CreateHostBuilder CreateHostBuilderDelegate { get; set; }
    public AspNetEnvironment Environment { get; set; } = AspNetEnvironment.Production;
    public Uri RootUri => LazyUri.Value;
    private IHost Host { get; set; }

    /// <summary>
    /// Find the path to the server that you are testing.
    /// </summary>
    /// <returns>The Path to the project</returns>
    protected static string FindSitePath()
    {
      DirectoryInfo gitRootDirectory = new GitService().GitRootDirectoryInfo();
      return Path.Combine(gitRootDirectory.FullName, "Tests", "TestApp", "Server");
    }

    protected static void RunInBackgroundThread(Action aAction)
    {
      using var isDone = new ManualResetEvent(false);

      new Thread(() =>
      {
        aAction();
        isDone.Set();
      }).Start();

      isDone.WaitOne();
    }

    protected IHost CreateWebHost()
    {
      if (CreateHostBuilderDelegate == null)
      {
        throw new InvalidOperationException(
            $"No value was provided for {nameof(CreateHostBuilderDelegate)}");
      }

      string sitePath = FindSitePath();

      IHostBuilder hostBuilder = CreateHostBuilderDelegate(new[]
      {
        "--urls", "http://127.0.0.1:0",
        "--contentroot", sitePath,
        "--environment", Environment.ToString(),
      });

      return hostBuilder.Build();
    }

    protected string StartAndGetRootUri()
    {
      Host = CreateWebHost();
      RunInBackgroundThread(Host.Start);
      return Host
        .Services
        .GetRequiredService<IServer>()
        .Features
        .Get<IServerAddressesFeature>()
        .Addresses.Single();
    }

  }
}