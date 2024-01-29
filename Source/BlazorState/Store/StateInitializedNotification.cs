namespace BlazorState;

public class StateInitializedNotification
(
  Type stateType
) : INotification
{
  public Type StateType { get; } = stateType;
}
