namespace Test.App.Client.Features.CloneTest;

internal partial class CloneTestState
{
  internal class CloneTestHandler
  (
    IStore store
  ) : BaseActionHandler<CloneTestAction>(store)
  {

    protected CloneTestState CloneTestState => Store.GetState<CloneTestState>();

    public override Task<Unit> Handle
    (
      CloneTestAction aCloneTestAction,
      CancellationToken aCancellationToken
    ) => Unit.Task;
  }
}
