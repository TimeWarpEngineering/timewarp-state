#region Purpose
// Negative analyzer example: an action declared at namespace level instead of inside a state.
#endregion

#region Design
// Compiled only when ANALYZER_TEST is defined, so normal builds skip it; the TimeWarp.State analyzer should report it.
#endregion

#if ANALYZER_TEST
// Code examples that the analyzer should fail on
namespace Test.App.Client.Features.Counter;

public class NonNestedAction : IAction
{
  // ReSharper disable once UnusedMember.Global
  public int Amount { get; set; }
}
#endif
