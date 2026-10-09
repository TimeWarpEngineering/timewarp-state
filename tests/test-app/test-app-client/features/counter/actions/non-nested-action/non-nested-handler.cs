#region Purpose
// Negative analyzer example: a handler for NonNestedAction.
#endregion

#region Design
// Compiled only when ANALYZER_TEST is defined, so normal builds skip it; pairs with non-nested-action.cs.
#endregion

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
