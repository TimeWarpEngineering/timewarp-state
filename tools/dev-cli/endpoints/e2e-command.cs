#region Purpose
// Run Playwright end-to-end tests for TimeWarp.State
#endregion
#region Design
// Wraps scripts/e2e.cs so SUT publish, Playwright install, and chrome.runsettings
// stay in one place. Defaults UseHttp=true when unset (matches CI). Ensures
// artifacts/packages exists first (NU1301). Child non-zero exit fails this process.
#endregion

namespace DevCli.Commands;

[NuruRoute("e2e", Description = "Run Playwright end-to-end tests")]
internal sealed class E2eCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<E2eCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(E2eCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Value;
      }

      string e2eRunner = Path.Combine(repoRoot, "scripts", "e2e.cs");
      if (!File.Exists(e2eRunner))
      {
        Terminal.WriteErrorLine($"E2E runner not found: {e2eRunner}");
        Environment.ExitCode = 1;
        return Value;
      }

      Directory.CreateDirectory(Path.Combine(repoRoot, "artifacts", "packages"));

      if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UseHttp")))
      {
        Environment.SetEnvironmentVariable("UseHttp", "true");
      }

      Terminal.WriteLine("Running Playwright end-to-end tests...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("run", "--file", e2eRunner)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("E2E tests failed!".Red());
        Environment.ExitCode = exitCode;
        return Value;
      }

      Terminal.WriteLine("\nE2E tests completed successfully!".Green());
      return Value;
    }
  }
}
