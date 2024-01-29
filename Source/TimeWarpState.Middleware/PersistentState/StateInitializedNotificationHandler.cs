namespace TimeWarpState.Middleware.PersistentState;

using BlazorState;
using MediatR;
using Microsoft.Extensions.Logging;

public class StateInitializedNotificationHandler
(
  ISender Sender
) : INotificationHandler<StateInitializedNotification>
{

  public async Task Handle(StateInitializedNotification stateInitializedNotification, CancellationToken cancellationToken)
  {
    // Logger.LogDebug("StateInitializedNotificationHandler: {StateName}",stateName);
    string fullName = stateInitializedNotification.StateType.FullName ?? throw new InvalidOperationException();
    string assemblyQualifiedName = stateInitializedNotification.StateType.AssemblyQualifiedName ?? throw new InvalidOperationException();
    
    string typeName = assemblyQualifiedName.Replace(fullName, $"{fullName}+Load+Action");
    Console.WriteLine($"StateInitializedNotificationHandler: {stateInitializedNotification.StateType.Name}");
    var actionType = Type.GetType(typeName);
    
    if (actionType != null)
    {
      object action = Activator.CreateInstance(actionType) ?? throw new InvalidOperationException();
      await Sender.Send(action, cancellationToken);
    }
  }
}
