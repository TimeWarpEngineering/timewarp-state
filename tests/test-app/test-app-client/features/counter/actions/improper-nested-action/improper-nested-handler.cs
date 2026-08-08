#if ANALYZER_TEST
// Code examples that the analyzer should fail on
namespace Test.App.Client.Features.Counter;

using static WrongNesting;

public partial class CounterState
{
  internal class ImproperNestedHandler
  (
    IStore store
  ) : BaseActionHandler<ImproperNestedAction>(store)
  {

    public override ValueTask<Unit> Handle
    (
      ImproperNestedAction action,
      CancellationToken cancellationToken
    ) => new ValueTask<Unit>(Unit.Value);
  }
}
#endif
