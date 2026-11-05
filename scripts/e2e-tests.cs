#region Purpose
// Runs the Playwright end-to-end project from the e2e runfile.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. Amuru 3 returns the dotnet test exit code, so a failed run is a
// false return, not an exception. UseHttp is set only on the dotnet test child; the test host inherits it.
#endregion

static partial class App
{
  static async Task<bool> RunTests(string testProjectDir, bool useHttp)
  {
    string[] arguments =
    [
      "test",
      "--no-build",
      "--settings:playwright-settings/chrome.runsettings",
      "./test-app-end-to-end-tests.csproj"
    ];
    string useHttpValue = useHttp ? "true" : "false";

    WriteLine($"Executing: UseHttp={useHttpValue} dotnet {string.Join(" ", arguments)}");
    int exitCode = await Shell.Builder("dotnet")
      .WithArguments(arguments)
      .WithWorkingDirectory(testProjectDir)
      .WithEnvironmentVariable("UseHttp", useHttpValue)
      .RunAsync();

    if (exitCode != 0)
    {
      WriteLine($"dotnet test exited with {exitCode}.");
    }

    return exitCode == 0;
  }
}
