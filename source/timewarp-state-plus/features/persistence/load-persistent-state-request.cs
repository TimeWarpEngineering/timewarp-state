namespace TimeWarp.State.Plus.PersistentState;

// Disambiguate from Microsoft.AspNetCore.Components.PersistentStateAttribute (added in .NET 10).
using PersistentStateAttribute = TimeWarp.Features.Persistence.PersistentStateAttribute;

/// <summary>
/// A Mediator request to (re)load a <c>[PersistentState]</c> state from its configured store.
/// </summary>
/// <remarks>
/// This is hand-written (not source-generated) on purpose. Mediator (martinothamar) registers
/// handlers at compile time via its own source generator, which only sees the original syntax
/// trees — never the output of another generator. The previous design generated a per-state
/// <c>LoadActionSet.Handler</c> in the persistence generator, so Mediator never saw or registered
/// it and dispatching the load action threw <see cref="MissingMessageHandlerException"/> at runtime.
/// A single hand-written request + handler lives in source, so Mediator's generator registers it.
/// </remarks>
public class LoadPersistentStateRequest : IRequest
{
  public LoadPersistentStateRequest(Type stateType) => StateType = stateType;

  public Type StateType { get; }
}

public class LoadPersistentStateRequestHandler : IRequestHandler<LoadPersistentStateRequest>
{
  private readonly IStore Store;
  private readonly IPersistenceService PersistenceService;
  private readonly ILogger<LoadPersistentStateRequestHandler> Logger;

  public LoadPersistentStateRequestHandler
  (
    IStore store,
    IPersistenceService persistenceService,
    ILogger<LoadPersistentStateRequestHandler> logger
  )
  {
    Store = store;
    PersistenceService = persistenceService;
    Logger = logger;
  }

  public async ValueTask<Unit> Handle(LoadPersistentStateRequest request, CancellationToken cancellationToken)
  {
    Type stateType = request.StateType;
    PersistentStateAttribute? persistentStateAttribute = stateType.GetCustomAttribute<PersistentStateAttribute>();

    if (persistentStateAttribute is null)
    {
      Logger.LogDebug("LoadPersistentStateRequest: {StateType} is not [PersistentState]; skipping load", stateType.Name);
      return Unit.Value;
    }

    object? loaded = await PersistenceService.LoadState(stateType, persistentStateAttribute.PersistentStateMethod);
    if (loaded is IState loadedState)
    {
      Store.SetState(loadedState);
      Logger.LogTrace("Loaded persisted state {StateType}", stateType.Name);
    }
    else
    {
      Logger.LogTrace("No persisted state found for {StateType}", stateType.Name);
    }

    return Unit.Value;
  }
}
