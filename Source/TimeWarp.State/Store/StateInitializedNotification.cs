namespace TimeWarp.State;

public class StateInitializedNotification
(
  Type stateType
) : INotification
{
  public Type StateType { get; } = stateType;
}
