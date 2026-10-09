#region Purpose
// Sample tests that exercise the TimeWarp.Fixie convention itself (pass, skip, tag, parameterized input).
#endregion

#region Design
// Copied from the TimeWarp.Fixie template: [Skip] shows up as one skipped test per run, [TestTag(Fast)] and [Input]
// show tagging and parameterization. They do not test TimeWarp.State.
#endregion

namespace ConventionTest_;

[TestTag(TestTags.Fast)]
public class SimpleNoApplicationTest_Should_
{
  public static void AlwaysPass() => true.ShouldBeTrue();

  [Skip("Demonstrates skip attribute")]
  public static void SkipExample() => true.ShouldBeFalse();

  [TestTag(TestTags.Fast)]
  public static void TagExample() => true.ShouldBeTrue();

  [Input(5, 3, 2)]
  [Input(8, 5, 3)]
  public static void Subtract(int x, int y, int expectedDifference)
  {
    int result = x - y;
    result.ShouldBe(expectedDifference);
  }
}
