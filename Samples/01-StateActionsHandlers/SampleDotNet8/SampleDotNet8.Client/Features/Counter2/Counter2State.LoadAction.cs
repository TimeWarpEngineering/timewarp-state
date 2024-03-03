namespace SampleDotNet8.Client.Features.Counter2;

public partial class Counter2State
{
  public static class Load
  {
    
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by BlazorState")]
    public class Action : IAction { }

    public class Handler(IStore store,
      IPersistenceService PersistenceService
    ) : ActionHandler<Action>(store)
    {
      Counter2State Counter2State => Store.GetState<Counter2State>();
      
      public override async Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        Console.WriteLine("Entering Counter2State.Load.Handler: Counter2State.Count: {0} Counter2State.Guid {1} ", Counter2State.Count, Counter2State.Guid);
        
        try
        {
          object? state = await PersistenceService.LoadState(typeof(Counter2State), PersistentStateMethod.LocalStorage);
          if (state is Counter2State counter2State)
          {
            Console.WriteLine("Loaded Counter2State.Load.Handler: Counter2State.Count: {0} Counter2State.Guid {1}", counter2State.Count, counter2State.Guid);
            Store.SetState(counter2State);
            Console.WriteLine("Loaded Counter2State.Load.Handler: Counter2State.Count: {0} Counter2State.Guid {1}", Counter2State.Count, Counter2State.Guid);
          }
        }
        catch (Exception e)
        {
          // if this is a JavaScript not available exception then we are prerendering and just ignore it
          Console.WriteLine("**********************************");
          Console.WriteLine(e);
          Console.WriteLine("**********************************");
        }
        
      }
    }
  }
}
