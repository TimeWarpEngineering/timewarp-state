#region Purpose
// StateCloneSourceGenerator: TWSG002 on an unsupported member, ICloneable is skipped, and a simple state is registered.
#endregion

#region Design
// The driver compiles inline source against TimeWarp.State. Assertions look at diagnostics and the emitted text,
// then compile the output so a broken cloner fails here.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Report_TWSG002
{
  public static void Given_Object_Field()
  {
    const string Source =
      """
      using TimeWarp.State;

      public sealed class BoxState : State<BoxState>
      {
        public object Payload { get; set; } = new();
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    IEnumerable<Diagnostic> diagnostics = runResult.Diagnostics.Concat(outputCompilation.GetDiagnostics());
    diagnostics.Any(diagnostic => diagnostic.Id == StateCloneSourceGenerator.DiagnosticId).ShouldBeTrue();
  }
}

public class Should_Skip_ICloneable
{
  public static void Given_HandWritten_Clone()
  {
    const string Source =
      """
      using System;
      using TimeWarp.State;

      public sealed class HandState : State<HandState>, ICloneable
      {
        public override void Initialize() { }
        public object Clone() => new HandState();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    if (source is not null)
    {
      source.ShouldNotContain("HandState");
    }
  }
}

public class Should_Emit_Registry_And_UnsafeAccessor
{
  public static void Given_Simple_State()
  {
    const string Source =
      """
      using TimeWarp.State;

      public sealed class CountState : State<CountState>
      {
        public int Count { get; set; }
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("UnsafeAccessor");
    source.ShouldContain("StateCloneRegistry.Register<global::CountState>");
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }
}

public class Should_Copy_AutoProperties_From_Metadata_Base
{
  public static void Given_Public_AutoProperty_On_Referenced_Base()
  {
    const string BaseSource =
      """
      using TimeWarp.State;

      namespace Foreign;

      public abstract class Holder<TState> : State<TState>
        where TState : Holder<TState>
      {
        public string? Label { get; set; }
      }
      """;

    const string DerivedSource =
      """
      using Foreign;

      public sealed class BoxState : Holder<BoxState>
      {
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("\"<Label>k__BackingField\"");
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }
}
