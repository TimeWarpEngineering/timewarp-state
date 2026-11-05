#region Purpose
// Shared pipeline primitives for `dev workflow`: the returned result, the banner, and the step runner.
#endregion

#region Design
// Pipelines return a PipelineResult; only WorkflowCommand.Handler turns it into Environment.ExitCode.
// The step endpoints (clean, build, test, e2e, verify-samples, check-version) still report failure through
// Environment.ExitCode, so RunStepAsync resets it before the step and reads it straight after. A step that
// throws or is cancelled also fails the pipeline with its reason instead of escaping past the banner.
#endregion

namespace DevCli.Commands;

internal sealed record PipelineResult(bool Succeeded, string? FailureReason)
{
  public static PipelineResult Success { get; } = new(true, null);

  public static PipelineResult Failed(string reason) => new(false, reason);
}

internal sealed class PipelineConsole
{
  private const string Rule = "===============================================================================";

  public PipelineConsole(ITerminal terminal)
  {
    Terminal = terminal;
  }

  public ITerminal Terminal { get; }

  public void Banner(string text)
  {
    Terminal.WriteLine("");
    Terminal.WriteLine(Rule);
    Terminal.WriteLine($"  {text}");
    Terminal.WriteLine(Rule);
  }

  public PipelineResult Abort(string reason)
  {
    Banner($"Pipeline ABORTED — {reason}");
    return PipelineResult.Failed(reason);
  }

  public PipelineResult AbortUnhandled(string checkName, Enum status)
  {
    Terminal.WriteErrorLine($"Release gate failed: unhandled {checkName} status '{status}'.");
    return Abort($"unhandled {checkName} status");
  }

  public async Task<bool> RunStepAsync(string stepName, Func<Task> step, CancellationToken ct)
  {
    Environment.ExitCode = 0;

    try
    {
      await step();
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
      Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} cancelled".Red());
      return false;
    }
    catch (Exception exception)
    {
      Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} threw: {exception.Message}".Red());
      return false;
    }

    int stepExitCode = Environment.ExitCode;
    Environment.ExitCode = 0;

    if (stepExitCode != 0)
    {
      Terminal.WriteErrorLine($"\nPipeline FAILED — {stepName} failed".Red());
      return false;
    }

    return true;
  }
}
