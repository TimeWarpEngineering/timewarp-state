#region Purpose
// PersistenceStateSourceGenerator: nested TWSG001, unique hint names, top-level Load() emit.
#endregion

namespace PersistenceStateSourceGenerator_;

public class Should_Report_TWSG001_For_Nested_PersistentState
{
  public static void Given_Nested_Class_With_PersistentState()
  {
    const string Source =
      """
      namespace App;

      public class Outer
      {
        [PersistentState]
        public class NestedWidgetState
        {
        }
      }
      """;

    GeneratorDriverRunResult runResult = PersistenceGeneratorTestDriver.Run(Source);

    Diagnostic diagnostic = runResult.Diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe(PersistenceStateSourceGenerator.NestedPersistentStateDiagnosticId);
    diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
    diagnostic.GetMessage().ShouldContain("NestedWidgetState");
    runResult.Results[0].GeneratedSources.ShouldBeEmpty();
  }
}

public class Should_Not_Throw_When_Two_Nested_Types_Share_A_Simple_Name
{
  public static void Given_Two_Nested_WidgetState_In_Same_Namespace()
  {
    const string Source =
      """
      namespace App;

      public class OuterA
      {
        [PersistentState]
        public class WidgetState
        {
        }
      }

      public class OuterB
      {
        [PersistentState]
        public class WidgetState
        {
        }
      }
      """;

    GeneratorDriverRunResult runResult = Should.NotThrow(() => PersistenceGeneratorTestDriver.Run(Source));

    runResult.Diagnostics.Select(diagnostic => diagnostic.Id).ShouldAllBe(id => id == PersistenceStateSourceGenerator.NestedPersistentStateDiagnosticId);
    runResult.Diagnostics.Length.ShouldBe(2);
    runResult.Results[0].GeneratedSources.ShouldBeEmpty();
    runResult.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "SG001");
  }
}

public class Should_Emit_Load_For_Top_Level_PersistentState
{
  public static void Given_Top_Level_Class()
  {
    const string Source =
      """
      namespace App;

      [PersistentState]
      public partial class WidgetState
      {
      }
      """;

    GeneratorDriverRunResult runResult = PersistenceGeneratorTestDriver.Run(Source);

    runResult.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == PersistenceStateSourceGenerator.NestedPersistentStateDiagnosticId);
    runResult.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "SG001");
    GeneratedSourceResult generatedSource = runResult.Results[0].GeneratedSources.ShouldHaveSingleItem();
    generatedSource.HintName.ShouldBe("App.WidgetState_Persistence.g.cs");
    generatedSource.SourceText.ToString().ShouldContain("public partial class WidgetState");
    generatedSource.SourceText.ToString().ShouldContain("LoadPersistentStateRequest");
  }
}

public class Should_Use_Containing_Types_In_Hint_Names
{
  public static void Given_Top_Level_And_Nested_Share_Simple_Name()
  {
    const string Source =
      """
      namespace App;

      [PersistentState]
      public partial class WidgetState
      {
      }

      public class Outer
      {
        [PersistentState]
        public class WidgetState
        {
        }
      }
      """;

    GeneratorDriverRunResult runResult = Should.NotThrow(() => PersistenceGeneratorTestDriver.Run(Source));

    runResult.Diagnostics.ShouldHaveSingleItem().Id.ShouldBe(PersistenceStateSourceGenerator.NestedPersistentStateDiagnosticId);
    GeneratedSourceResult generatedSource = runResult.Results[0].GeneratedSources.ShouldHaveSingleItem();
    generatedSource.HintName.ShouldBe("App.WidgetState_Persistence.g.cs");
    generatedSource.SourceText.ToString().ShouldContain("public partial class WidgetState");
  }
}
