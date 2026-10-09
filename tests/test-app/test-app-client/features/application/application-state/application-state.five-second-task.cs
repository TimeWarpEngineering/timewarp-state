#region Purpose
// Long-running tracked action (a 5 second delay) used to test action tracking and cancellation.
#endregion

#region Design
// [TrackAction] puts it in ActionTrackingState while it runs. FiveSecondTask links an optional external token with the
// state's CancellationToken so tests and ActiveActionsPage can cancel it either way.
#endregion

namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  public static class FiveSecondTaskActionSet
  {
    [TrackAction]
    public sealed record Action : IAction;

    internal sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}

      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start five second task");
        await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);
        Console.WriteLine("Five second task complete");
      }
    }
  }

  public async Task FiveSecondTask(CancellationToken? externalCancellationToken = null)
  {
    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
      ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
      : null;

    await Sender.Send
    (
      new FiveSecondTaskActionSet.Action(),
      linkedCts?.Token ?? CancellationToken
    );
  }
}
