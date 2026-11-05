#region Purpose
// SutHost: starts the published test-app server, waits for it to answer, stops it, and prints its logs.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. The server runs through Amuru StreamCombinedAsync on a background
// pump task that writes each line to sut_output.log or sut_error.log. The host owns the cancellation source, the
// pump, and both writers. DisposeAsync cancels the token, which makes CliWrap kill the process tree. It then waits a
// bounded time for the pump and closes the writers only once the pump has finished. It never throws, so callers can
// dispose from a finally block, and it is idempotent. HasExited is the pump being complete, so WaitUntilReadyAsync
// stops early when the server dies before it answers.
#endregion

sealed class SutHost : IAsyncDisposable
{
  static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(15);

  readonly CancellationTokenSource Stopping = new();
  readonly StreamWriter OutputWriter;
  readonly StreamWriter ErrorWriter;
  readonly CommandResult Server;
  readonly Task Pump;
  bool Disposed;

  SutHost(string executablePath, string workingDirectory, string url, bool useHttp)
  {
    Url = url;
    OutputLogPath = Path.Combine(workingDirectory, "sut_output.log");
    ErrorLogPath = Path.Combine(workingDirectory, "sut_error.log");
    OutputWriter = new StreamWriter(OutputLogPath) { AutoFlush = true };
    ErrorWriter = new StreamWriter(ErrorLogPath) { AutoFlush = true };
    Server = Shell.Builder(executablePath)
      .WithArguments("--urls", url)
      .WithWorkingDirectory(workingDirectory)
      .WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
      .WithEnvironmentVariable("UseHttp", useHttp ? "true" : "false")
      .Build();
    Pump = PumpAsync(Stopping.Token);
  }

  public string Url { get; }

  public string OutputLogPath { get; }

  public string ErrorLogPath { get; }

  public bool HasExited => Pump.IsCompleted;

  public static SutHost Start(string outputPath, string url, bool useHttp)
  {
    string workingDirectory = Path.GetFullPath(outputPath);
    string executableName = OperatingSystem.IsWindows() ? "Test.App.Server.exe" : "Test.App.Server";
    string executablePath = Path.Combine(workingDirectory, executableName);
    if (!File.Exists(executablePath))
    {
      throw new E2eStepFailedException($"SUT executable not found at {executablePath}.");
    }

    WriteLine($"Starting SUT: {executablePath} --urls {url}");
    SutHost host = new(executablePath, workingDirectory, url, useHttp);
    WriteLine($"Output log: {host.OutputLogPath}");
    WriteLine($"Error log: {host.ErrorLogPath}");
    return host;
  }

  public async Task WaitUntilReadyAsync(int maxAttempts, TimeSpan retryInterval)
  {
    using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
      if (HasExited)
      {
        throw new E2eStepFailedException($"SUT exited before it became ready ({DescribeExit()}). See {ErrorLogPath}.");
      }

      try
      {
        using HttpResponseMessage response = await client.GetAsync(Url);
        if (response.IsSuccessStatusCode)
        {
          WriteLine($"SUT is ready at {Url} (attempt {attempt}).");
          return;
        }

        WriteLine($"Attempt {attempt}/{maxAttempts}: {Url} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
      }
      catch (HttpRequestException ex)
      {
        WriteLine($"Attempt {attempt}/{maxAttempts}: {ex.Message}");
      }
      catch (TaskCanceledException)
      {
        WriteLine($"Attempt {attempt}/{maxAttempts}: no response within {client.Timeout.TotalSeconds:0}s.");
      }

      await Task.Delay(retryInterval);
    }

    throw new E2eStepFailedException($"SUT did not become ready at {Url} after {maxAttempts} attempts.");
  }

  /// <summary>Prints both SUT logs. Call after DisposeAsync so the logs are complete and no writer holds them.</summary>
  public async Task DisplayLogsAsync()
  {
    WriteLine("Displaying SUT logs:");
    await DisplayLogAsync("SUT Output", OutputLogPath);
    await DisplayLogAsync("SUT Error Output", ErrorLogPath);
  }

  public async ValueTask DisposeAsync()
  {
    if (Disposed)
    {
      return;
    }

    Disposed = true;
    if (!Pump.IsCompleted)
    {
      WriteLine("Stopping SUT...");
      await Stopping.CancelAsync();
    }

    try
    {
      await Pump.WaitAsync(StopTimeout);
      WriteLine($"SUT stopped ({DescribeExit()}).");
    }
    catch (TimeoutException)
    {
      WriteLine($"Warning: SUT output did not close within {StopTimeout.TotalSeconds:0}s of the stop request.");
    }
    catch (Exception ex)
    {
      WriteLine($"Warning: SUT run failed: {ex.Message}");
    }

    // A pump that is still running would write into a closed writer; leave the writers to the process exit then.
    if (Pump.IsCompleted)
    {
      await OutputWriter.DisposeAsync();
      await ErrorWriter.DisposeAsync();
      Stopping.Dispose();
    }
  }

  async Task PumpAsync(CancellationToken cancellationToken)
  {
    // Yield first so the constructor returns before the server starts streaming.
    await Task.Yield();
    try
    {
      await foreach (OutputLine line in Server.StreamCombinedAsync(cancellationToken))
      {
        (line.IsError ? ErrorWriter : OutputWriter).WriteLine(line.Text);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      // The stop request: CliWrap has killed the process tree.
    }
  }

  string DescribeExit() =>
    Stopping.IsCancellationRequested ? "stopped by the e2e run"
    : Server.LastOutput is { } output ? $"exit code {output.ExitCode}"
    : "exit code unknown";

  static async Task DisplayLogAsync(string title, string path)
  {
    if (File.Exists(path))
    {
      WriteLine($"{title}:");
      WriteLine(await File.ReadAllTextAsync(path));
    }
    else
    {
      WriteLine($"{title} log file not found at: {path}");
    }
  }
}
