namespace Test.App.Client.Features.Counter;

using static Test.App.Client.Features.Counter.WrongNesting;

public partial class CounterState
{
  internal class ImproperNestedHandler
  (
    IStore store
  ) : BaseActionHandler<ImproperNestedAction>(store)
  {

    public override Task<Unit> Handle
    (
      ImproperNestedAction aImproperNestedAction,
      CancellationToken aCancellationToken
    ) => Unit.Task;
  }
}
