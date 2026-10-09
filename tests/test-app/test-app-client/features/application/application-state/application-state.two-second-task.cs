#region Purpose
// Short tracked action (a 2 second delay) used alongside the five second task in action tracking tests.
#endregion

#region Design
// [TrackAction] makes it visible in ActionTrackingState while it runs. Callers send the action directly; the
// convenience TwoSecondTask method is left commented out.
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
      public Handler(IStore store) : base(store) {}
      public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start two Second Task");
        await Task.Delay(millisecondsDelay: 2000, cancellationToken: cancellationToken);
        Console.WriteLine("Two Second Task Complete");
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
