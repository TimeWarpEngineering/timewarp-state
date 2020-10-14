namespace TestApp.EndToEnd.Tests.Infrastructure
{
  using System;
  using System.Diagnostics;
  using System.Net.Http;

  public class SeleniumStandAlone : IDisposable
  {
    public SeleniumStandAlone()
    {
      //Process = new Process()
      //{
      //  StartInfo = new ProcessStartInfo
      //  {
      //    FileName = "selenium-standalone",
      //    Arguments = "start",
      //    UseShellExecute = true
      //  }
      //};
      //Process.Start();
      WaitForSelenium().Wait();
    }

    internal async System.Threading.Tasks.Task WaitForSelenium()
    {
      using var httpClient = new HttpClient();
      using HttpResponseMessage response = await httpClient.GetAsync("http://localhost:4444/wd/hub");

      response.EnsureSuccessStatusCode();

    }

    public Process Process { get; }

    public void Dispose() => Process?.Kill();
  }
}