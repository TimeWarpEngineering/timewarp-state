namespace Test.App.Client.Features.Purple;

using TimeWarp.Features.Persistence;
using TimeWarp.State;

public partial class PurpleState
{
  public static class Load2
  {
    public class Action : IAction { }

    public class Handler : ActionHandler<Action>
    {
      private readonly IPersistenceService PersistenceService1;
      private readonly ILogger<Handler> Logger;
      public Handler
      (
        IStore store,
        IPersistenceService PersistenceService,
        ILogger<Handler> logger
      ) : base(store)
      {
        PersistenceService1 = PersistenceService;
        Logger = logger;
      }
      public override async System.Threading.Tasks.Task Handle(Action action, System.Threading.CancellationToken cancellationToken)
      {
        try
        {
          object? state = await PersistenceService1.LoadState(typeof(PurpleState), PersistentStateMethod.LocalStorage);
          if (state is PurpleState purpleState) Store.SetState(purpleState);
          else Logger.LogTrace("PurpleState is null");
        }
        catch (Exception exception)
        {
          Logger.LogError(exception, "Error loading PurpleState");
          // if this is a JavaScript not available exception then we are prerendering and just swallow it
        }
      }
    }
  }
}
