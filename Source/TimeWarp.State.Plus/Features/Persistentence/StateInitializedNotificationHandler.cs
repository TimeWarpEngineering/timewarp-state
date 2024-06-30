namespace TimeWarp.State.Plus.PersistentState;

public class StateInitializedNotificationHandler : INotificationHandler<StateInitializedNotification>
{
  private readonly ISender Sender;
  private readonly ILogger<StateInitializedNotificationHandler> Logger;
  public StateInitializedNotificationHandler
  (
    ISender sender,
    ILogger<StateInitializedNotificationHandler> logger
  )
  {
    Sender = sender;
    Logger = logger;
  }

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
