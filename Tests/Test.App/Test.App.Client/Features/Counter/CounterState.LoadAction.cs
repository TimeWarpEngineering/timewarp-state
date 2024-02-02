namespace Test.App.Client.Features.Counter;

using BlazorState.Features.Persistence.Abstractions;
using BlazorState.Features.Persistence.Attributes;
using BlazorState.Services;
using System.Diagnostics.CodeAnalysis;

public partial class CounterOneState
{
  public static class Load
  {
    
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by BlazorState")]
    public class Action : IAction { }

    public class Handler(IStore store,
      IPersistenceService PersistenceService
    ) : ActionHandler<Action>(store)
    {
      CounterOneState CounterState => Store.GetState<CounterOneState>();
      
      public override async Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        Console.WriteLine("Entering CounterState.Load.Handler: CounterState.Count: {0} CounterState.Guid {1} ", CounterState.Count, CounterState.Guid);
        Console.WriteLine("CounterState.Load.Handler: Loading CounterState");
        
        object? state = await PersistenceService.LoadState(typeof(CounterOneState), PersistentStateMethod.LocalStorage);
        if (state is CounterOneState counterState)
        {
          Console.WriteLine("Loaded CounterState.Load.Handler: counterState.Count: {0} counterState.Guid {1}", counterState.Count, counterState.Guid);
          Store.SetState(counterState);
          Console.WriteLine("Loaded CounterState.Load.Handler: CounterState.Count: {0} CounterState.Guid {1}", CounterState.Count, CounterState.Guid);
        }
      }
    }
  }
}
