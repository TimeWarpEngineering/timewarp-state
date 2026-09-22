namespace TimeWarp.State.Telemetry.Tests;

internal sealed class TelemetryTestState : IState
{
  [JsonIgnore]
  public ISender<ClientPipeline> Sender { get; set; } = null!;

  public Guid Guid { get; set; } = Guid.NewGuid();

  public int Count { get; set; }

  public void Initialize() { }

  public void CancelOperations() { }

  public sealed class IncrementAction : IAction;

  public sealed class ThrowAction : IAction;
}
