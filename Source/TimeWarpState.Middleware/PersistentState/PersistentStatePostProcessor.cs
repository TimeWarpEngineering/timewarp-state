namespace TimeWarpState.Middleware;

using Blazored.SessionStorage;
using BlazorState;
using BlazorState.Features.Persistence.Attributes;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Reflection;

internal class PersistentStatePostProcessor<TRequest, TResponse>
(
  ILogger<PersistentStatePostProcessor<TRequest, TResponse>> logger,
  IStore store,
  ISessionStorageService sessionStorageService
) : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger = logger;
  private readonly IStore Store = store;
  private readonly ISessionStorageService SessionStorageService = sessionStorageService;

  public async Task Process(TRequest aRequest, TResponse response, CancellationToken cancellationToken)
  {
    // We want to persist using the method defined by the PersistentStateAttribute
    // Should we inject a common interface IPersistentState/IPersistState?
    // Then the user would register the IPersistentState with a name and the method.
    // Then we could have a dictionary of IPersistState and call the appropriate one based on PersistentStateMethod?
    // Or we could have the IPersistState.Persist method take the PersistentStateMethod as a parameter?

    Type requestType = typeof(TRequest);
    Type currentType = requestType;
    while (currentType.DeclaringType != null)
    {
      currentType = currentType.DeclaringType;
    }
    // currentType is now the top-level non-nested type

    // Get the PersistentStateAttribute
    PersistentStateAttribute? persistentStateAttribute =
      currentType?.GetCustomAttribute<PersistentStateAttribute>();
      
    if (persistentStateAttribute is null) return;

    object state = Store.GetStateAsync(currentType);

    // switch on the PersistentStateMethod
    switch (persistentStateAttribute.PersistentStateMethod)
    {
      case PersistentStateMethod.Server:
        break;
      case PersistentStateMethod.SessionStorage:
        Console.WriteLine("Save to Session Storage");
        await SessionStorageService.SetItemAsync(currentType!.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.LocalStorage:
        Console.WriteLine("Save to Local Storage");
        break;
    }
  }
}
