namespace SampleDotNet8.Client.Features.Counter;

public partial class CounterState
{
  public static class Load
  {
    
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by TimeWarp.State")]
    public class Action : IAction { }

    public class Handler(IStore store,
      IPersistenceService PersistenceService
    ) : ActionHandler<Action>(store)
    {
      CounterState CounterState => Store.GetState<CounterState>();
      
      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Entering CounterState.Load.Handler: CounterState.Count: {0} CounterState.Guid {1} ", CounterState.Count, CounterState.Guid);
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
