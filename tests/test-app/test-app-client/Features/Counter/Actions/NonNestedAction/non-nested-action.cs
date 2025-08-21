#if ANALYZER_TEST
// Code examples that the analyzer should fail on
namespace Test.App.Client.Features.Counter;

public class NonNestedAction : IAction
{
  // ReSharper disable once UnusedMember.Global
  public int Amount { get; set; }
}
#endif
