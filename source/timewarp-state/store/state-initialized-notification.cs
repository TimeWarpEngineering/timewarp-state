#region Purpose
// Notification carrying the type of a state that has finished initializing.
#endregion

#region Design
// Immutable INotification with a primary constructor so it can be published through the mediator.
#endregion

namespace TimeWarp.State;

public class StateInitializedNotification
(
  Type stateType
) : INotification
{
  public Type StateType { get; } = stateType;
}
