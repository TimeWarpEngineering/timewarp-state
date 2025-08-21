namespace Test.App.Client.Features.CloneTest;

public partial class CloneableState
{
  public static class CloneTestActionSet
  {
    internal sealed class Action : IAction;
    internal sealed class Handler : BaseActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      private CloneableState CloneableState => Store.GetState<CloneableState>();
    
      public override Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        // Note: This is a test to verify that the state is cloned.
        // It is not an example of any real-world usage.
        if ( CloneableState.Count != 42) throw new Exception("Count is not 42 it seems I have failed to clone the state");
        CloneableState.Count++;
        return Task.CompletedTask;
      }
    }
  }
}
