#region Purpose
// Starts the system under test, waits for it, prints its logs, and stops it.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. Log writers live on App so KillSut can dispose them after the process exits.
#endregion

static partial class App
{
  static async Task<System.Diagnostics.Process?> StartSut(string mode, string outputPath, string sutUrl, int sutPort, bool useHttp)
{
    switch (mode)
    {
        case "Manual":
            WriteLine("Please start the SUT in another console using the following command:");
            WriteLine($"{outputPath}/Test.App.Server --urls {sutUrl}:{sutPort}");
            WriteLine("Press Enter when the SUT is ready...");
            ReadLine();
            return null;

        case "Development":
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            WriteLine("Starting SUT in Development mode...");
            var devExitCode = await Shell.Builder("dotnet")
                .WithArguments("run", "--urls", $"{sutUrl}:{sutPort}")
                .WithWorkingDirectory("./tests/test-app/test-app-server")
                .RunAsync();
            return null;

        case "Release":
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            WriteLine("Starting SUT in Release configuration...");
            var releaseExitCode = await Shell.Builder("dotnet")
                .WithArguments("run", "--configuration", "Release", "--urls", $"{sutUrl}:{sutPort}")
                .WithWorkingDirectory("./tests/test-app/test-app-server")
                .RunAsync();
            return null;

        default:
            // Auto mode
            WriteLine("Starting SUT in Auto mode...");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var executableName = OperatingSystem.IsWindows() ? "Test.App.Server.exe" : "Test.App.Server";
            var executablePath = Path.Combine(outputPath, executableName);

            if (File.Exists(executablePath))
            {
                var outputLogPath = Path.Combine(outputPath, "sut_output.log");
                var errorLogPath = Path.Combine(outputPath, "sut_error.log");
                WriteLine($"Starting SUT: {executablePath} --urls {sutUrl}:{sutPort}");
                WriteLine($"Output log: {outputLogPath}");
                WriteLine($"Error log: {errorLogPath}");

                Environment.SetEnvironmentVariable("UseHttp", useHttp.ToString().ToLower());

                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = $"--urls {sutUrl}:{sutPort}",
                    WorkingDirectory = outputPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                });

                if (process != null)
                {
                    // Redirect output to files. The writers are disposed in KillSut, not here: disposing them on
                    // return made the first SUT output line throw ObjectDisposedException on a thread-pool thread.
                    SutOutputWriter = new StreamWriter(outputLogPath) { AutoFlush = true };
                    SutErrorWriter = new StreamWriter(errorLogPath) { AutoFlush = true };
                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) { try { SutOutputWriter?.WriteLine(e.Data); } catch (ObjectDisposedException) { } } };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) { try { SutErrorWriter?.WriteLine(e.Data); } catch (ObjectDisposedException) { } } };
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                return process;
            }
            else
            {
                throw new Exception($"Executable not found at {executablePath}");
            }
    }
}

  static async Task WaitForSut(string url, int maxRetries, int retryInterval)
{
    using var client = new HttpClient();
    client.Timeout = TimeSpan.FromSeconds(5);

    for (int retries = 0; retries < maxRetries; retries++)
    {
        try
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                WriteLine("SUT is ready.");
                return;
            }
        }
        catch
        {
            WriteLine($"Attempt {retries + 1} failed");
        }

        await Task.Delay(TimeSpan.FromSeconds(retryInterval));
    }

    throw new Exception("SUT did not become ready in time.");
}

  static async Task DisplaySutLogs(string outputLogPath, string errorLogPath)
{
    WriteLine("Displaying SUT logs:");

    if (File.Exists(outputLogPath))
    {
        WriteLine("SUT Output:");
        WriteLine(await File.ReadAllTextAsync(outputLogPath));
    }
    else
    {
        WriteLine($"SUT Output log file not found at: {outputLogPath}");
    }

    if (File.Exists(errorLogPath))
    {
        WriteLine("SUT Error Output:");
        WriteLine(await File.ReadAllTextAsync(errorLogPath));
    }
    else
    {
        WriteLine($"SUT Error log file not found at: {errorLogPath}");
    }
}

  static async Task KillSut(System.Diagnostics.Process sutProcess)
{
    if (!sutProcess.HasExited)
    {
        sutProcess.Kill();
        WriteLine("SUT process terminated.");
    }

    // Wait on both paths: the OutputDataReceived handlers may still be draining into the writers below.
    sutProcess.WaitForExit();

    SutOutputWriter?.Dispose();
    SutErrorWriter?.Dispose();
    SutOutputWriter = null;
    SutErrorWriter = null;
}
}
