namespace Test.App.Client.Features.CloneTest;

[UsedImplicitly]
internal partial class CloneableState
{
  [UsedImplicitly]
  internal class CloneTestHandler
  (
    IStore store
  ) : BaseActionHandler<CloneTestAction>(store)
  {
    private CloneableState CloneableState => Store.GetState<CloneableState>();
    
    public override Task Handle
    (
      CloneTestAction aCloneTestAction,
      CancellationToken aCancellationToken
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
