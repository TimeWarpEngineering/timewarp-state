namespace Test.App.Client.Features.CloneTest;

[UsedImplicitly]
internal partial class CloneTestState
{
  [UsedImplicitly]
  internal class CloneTestHandler
  (
    IStore store
  ) : BaseActionHandler<CloneTestAction>(store)
  {
    public override Task<Unit> Handle
    (
      CloneTestAction aCloneTestAction,
      CancellationToken aCancellationToken
    ) => Unit.Task;
  }
}
