#region Purpose
// Run the TimeWarp.State test suite
#endregion
#region Design
// Restores local tools (fixie.console from .config/dotnet-tools.json) so
// `dotnet fixie` is available on a clean CI checkout. Then delegates to
// scripts/test.cs for analyzer/state/plus/client/architecture suites.
// E2E stays on scripts/e2e.cs (needs a running SUT).
#endregion

namespace DevCli.Commands;

[NuruRoute("test", Description = "Run the test suite")]
internal sealed class TestCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<TestCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(TestCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Value;
      }

      string testRunner = Path.Combine(repoRoot, "scripts", "test.cs");
      if (!File.Exists(testRunner))
      {
        Terminal.WriteErrorLine($"Test runner not found: {testRunner}");
        Environment.ExitCode = 1;
        return Value;
      }

      Directory.CreateDirectory(Path.Combine(repoRoot, "artifacts", "packages"));

      Terminal.WriteLine("Restoring local tools (fixie.console)...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      int restoreExitCode = await Shell.Builder("dotnet")
        .WithArguments("tool", "restore")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (restoreExitCode != 0)
      {
        Terminal.WriteErrorLine("dotnet tool restore failed!".Red());
        Environment.ExitCode = restoreExitCode;
        return Value;
      }

      Terminal.WriteLine("Running test suite...");

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("run", "--file", testRunner)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("Tests failed!".Red());
        Environment.ExitCode = exitCode;
        return Value;
      }

      Terminal.WriteLine("\nTests completed successfully!".Green());
      return Value;
    }
  }
}
