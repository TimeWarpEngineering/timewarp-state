#region Purpose
// Short tracked action (a 2 second delay) used alongside the five second task in action tracking tests.
#endregion

#region Design
// [TrackAction] makes it visible in ActionTrackingState while it runs. Callers send the action directly; the
// convenience TwoSecondTask method is left commented out. Start/complete go to ILogger because Console is
// banned (RS0030).
#endregion

namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  public static class TwoSecondTaskActionSet
  {
    [TrackAction]
    public sealed class Action : IAction;

    internal sealed class Handler : StateActionHandler<Action>
    {
      private readonly ILogger<Handler> Logger;

      public Handler(IStore store, ILogger<Handler> logger) : base(store)
      {
        Logger = logger;
      }

      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        Logger.LogInformation("Start two second task");
        await Task.Delay(millisecondsDelay: 2000, cancellationToken: cancellationToken);
        Logger.LogInformation("Two second task complete");
      }
    }
  }
  
  // public async Task TwoSecondTask(CancellationToken? externalCancellationToken = null)
  // {
  //   using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
  //     ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
  //     : null;
  //
  //   await Sender.Send
  //   (
  //     new TwoSecondTaskActionSet.Action(),
  //     linkedCts?.Token ?? CancellationToken
  //   );
  // }
}
