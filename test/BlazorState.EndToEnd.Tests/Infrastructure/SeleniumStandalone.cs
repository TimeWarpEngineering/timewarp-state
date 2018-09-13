namespace BlazorState.EndToEnd.Tests.Infrastructure
{
  using System;
  using System.Diagnostics;
  using System.Threading;

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
      Thread.Sleep(1000); // Wait for selenium-standalone to start.
      // TODO: should be able to tell when ready some how.
    }

    public Process Process { get; }

    public void Dispose() => Process.CloseMainWindow();
  }
}