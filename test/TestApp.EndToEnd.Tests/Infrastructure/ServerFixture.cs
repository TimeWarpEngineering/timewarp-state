namespace TestApp.EndToEnd.Tests.Infrastructure
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Threading;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Hosting.Server.Features;

  public class ServerFixture
  {
    private readonly Lazy<Uri> LazyUri;

    public ServerFixture()
    {
      LazyUri = new Lazy<Uri>(() =>
        new Uri(StartAndGetRootUri()));
    }

    public delegate IWebHost BuildWebHost(string[] args);

    public BuildWebHost BuildWebHostMethod { get; set; }
    public AspNetEnvironment Environment { get; set; } = AspNetEnvironment.Production;
    public Uri RootUri => LazyUri.Value;
    private IWebHost WebHost { get; set; }

    /// <summary>
    /// Find the path to the server that you are testing.
    /// </summary>
    /// <param name="aProjectName"></param>
    /// <returns>The Path to the project</returns>
    protected static string FindSitePath(string aProjectName)
    {
      DirectoryInfo gitRootDirectory = new GitService().GitRootDirectoryInfo();
      return Path.Combine(gitRootDirectory.FullName, "test","TestApp","TestApp.Server");
    }

    protected static void RunInBackgroundThread(Action aAction)
    {
      var isDone = new ManualResetEvent(false);

      new Thread(() =>
      {
        aAction();
        isDone.Set();
      }).Start();

      isDone.WaitOne();
    }

    protected IWebHost CreateWebHost()
    {
      if (BuildWebHostMethod == null)
      {
        throw new InvalidOperationException(
            $"No value was provided for {nameof(BuildWebHostMethod)}");
      }

      string sitePath = FindSitePath(
                BuildWebHostMethod.Method.DeclaringType.Assembly.GetName().Name);

      IWebHost webHost = BuildWebHostMethod(new[]
      {
        "--urls", "http://127.0.0.1:0",
        "--contentroot", sitePath,
        "--environment", Environment.ToString(),
      });

      return webHost;
    }

    protected string StartAndGetRootUri()
    {
      WebHost = CreateWebHost();
      RunInBackgroundThread(WebHost.Start);
      return WebHost.ServerFeatures
          .Get<IServerAddressesFeature>()
          .Addresses.Single();
    }

  }
}