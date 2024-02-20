namespace Test.App.Client.Features.Blue;

public partial class BlueState
{
  // ReSharper disable once UnusedType.Global
  public static class Load
  {
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by BlazorState")]
    public class Action : IAction { }

    // ReSharper disable once UnusedType.Global
    public class Handler
    (
      IStore store,
      IPersistenceService PersistenceService
    ) : ActionHandler<Action>(store)
    {
      
      public override async Task Handle(Action aAction, CancellationToken aCancellationToken)
      {
        try
        {
          object? state = await PersistenceService.LoadState(typeof(BlueState), PersistentStateMethod.LocalStorage);
          if (state is BlueState blueState) Store.SetState(blueState);
        }
        catch (Exception e)
        {
          // if this is a JavaScript not available exception then we are prerendering and just ignore it
        }
      }
    }
  }
}
