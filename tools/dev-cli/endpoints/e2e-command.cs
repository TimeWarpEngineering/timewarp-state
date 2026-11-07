#region Purpose
// Run Playwright end-to-end tests for TimeWarp.State
#endregion
#region Design
// Wraps scripts/e2e.cs so SUT publish, Playwright install, and chrome.runsettings
// stay in one place. Defaults UseHttp=true when unset (matches CI). dev.cs sets
// NUGET_PACKAGES to artifacts/nuget-packages before this command starts
// scripts/e2e.cs. Child non-zero exit fails this process. Root nuget.config does
// not list artifacts/packages, so this command does not create that folder.
// --skip-setup and --with-deps pass through to scripts/e2e.cs. The browser step
// skips the install when the Chromium revision Playwright wants is cached, and
// adds Playwright's --with-deps (apt, needs root) only in CI or with --with-deps.
#endregion

namespace DevCli.Commands;

[NuruRoute("e2e", Description = "Run Playwright end-to-end tests. Skips the Chromium install when the revision Playwright wants is cached; --skip-setup skips tool restore, dev certs, and browser install")]
internal sealed class E2eCommand : ICommand<Unit>
{
  [Option("skip-setup", null, Description = "Skip tool restore, dev certs, and Playwright browser install (box already set up)")]
  public bool SkipSetup { get; set; }

  [Option("with-deps", null, Description = "Install Playwright system packages (apt, needs passwordless sudo) outside CI")]
  public bool WithDeps { get; set; }

  internal sealed class Handler : ICommandHandler<E2eCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(E2eCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string e2eRunner = Path.Combine(repoRoot, "scripts", "e2e.cs");
      if (!File.Exists(e2eRunner))
      {
        Terminal.WriteErrorLine($"E2E runner not found: {e2eRunner}");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UseHttp")))
      {
        Environment.SetEnvironmentVariable("UseHttp", "true");
      }

      Terminal.WriteLine("Running Playwright end-to-end tests...");
      Terminal.WriteLine($"Working from: {repoRoot}");

      List<string> arguments = ["run", "--file", e2eRunner, "--"];
      if (command.SkipSetup)
      {
        arguments.Add("--skip-setup");
      }

      if (command.WithDeps)
      {
        arguments.Add("--with-deps");
      }

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments([.. arguments])
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine("E2E tests failed!".Red());
        Environment.ExitCode = exitCode;
        return Unit.Value;
      }

      Terminal.WriteLine("\nE2E tests completed successfully!".Green());
      return Unit.Value;
    }
  }
}
