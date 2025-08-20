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
