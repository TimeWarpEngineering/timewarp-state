using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace BlazorState.EndToEnd.Tests.Infrastructure
{
  public class ServerFixture
  {
    private readonly Lazy<Uri> _rootUriInitializer;

    public ServerFixture()
    {
      _rootUriInitializer = new Lazy<Uri>(() =>
        new Uri(StartAndGetRootUri()));
    }

    public delegate IWebHost BuildWebHost(string[] args);

    public BuildWebHost BuildWebHostMethod { get; set; }
    public AspNetEnvironment Environment { get; set; } = AspNetEnvironment.Production;
    public Uri RootUri => _rootUriInitializer.Value;
    private IWebHost WebHost { get; set; }

    protected static string FindSampleOrTestSitePath(string projectName)
    {
      string solutionDir = FindSolutionDir();
      string[] possibleLocations = new[]
      {
        Path.Combine(solutionDir, "samples", projectName),
        Path.Combine(solutionDir, "samples", "Hosted", projectName),
        Path.Combine(solutionDir, "test", "testapps", projectName)
      };

      return possibleLocations.FirstOrDefault(Directory.Exists)
          ?? throw new ArgumentException($"Cannot find a sample or test site with name '{projectName}'.");
    }

    protected static string FindSolutionDir()
    {
      return FindClosestDirectoryContaining(
          "BlazorState.sln",
          Path.GetDirectoryName(typeof(ServerFixture).Assembly.Location));
    }

    protected static void RunInBackgroundThread(Action action)
    {
      var isDone = new ManualResetEvent(false);

      new Thread(() =>
      {
        action();
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

      return BuildWebHostMethod(new[]
      {
                "--urls", "http://127.0.0.1:0",
                "--contentroot", sampleSitePath,
                "--environment", Environment.ToString(),
            });
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
                      string filename,
      string startDirectory)
    {
      string dir = startDirectory;
      while (true)
      {
        if (File.Exists(Path.Combine(dir, filename)))
        {
          return dir;
        }

        dir = Directory.GetParent(dir)?.FullName;
        if (string.IsNullOrEmpty(dir))
        {
          throw new FileNotFoundException(
              $"Could not locate a file called '{filename}' in " +
              $"directory '{startDirectory}' or any parent directory.");
        }
      }
    }
  }
}