#region Purpose
// Runs the Playwright end-to-end project from the e2e runfile.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. A non-zero dotnet test exit is an exception on this Amuru line, so the catch records the failure.
#endregion

static partial class App
{
  static async Task<bool> RunTests(string testProjectDir, bool useHttp)
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

        try
        {
            var exitCode = await Shell.Builder("dotnet")
                .WithArguments(targetArguments)
                .WithWorkingDirectory(testProjectDir)
                .RunAsync();
            if (exitCode != 0) testsFailed = true;
        }
        catch (Exception ex)
        {
            // Non-zero dotnet test exit surfaces as an exception on this Amuru line.
            WriteLine($"dotnet test failed: {ex.Message.Split('\n')[0]}");
            testsFailed = true;
        }

        if (testsFailed) break;
    }

    return testsFailed;
}
}
