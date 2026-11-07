#region Purpose
// Launch the TimeWarp.State test app (test-app-server hosting test-app-client WASM)
#endregion
#region Design
// Wraps `dotnet run --project tests/test-app/test-app-server --launch-profile <profile>`.
// Profiles come from that project's Properties/launchSettings.json: https (default,
// https://localhost:7011;http://localhost:5058) and http (http://localhost:5058).
// --http is a shortcut for --profile http. Remaining args pass through to
// `dotnet run` (for example `-- --no-build`). Unknown profiles are refused before
// launch. dev.cs sets NUGET_PACKAGES before the child run. The child exit code
// becomes this process's exit code.
#endregion

namespace DevCli.Commands;

[NuruRoute("run", Description = "Launch the test app (test-app-server hosting test-app-client)")]
internal sealed class RunCommand : ICommand<Unit>
{
  internal const string DefaultProfile = "https";
  internal const string HttpProfile = "http";

  [Option("profile", "p", Description = "Launch profile: https (default) or http")]
  public string? Profile { get; set; }

  [Option("http", null, Description = "Shortcut for --profile http")]
  public bool Http { get; set; }

  [Parameter(IsCatchAll = true, Description = "Extra arguments passed to dotnet run")]
  public string[] Args { get; set; } = [];

  internal sealed class Handler : ICommandHandler<RunCommand, Unit>
  {
    private static readonly string[] KnownProfiles = [DefaultProfile, HttpProfile];

    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async Task<Unit> Handle(RunCommand command, CancellationToken ct)
    {
      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string? profile = ResolveProfile(command);
      if (profile is null)
      {
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string projectPath = Path.Combine(repoRoot, "tests", "test-app", "test-app-server");
      string projectFile = Path.Combine(projectPath, "test-app-server.csproj");
      if (!File.Exists(projectFile))
      {
        Terminal.WriteErrorLine($"Test app project not found: {projectFile}");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string[] arguments =
      [
        "run",
        "--project", projectPath,
        "--launch-profile", profile,
        .. command.Args
      ];

      Terminal.WriteLine($"Launching test app (profile: {profile})...");
      Terminal.WriteLine($"Working from: {repoRoot}");
      Terminal.WriteLine($"dotnet {string.Join(' ', arguments)}");

      int exitCode = await Shell.Builder("dotnet")
        .WithArguments(arguments)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);

      if (exitCode != 0)
      {
        Terminal.WriteErrorLine($"Test app exited with code {exitCode}.".Red());
        Environment.ExitCode = exitCode;
      }

      return Unit.Value;
    }

    private string? ResolveProfile(RunCommand command)
    {
      string? requested = command.Profile?.Trim();
      if (command.Http)
      {
        if (!string.IsNullOrEmpty(requested) && !string.Equals(requested, HttpProfile, StringComparison.OrdinalIgnoreCase))
        {
          Terminal.WriteErrorLine($"Error: --http conflicts with --profile {requested}.");
          return null;
        }

        return HttpProfile;
      }

      if (string.IsNullOrEmpty(requested))
      {
        return DefaultProfile;
      }

      foreach (string known in KnownProfiles)
      {
        if (string.Equals(requested, known, StringComparison.OrdinalIgnoreCase))
        {
          return known;
        }
      }

      Terminal.WriteErrorLine($"Error: unknown profile '{requested}'. Use one of: {string.Join(", ", KnownProfiles)}.");
      return null;
    }
  }
}
