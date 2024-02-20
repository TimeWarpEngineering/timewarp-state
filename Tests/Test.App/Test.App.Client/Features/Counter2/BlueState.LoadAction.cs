namespace Test.App.Client.Features.Counter2;

public partial class BlueState
{
  public static class Load
  {
    
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by BlazorState")]
    public class Action : IAction { }

    public class Handler(IStore store,
      IPersistenceService PersistenceService
    ) : ActionHandler<Action>(store)
    {
      BlueState BlueState => Store.GetState<BlueState>();
      
      public override async Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        Console.WriteLine("Entering Counter2State.Load.Handler: Counter2State.Count: {0} Counter2State.Guid {1} ", BlueState.Count, BlueState.Guid);
        
        try
        {
          object? state = await PersistenceService.LoadState(typeof(BlueState), PersistentStateMethod.LocalStorage);
          if (state is BlueState counter2State)
          {
            Console.WriteLine("Loaded Counter2State.Load.Handler: Counter2State.Count: {0} Counter2State.Guid {1}", counter2State.Count, counter2State.Guid);
            Store.SetState(counter2State);
            Console.WriteLine("Loaded Counter2State.Load.Handler: Counter2State.Count: {0} Counter2State.Guid {1}", BlueState.Count, BlueState.Guid);
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
