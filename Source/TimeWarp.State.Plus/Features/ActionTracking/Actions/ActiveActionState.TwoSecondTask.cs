namespace TimeWarp.Features.Processing;

public partial class ActiveActionState
{
  public static class TwoSecondTask
  {
    [TrackAction]
    public record Action : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ActiveActionState ActiveActionState => Store.GetState<ActiveActionState>();

      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start two Second Task");
        await Task.Delay(millisecondsDelay: 2000, cancellationToken: cancellationToken);
        Console.WriteLine("Two Second Task Complete");
      }
    }
  }
}
