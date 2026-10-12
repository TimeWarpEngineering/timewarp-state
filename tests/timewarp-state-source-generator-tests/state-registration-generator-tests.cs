#region Purpose
// State registration emitted beside the clone registry: nested states, generic bases, ICloneable closures, and skips.
#endregion

#region Design
// The driver compiles inline source. Assertions read the generated text and require the output compilation to be
// free of errors. Open and file-local states must not become TryAddTransient calls.
#endregion

namespace StateRegistrationGenerator_;

public class Should_Register_Nested_State
{
  public static void Given_Public_Nested_State()
  {
    const string Source =
      """
      using TimeWarp.State;

      public static class Outer
      {
        public sealed class InnerState : State<InnerState>
        {
          public int Count { get; set; }
          public override void Initialize() { }
        }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("TryAddTransient<global::Outer.InnerState>");
    GeneratedOutput.Errors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Register_Concrete_Child_Of_Generic_Base
{
  public static void Given_HistState_Shape()
  {
    const string Source =
      """
      using TimeWarp.State;

      public abstract class Holder<TState> : State<TState>
        where TState : Holder<TState>
      {
      }

      public sealed class ChildState : Holder<ChildState>
      {
        public int Count { get; set; }
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("TryAddTransient<global::ChildState>");
    source.ShouldNotContain("TryAddTransient<global::Holder");
    GeneratedOutput.Errors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Register_Closed_ICloneable_Generic
{
  public static void Given_Box_Of_Int()
  {
    const string Source =
      """
      using System;
      using TimeWarp.State;

      public sealed class Box<T> : State<Box<T>>, ICloneable
      {
        public override void Initialize() { }
        public object Clone() => new Box<T>();
      }

      public sealed class Mention
      {
        public Box<int>? Value { get; set; }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("TryAddTransient<global::Box<int>>");
    source.ShouldContain("RootStateForTrim<global::Box<int>>");
    source.ShouldNotContain("UnsafeAccessor");
    source.ShouldNotContain("TryAddTransient<global::Box<T>>");
    GeneratedOutput.Errors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Not_Register_GenerateClone_NonState
{
  public static void Given_Payload_Beside_CountState()
  {
    const string Source =
      """
      using TimeWarp.State;

      [GenerateClone]
      public sealed class Payload
      {
        public int Value { get; set; }
      }

      public sealed class CountState : State<CountState>
      {
        public int Count { get; set; }
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("TryAddTransient<global::CountState>");
    source.ShouldNotContain("TryAddTransient<global::Payload>");
    GeneratedOutput.Errors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Not_Register_Open_Or_FileLocal_State
{
  public static void Given_Open_Generic_State()
  {
    const string Source =
      """
      using TimeWarp.State;

      public class OpenState<T> : State<OpenState<T>>
      {
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.Any(diagnostic => diagnostic.Id == StateCloneSourceGenerator.DiagnosticId).ShouldBeTrue();
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    if (source is not null)
    {
      source.ShouldNotContain("TryAddTransient");
    }

    GeneratedOutput.Errors(outputCompilation).ShouldBeEmpty();
  }

  public static void Given_FileLocal_State()
  {
    const string Source =
      """
      using TimeWarp.State;

      file sealed class LocalState : State<LocalState>
      {
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.Any(diagnostic => diagnostic.Id == StateCloneSourceGenerator.DiagnosticId).ShouldBeTrue();
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    if (source is not null)
    {
      source.ShouldNotContain("TryAddTransient");
    }

    GeneratedOutput.Errors(outputCompilation).ShouldBeEmpty();
  }
}

internal static class GeneratedOutput
{
  public static IEnumerable<string> Errors(Compilation outputCompilation) =>
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.Ordinal) == true)
      .Select(diagnostic => diagnostic.ToString());
}
