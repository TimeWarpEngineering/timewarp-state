namespace TimeWarp.State.Plus.PersistentState;

// Disambiguate from Microsoft.AspNetCore.Components.PersistentStateAttribute (added in .NET 10).
using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;

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
    // Only persistent states auto-load; skip the dispatch entirely for the common (non-persistent) case.
    if (stateInitializedNotification.StateType.GetCustomAttribute<PersistentStateAttribute>() is null) return;

    Logger.LogDebug
    (
      EventIds.StateInitializedNotificationHandler_Handling,
      message: "StateInitializedNotificationHandler: {StateTypeName}",
      stateInitializedNotification.StateType.Name
    );

    // Load via a hand-written mediator request whose handler IS linked (see LoadPersistentStateRequest).
    await Sender.Send(new LoadPersistentStateRequest(stateInitializedNotification.StateType), cancellationToken);
  }
}
