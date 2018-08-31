namespace BlazorState.EndToEnd.Tests.Infrastructure
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

    protected static string FindSampleOrTestSitePath(string aProjectName)
    {
      string solutionDir = FindSolutionDir();
      string[] possibleLocations = new[]
      {
        Path.Combine(solutionDir, "samples", "TimeWarpBlazor", aProjectName),
      };

      return possibleLocations.FirstOrDefault(Directory.Exists)
        ?? throw new ArgumentException($"Cannot find a sample or test site with name '{aProjectName}'.");
    }

    protected static string FindSolutionDir()
    {
      return FindClosestDirectoryContaining(
        aFilename: "BlazorState.sln",
        aStartDirectory: Path.GetDirectoryName(typeof(ServerFixture).Assembly.Location));
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

      string sampleSitePath = FindSampleOrTestSitePath(
                BuildWebHostMethod.Method.DeclaringType.Assembly.GetName().Name);

      IWebHost webHost = BuildWebHostMethod(new[]
      {
        "--urls", "http://127.0.0.1:0",
        "--contentroot", sampleSitePath,
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

    private static string FindClosestDirectoryContaining(
      string aFilename,
      string aStartDirectory)
    {
      string directory = aStartDirectory;
      while (true)
      {
        if (File.Exists(Path.Combine(directory, aFilename)))
          return directory;

        directory = Directory.GetParent(directory)?.FullName;
        if (string.IsNullOrEmpty(directory))
        {
          throw new FileNotFoundException(
              $"Could not locate a file called '{aFilename}' in " +
              $"directory '{aStartDirectory}' or any parent directory.");
        }
      }
    }
  }
}