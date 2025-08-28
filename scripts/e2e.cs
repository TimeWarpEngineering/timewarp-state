#!/usr/bin/dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Nuru
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

var app = new NuruAppBuilder()
    .AddDefaultRoute(async () => await RunE2eTests())
    .AddAutoHelp()
    .Build();

return await app.RunAsync(args);

async Task RunE2eTests()
{
    using var context = ScriptContext.FromRelativePath("..");

    // Configuration variables
    var sutProjectDir = "./tests/test-app/test-app-server";
    var outputPath = "./tests/test-app/output";
    var useHttp = true;
    var protocol = useHttp ? "http" : "https";
    var sutUrl = $"{protocol}://localhost";
    var testProjectDir = "./tests/test-app-end-to-end-tests";
    var testProjectPath = $"{testProjectDir}/test-app-end-to-end-tests.csproj";
    var analyzerProjectPath = "./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj";
    var sourceGeneratorProjectPath = "./source/timewarp-state-source-generator/timewarp-state-source-generator.csproj";
    var sutPort = 7011;
    var maxRetries = 30;
    var retryInterval = 1;
    var runMode = "Auto"; // Possible values: "Auto", "Manual", "Development", "Release"

    await WriteStepHeader("Restore-Tools-And-Cleanup");
    await RestoreToolsAndCleanup();
    await WriteStepFooter("Restore-Tools-And-Cleanup");

    await WriteStepHeader("Install-LinuxDevCerts");
    await InstallLinuxDevCerts();
    await WriteStepFooter("Install-LinuxDevCerts");

    await WriteStepHeader("Build-Analyzer");
    await BuildAnalyzer(analyzerProjectPath);
    await WriteStepFooter("Build-Analyzer");

    await WriteStepHeader("Build-SourceGenerator");
    await BuildSourceGenerator(sourceGeneratorProjectPath);
    await WriteStepFooter("Build-SourceGenerator");

    await WriteStepHeader("Update-ClientAppSettings");
    await UpdateClientAppSettings(useHttp);
    await WriteStepFooter("Update-ClientAppSettings");

    await WriteStepHeader("Build-And-Publish-Sut");
    await BuildAndPublishSut(sutProjectDir, outputPath);
    await WriteStepFooter("Build-And-Publish-Sut");

    await WriteStepHeader("Build-Test");
    await BuildTest(testProjectDir);
    await WriteStepFooter("Build-Test");

    await WriteStepHeader("Ensure-Browsers-Installed");
    await EnsureBrowsersInstalled(testProjectDir);
    await WriteStepFooter("Ensure-Browsers-Installed");

    await WriteStepHeader("Start-Sut");
    var sutProcess = await StartSut(runMode, outputPath, sutUrl, sutPort, useHttp);
    await WriteStepFooter("Start-Sut");

    try
    {
        await WaitForSut($"{sutUrl}:{sutPort}", maxRetries, retryInterval);

        var testsFailed = await RunTests(testProjectDir, useHttp);

        if (runMode == "Auto")
        {
            var outputLogPath = Path.Combine(outputPath, "sut_output.log");
            var errorLogPath = Path.Combine(outputPath, "sut_error.log");

            if (testsFailed)
            {
                WriteLine("Tests failed. Displaying SUT logs:");
                await DisplaySutLogs(outputLogPath, errorLogPath);
            }
            else
            {
                WriteLine("Tests passed. SUT logs available at:");
                WriteLine($"Output log: {outputLogPath}");
                WriteLine($"Error log: {errorLogPath}");
            }
        }

        if (runMode is "Development" or "Release")
        {
            WriteLine($"Tests completed. SUT is still running in {runMode} mode. Press Ctrl+C to stop.");
            // In a real implementation, you'd handle cancellation tokens here
        }
    }
    catch (Exception ex)
    {
        WriteLine($"An error occurred during test execution: {ex.Message}");
        if (runMode == "Auto")
        {
            var outputLogPath = Path.Combine(outputPath, "sut_output.log");
            var errorLogPath = Path.Combine(outputPath, "sut_error.log");
            await DisplaySutLogs(outputLogPath, errorLogPath);
        }
    }
    finally
    {
        if (runMode == "Auto" && sutProcess != null)
        {
            await KillSut(sutProcess);
        }
        else
        {
            WriteLine($"Please remember to stop the SUT process running in {runMode} mode.");
        }
    }
}

async Task WriteStepHeader(string stepName)
{
    WriteLine($"\n========== Starting: {stepName} ==========");
}

async Task WriteStepFooter(string stepName)
{
    WriteLine($"========== Completed: {stepName} ==========\n");
}

async Task EnsureBrowsersInstalled(string testProjectDir)
{
    var playwrightPath = $"{testProjectDir}/bin/Debug/net9.0/playwright.ps1";
    if (File.Exists(playwrightPath))
    {
        WriteLine("Installing Playwright Chromium browser...");
        var exitCode = await Shell.Builder("pwsh")
            .WithArguments(playwrightPath, "install", "chromium", "--with-deps")
            .RunAsync();
        if (exitCode != 0)
        {
            WriteLine($"Warning: Playwright installation may have issues. Exit code: {exitCode}");
        }
    }
    else
    {
        throw new Exception($"Playwright script not found at {playwrightPath}. Make sure the Test.App.EndToEnd.Tests project is built.");
    }
}

async Task RestoreToolsAndCleanup()
{
    // Restore .NET tools
    var exitCode = await DotNet.Tool().Restore().Build().RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Clean the solution
    exitCode = await DotNet.Clean().Build().RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Clean the Output directory
    if (Directory.Exists("./tests/test-app/output"))
    {
        Directory.Delete("./tests/test-app/output", true);
        WriteLine("Deleted: ./tests/test-app/output");
    }
}

async Task BuildAnalyzer(string analyzerProjectPath)
{
    var exitCode = await DotNet.Build()
        .WithProject(analyzerProjectPath)
        .WithConfiguration("Release")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

async Task BuildSourceGenerator(string sourceGeneratorProjectPath)
{
    var exitCode = await DotNet.Build()
        .WithProject(sourceGeneratorProjectPath)
        .WithConfiguration("Release")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

async Task UpdateClientAppSettings(bool useHttp)
{
    var appSettingsPath = "./tests/test-app/test-app-client/wwwroot/appsettings.json";
    if (File.Exists(appSettingsPath))
    {
        var json = File.ReadAllText(appSettingsPath);
        var appSettings = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
        // In a real implementation, you'd properly update the JSON
        // For now, we'll use a simple string replacement
        var updatedJson = json.Replace("\"UseHttp\": false", "\"UseHttp\": true")
                             .Replace("\"UseHttp\": true", $"\"UseHttp\": {useHttp.ToString().ToLower()}");
        File.WriteAllText(appSettingsPath, updatedJson);
    }
}

async Task BuildAndPublishSut(string sutProjectDir, string outputPath)
{
    // Restore dependencies
    var exitCode = await DotNet.Restore()
        .WithProject($"{sutProjectDir}/test-app-server.csproj")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Build the solution
    exitCode = await DotNet.Build()
        .WithProject($"{sutProjectDir}/test-app-server.csproj")
        .WithConfiguration("Release")
        .WithNoRestore()
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Publish the SUT
    exitCode = await DotNet.Publish()
        .WithProject($"{sutProjectDir}/test-app-server.csproj")
        .WithConfiguration("Release")
        .WithOutput(outputPath)
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

async Task BuildTest(string testProjectDir)
{
    // Restore dependencies
    var exitCode = await DotNet.Restore()
        .WithProject($"{testProjectDir}/test-app-end-to-end-tests.csproj")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);

    // Build the test project
    exitCode = await DotNet.Build()
        .WithProject($"{testProjectDir}/test-app-end-to-end-tests.csproj")
        .WithConfiguration("Debug")
        .Build()
        .RunAsync();
    if (exitCode != 0) Environment.Exit(1);
}

async Task<System.Diagnostics.Process?> StartSut(string mode, string outputPath, string sutUrl, int sutPort, bool useHttp)
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
                    // Redirect output to files
                    using var outputWriter = new StreamWriter(outputLogPath);
                    using var errorWriter = new StreamWriter(errorLogPath);
                    process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputWriter.WriteLine(e.Data); };
                    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorWriter.WriteLine(e.Data); };
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

async Task WaitForSut(string url, int maxRetries, int retryInterval)
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

async Task DisplaySutLogs(string outputLogPath, string errorLogPath)
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

async Task<bool> RunTests(string testProjectDir, bool useHttp)
{
    var settings = new[] { "chrome.runsettings" };
    var testsFailed = false;

    WriteLine("Running E2E tests");
    Environment.SetEnvironmentVariable("UseHttp", useHttp.ToString().ToLower());

    foreach (var setting in settings)
    {
        var targetArguments = new[] {
            "test",
            "--no-build",
            $"--settings:playwright-settings/{setting}",
            "-e", $"UseHttp={useHttp.ToString().ToLower()}",
            "./test-app-end-to-end-tests.csproj"
        };

        WriteLine($"Executing: dotnet {string.Join(" ", targetArguments)}");

        var exitCode = await Shell.Builder("dotnet")
            .WithArguments(targetArguments)
            .WithWorkingDirectory(testProjectDir)
            .RunAsync();

        if (exitCode != 0)
        {
            testsFailed = true;
            break;
        }
    }

    return testsFailed;
}

async Task KillSut(System.Diagnostics.Process sutProcess)
{
    if (!sutProcess.HasExited)
    {
        sutProcess.Kill();
        WriteLine("SUT process terminated.");
    }
}

async Task InstallLinuxDevCerts()
{
    if (OperatingSystem.IsLinux())
    {
        WriteLine("Installing Linux development certificates...");
        var exitCode = await DotNet.DevCerts().Https().WithClean().Build().RunAsync();
        exitCode = await DotNet.DevCerts().Https().WithTrust().Build().RunAsync();
        if (exitCode == 0)
        {
            WriteLine("Linux development certificates installed successfully.");
        }
        else
        {
            WriteLine("Failed to install Linux development certificates.");
        }
    }
    else
    {
        WriteLine("Skipping Linux development certificate installation (not running on Linux).");
    }
}