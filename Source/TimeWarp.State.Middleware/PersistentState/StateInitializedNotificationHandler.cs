namespace TimeWarp.State.Middleware.PersistentState;

public class StateInitializedNotificationHandler
(
  ISender Sender,
  ILogger<StateInitializedNotificationHandler> logger
) : INotificationHandler<StateInitializedNotification>
{
  private ILogger Logger => logger;

  public async Task Handle(StateInitializedNotification stateInitializedNotification, CancellationToken cancellationToken)
  {
    string fullName = stateInitializedNotification.StateType.FullName ?? throw new InvalidOperationException();
    string assemblyQualifiedName = stateInitializedNotification.StateType.AssemblyQualifiedName ?? throw new InvalidOperationException();
    
    string typeName = assemblyQualifiedName.Replace(fullName, $"{fullName}+Load+Action");
    Logger.LogDebug("StateInitializedNotificationHandler: {StateTypeName}", stateInitializedNotification.StateType.Name);
    var actionType = Type.GetType(typeName);
    
    if (actionType != null)
    {
      object action = Activator.CreateInstance(actionType) ?? throw new InvalidOperationException();
      await Sender.Send(action, cancellationToken);
    }
  }
}
