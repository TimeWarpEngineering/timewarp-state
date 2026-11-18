#region Purpose
// Runs the Playwright end-to-end project from the e2e runfile.
#endregion

#region Design
// scripts/e2e.cs names this file with #:include. Amuru 3 returns the dotnet test exit code, so a failed run is a
// false return, not an exception. UseHttp is set only on the dotnet test child; the test host inherits it.
// --collect turns on coverlet.collector, which this test project already references. IncludeTestAssembly is
// required: coverlet skips the test assembly by default, and this suite loads no product assembly in-process
// (the SUT is a separate process), so the report would otherwise contain no lines. The results directory is
// absolute and reset before the run: the child's working directory is the test project, and a relative path
// would miss the artifacts/coverage tree the workflow uploads. The reset refuses any other directory.
#endregion

static partial class App
{
  static async Task<bool> RunTests(string testProjectDir, bool useHttp, string coverageDirectory)
  {
    string preparedCoverageDirectory = PrepareCoverageDirectory(coverageDirectory);
    const string projectFile = "./test-app-end-to-end-tests.csproj";
    const string settingsFile = "playwright-settings/chrome.runsettings";
    const string collector = "XPlat Code Coverage;IncludeTestAssembly=true";
    string useHttpValue = useHttp ? "true" : "false";

    WriteLine(
      $"Executing: UseHttp={useHttpValue} dotnet test {projectFile} --results-directory {preparedCoverageDirectory} " +
      $"--settings {settingsFile} --no-build --collect \"{collector}\"");
    int exitCode = await DotNet.Test()
      .WithProject(projectFile)
      .WithNoBuild()
      .WithSettings(settingsFile)
      .WithCollect(collector)
      .WithResultsDirectory(preparedCoverageDirectory)
      .WithWorkingDirectory(testProjectDir)
      .WithEnvironmentVariable("UseHttp", useHttpValue)
      .RunAsync();

    if (exitCode != 0)
    {
      WriteLine($"dotnet test exited with {exitCode}.");
    }

    return exitCode == 0;
  }

  static string PrepareCoverageDirectory(string coverageDirectory)
  {
    string fullPath = Path.GetFullPath(coverageDirectory);
    string expectedSuffix = $"{Path.DirectorySeparatorChar}artifacts{Path.DirectorySeparatorChar}coverage";
    if (!fullPath.EndsWith(expectedSuffix, StringComparison.Ordinal))
    {
      throw new E2eStepFailedException($"Refusing to reset coverage directory '{fullPath}'.");
    }

    if (Directory.Exists(fullPath))
    {
      Directory.Delete(fullPath, recursive: true);
    }

    Directory.CreateDirectory(fullPath);
    return fullPath;
  }
}
