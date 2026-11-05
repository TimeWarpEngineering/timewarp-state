#region Purpose
// Degenerate attribute use: [GenerateClone] on a type with no members, and [CloneShared] on a member that is not a field or property.
#endregion

#region Design
// These pin what the generator does today, not what it ought to do. [CloneShared] is declared for fields and
// properties only, so a method is a compiler error (CS0592) before the generator matters; the generator must still not
// add TWSG002 on top of it or throw.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Handle_Degenerate_Attribute_Use
{
  public static void Given_GenerateClone_On_Empty_Type_Emits_A_Clone_That_Copies_Nothing()
  {
    const string Source =
      """
      using TimeWarp.State;
      [GenerateClone] public class Empty { }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("global::Empty clone = new();");
    source.ShouldNotContain("F_");
    StateCloneShapeSources.ErrorsOf(outputCompilation).ShouldBeEmpty();
  }

  public static void Given_CloneShared_On_Method_Is_Only_A_CS0592_Error_And_The_Method_Is_Ignored()
  {
    const string Source =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder
      {
        public int Count { get; set; }
        [CloneShared] public void Refresh() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    StateCloneShapeSources.ErrorsOf(outputCompilation).ShouldHaveSingleItem().ShouldContain("CS0592");
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("F_Count_");
    source.ShouldNotContain("Refresh");
  }
}
