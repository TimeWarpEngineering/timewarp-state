namespace TimeWarp.Features.Processing;

public partial class ProcessingState
{
  public static class FiveSecondTask
  {
    [TrackProcessing]
    public record Action : IAction;

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ProcessingState ProcessingState => Store.GetState<ProcessingState>();

      public override async Task Handle(Action action, CancellationToken cancellationToken)
      {
        Console.WriteLine("Start five second task");
        await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);
        Console.WriteLine("Five second task complete");
      }
    }
  }
}
