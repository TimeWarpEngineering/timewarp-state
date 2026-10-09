#region Purpose
// Proves EnumerableEqual is false when two sequences differ in length.
#endregion

#region Design
// Calls the test-app helper directly. The interesting case is a shared prefix with one extra element, which a
// pairwise walk stops on without comparing counts.
#endregion

namespace EnumerableEqualTests;

public class Should_
{
  public static void Return_False_When_The_Second_Sequence_Is_Longer()
  {
    int[] shorter = [1, 2];
    int[] longer = [1, 2, 3];

    shorter.EnumerableEqual(longer).ShouldBeFalse();
  }

  public static void Return_False_When_The_First_Sequence_Is_Longer()
  {
    int[] longer = [1, 2, 3];
    int[] shorter = [1, 2];

    longer.EnumerableEqual(shorter).ShouldBeFalse();
  }

  public static void Return_True_When_Sequences_Match()
  {
    int[] left = [1, 2, 3];
    int[] right = [1, 2, 3];

    left.EnumerableEqual(right).ShouldBeTrue();
  }
}
