using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace BlazorHosted_CSharp.EndToEnd.Tests.Infrastructure
{
  public class SeleniumStandAlone : IDisposable
  {
    public SeleniumStandAlone()
    {
      Process = new Process()
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = "selenium-standalone",
          Arguments = "start",
          UseShellExecute = true
        }
      };
      Process.Start();
      WaitForSelenium().Wait();
    }

    internal async System.Threading.Tasks.Task WaitForSelenium()
    {
      var httpClient = new HttpClient();
      HttpResponseMessage response = await httpClient.GetAsync("http://localhost:4444/wd/hub");
      response.EnsureSuccessStatusCode();
    }

    public Process Process { get; }

    public void Dispose() => Process.CloseMainWindow();
  }
}