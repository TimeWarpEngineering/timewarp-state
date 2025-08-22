#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Purple;

using TimeWarp.Features.Persistence;
using TimeWarp.State;

public partial class PurpleState
{
  internal sealed class StateLoadedNotification : INotification
  {
    public string TypeName { get; }

    public StateLoadedNotification(string typeName)
    {
      TypeName = typeName;
    }
  }

  internal static class LoadActionSet
  {
    internal sealed class Action : IAction;

    internal sealed class Handler : ActionHandler<Action>
    {
      private readonly IPersistenceService PersistenceService;
      private readonly ILogger<Handler> Logger;
      private readonly IPublisher Publisher;
      
      public Handler
      (
        IStore store,
        IPersistenceService persistenceService,
        ILogger<Handler> logger,
        IPublisher publisher
      ) : base(store)
      {
        PersistenceService = persistenceService;
        Logger = logger;
        Publisher = publisher;
      }
      
      public override async System.Threading.Tasks.Task Handle(Action action, System.Threading.CancellationToken cancellationToken)
      {
        try
        {
            object? state = await PersistenceService.LoadState(typeof(PurpleState), PersistentStateMethod.LocalStorage);
            if (state is PurpleState purpleState)
            {
              Store.SetState(purpleState);
              Logger.LogTrace("PurpleState loaded successfully");
            }
            else
            {
              Logger.LogTrace("PurpleState is null");
            }
            
            // Send notification regardless of whether state was found or not
            await Publisher.Publish(new StateLoadedNotification(typeof(PurpleState).FullName!), cancellationToken);
        }
        catch (Exception exception)
        {
          Logger.LogError(exception, "Error loading PurpleState");
          // if this is a JavaScript not available exception then we are prerendering and just swallow it
          
          // Send notification even if an error occurred
          await Publisher.Publish(new StateLoadedNotification(typeof(PurpleState).FullName!), cancellationToken);
        }
      }
    }
  }
  public async Task Load(CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;
  
    await Sender.Send
    (
      new LoadActionSet.Action(),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
#pragma warning restore CS1591
