namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  internal class NonNestedHandler
  (
    IStore store
  ) : BaseActionHandler<NonNestedAction>(store)
  {

    public override Task<Unit> Handle
    (
      NonNestedAction aNonNestedAction,
      CancellationToken aCancellationToken
    ) => Unit.Task;
  }
}
