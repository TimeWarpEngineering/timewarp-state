namespace TimeWarp.State.Middleware;

using Blazored.SessionStorage;
using BlazorState;
using BlazorState.Features.Persistence.Attributes;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Reflection;

public class PersistentStatePostProcessor<TRequest, TResponse>
(
  ILogger<PersistentStatePostProcessor<TRequest, TResponse>> logger,
  IStore Store,
  ISessionStorageService SessionStorageService,
  ILocalStorageService LocalSessionStorageService
) : IRequestPostProcessor<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger = logger;


  public async Task Process(TRequest aRequest, TResponse response, CancellationToken cancellationToken)
  {
    // TODO: remove below or replace with proper Logger.Debug
    Console.WriteLine($"PersistentStatePostProcessor: {typeof(TRequest).FullName}");
    // We want to persist using the method defined by the PersistentStateAttribute
    // Should we inject a common interface IPersistentState/IPersistState?
    // Then the user would register the IPersistentState with a name and the method.
    // Then we could have a dictionary of IPersistState and call the appropriate one based on PersistentStateMethod?
    // Or we could have the IPersistState.Persist method take the PersistentStateMethod as a parameter?

    Type currentType = typeof(TRequest);
    while (currentType.DeclaringType != null)
    {
      currentType = currentType.DeclaringType;
    }
    // currentType is now the top-level non-nested type

    PersistentStateAttribute? persistentStateAttribute =
      currentType.GetCustomAttribute<PersistentStateAttribute>();
      
    if (persistentStateAttribute is null) return;

    object state = Store.GetState(currentType);

    switch (persistentStateAttribute.PersistentStateMethod)
    {
      case PersistentStateMethod.Server:
        break;
      case PersistentStateMethod.SessionStorage:
        Console.WriteLine($"Save to Session Storage {currentType.Name}");
        await SessionStorageService.SetItemAsync(currentType.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.LocalStorage:
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Save to Local Storage {currentType.Name}");
        await LocalSessionStorageService.SetItemAsync(currentType.Name, state, cancellationToken);
        break;
      case PersistentStateMethod.PreRender:
        // TODO: This needs to be tried and see if improves UX.
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }
}
