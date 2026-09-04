#region Purpose
// Build the TimeWarp.State solution in Release
#endregion
#region Design
// Names timewarp-state.slnx explicitly. Ensures artifacts/packages exists first
// because nuget.config lists that folder as a local source (NU1301 when missing).
#endregion

namespace DevCli.Commands;

[NuruRoute("build", Description = "Build all projects in the repository")]
internal sealed class BuildCommand : ICommand<Unit>
{
  [Option("clean", "c", Description = "Clean before building")]
  public bool Clean { get; set; }

  [Option("quiet", "q", Description = "Hide build output unless the command fails")]
  public bool Quiet { get; set; }

  internal sealed class Handler : ICommandHandler<BuildCommand, Unit>
  {
    private readonly ITerminal Terminal;
    private BuildCommand Command = null!;
    private CancellationToken Ct;
    private string RepoRoot = null!;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(BuildCommand command, CancellationToken ct)
    {
      Command = command;
      Ct = ct;

      if (!FindRepoRoot())
      {
        return Value;
      }

      Directory.CreateDirectory(Path.Combine(RepoRoot, "artifacts", "packages"));

      if (!await CleanAsync())
      {
        return Value;
      }

      if (!await BuildAsync())
      {
        return Value;
      }

      Terminal.WriteLine("\nBuild completed successfully!".Green());
      return Value;
    }

    private bool FindRepoRoot()
    {
      string? root = Git.FindRoot();
      if (root is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        Environment.ExitCode = 1;
        return false;
      }

      RepoRoot = root;
      Terminal.WriteLine($"Building repository at {RepoRoot}...");
      return true;
    }

    private async Task<bool> CleanAsync()
    {
      if (!Command.Clean)
      {
        return true;
      }

      string solutionFile = Path.Combine(RepoRoot, "timewarp-state.slnx");
      Terminal.WriteLine($"\nCleaning {solutionFile}...");
      CommandResult command = DotNet.Clean()
        .WithProject(solutionFile)
        .WithNoValidation()
        .Build();

      return await ExecuteAsync(command, "Clean failed!");
    }

    private async Task<bool> BuildAsync()
    {
      string solutionFile = Path.Combine(RepoRoot, "timewarp-state.slnx");
      Terminal.WriteLine($"\nBuilding {solutionFile}...");
      CommandResult command = DotNet.Build()
        .WithProject(solutionFile)
        .WithConfiguration("Release")
        .WithNoValidation()
        .Build();

      return await ExecuteAsync(command, "Build failed!");
    }

    private async Task<bool> ExecuteAsync(CommandResult command, string failureMessage)
    {
      if (Command.Quiet)
      {
        CommandOutput result = await command.CaptureAsync(Ct);
        if (!result.Success)
        {
          Terminal.WriteErrorLine(result.Combined);
          Terminal.WriteErrorLine(failureMessage.Red());
          Environment.ExitCode = 1;
          return false;
        }

        return true;
      }

      int exitCode = await command.RunAsync(Ct);
      if (exitCode != 0)
      {
        Terminal.WriteErrorLine(failureMessage.Red());
        Environment.ExitCode = exitCode;
        return false;
      }

      return true;
    }
  }
}
