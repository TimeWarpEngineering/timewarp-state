using System;
using System.Diagnostics;
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
      Thread.Sleep(2000); // Wait for selenium-standalone to start.
    }

    public Process Process { get; }

    public void Dispose() => Process.CloseMainWindow();
  }
}