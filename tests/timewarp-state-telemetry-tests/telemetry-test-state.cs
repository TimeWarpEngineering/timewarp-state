#region Purpose
// Minimal IState and actions used by the telemetry tests.
#endregion

#region Design
// Implements IState directly rather than State<T> to keep it free of store behavior. Sender is [JsonIgnore] so
// snapshots contain only Guid and Count. The nested IncrementCountActionSet checks the ActionSet display name.
#endregion

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

  public static class IncrementCountActionSet
  {
    public sealed class Action : IAction;
  }
}
