namespace SampleDotNet8.Client.Features.Counter;

using BlazorState.Features.Persistence.Abstractions;
using BlazorState.Features.Persistence.Attributes;
using BlazorState.Services;
using System.Diagnostics.CodeAnalysis;

public partial class CounterState
{
  public static class Load
  {
    
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by BlazorState")]
    public class Action : IAction { }

    public class Handler(IStore store,
      IPersistenceService PersistenceService,
      RenderPhaseService RenderPhaseService
    ) : ActionHandler<Action>(store)
    {
      CounterState CounterState => Store.GetState<CounterState>();
      
      public override async Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        Console.WriteLine("Entering CounterState.Load.Handler: CounterState.Count: {0} CounterState.Guid {1} ", CounterState.Count, CounterState.Guid);
        
        if (RenderPhaseService.IsPreRendering)
        {
          Console.WriteLine("Skipping Load as we are prerendering");
          return;
        }
        
        Console.WriteLine("CounterState.Load.Handler: Loading CounterState");
        
        object? state = await PersistenceService.LoadState(typeof(CounterState), PersistentStateMethod.LocalStorage);
        if (state is CounterState counterState)
        {
          Console.WriteLine("Loaded CounterState.Load.Handler: counterState.Count: {0} counterState.Guid {1}", counterState.Count, counterState.Guid);
          Store.SetState(counterState);
          Console.WriteLine("Loaded CounterState.Load.Handler: CounterState.Count: {0} CounterState.Guid {1}", CounterState.Count, CounterState.Guid);
        }
      }
    }
  }
}
