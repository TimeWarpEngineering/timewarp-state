#if ANALYZER_TEST
// Code examples that the analyzer should fail on
namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  internal class NonNestedHandler
  (
    IStore store
  ) : BaseActionHandler<NonNestedAction>(store)
  {

    public override ValueTask Handle
    (
      NonNestedAction action,
      CancellationToken cancellationToken
    ) => default;
  }
}
#endif
