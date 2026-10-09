#region Purpose
// Negative analyzer example: a handler for ImproperNestedAction.
#endregion

#region Design
// Compiled only when ANALYZER_TEST is defined, so normal builds skip it; pairs with improper-nested-action.cs.
#endregion

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

    public override ValueTask Handle
    (
      ImproperNestedAction action,
      CancellationToken cancellationToken
    ) => default;
  }
}
#endif
