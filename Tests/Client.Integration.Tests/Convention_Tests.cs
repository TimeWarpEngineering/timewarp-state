namespace ConventionTest_;

using FluentAssertions;

[TestTag(TestTags.Fast)]
public class SimpleNoApplicationTest_Should_
{
  public static void AlwaysPass() => true.Should().BeTrue();

  [Skip("Demonstrates skip attribute")]
  public static void SkipExample() => true.Should().BeFalse();

  [TestTag(TestTags.Fast)]
  public static void TagExample() => true.Should().BeTrue();

  [Input(5, 3, 2)]
  [Input(8, 5, 3)]
  public static void Subtract(int aX, int aY, int aExpectedDifference)
  {
    int result = aX - aY;
    result.Should().Be(aExpectedDifference);
  }
}
