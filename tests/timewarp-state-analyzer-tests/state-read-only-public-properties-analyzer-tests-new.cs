#region Purpose
// Tests for StateReadOnlyPublicPropertiesAnalyzer (TWS0012): a public writable setter on a TimeWarp.State state is an error.
#endregion

#region Design
// A foreign State<T> with a public setter must not report. init, internal, protected-on-abstract, and positional
// record properties must not report. Uses the pinned Net110 references. An older, commented-out version of these
// tests is in state-read-only-public-properties-analyzer-tests.cs.
#endregion

// ReSharper disable InconsistentNaming
namespace StateReadOnlyPublicPropertiesAnalyzer_;

public class Should_Not_Trigger_TWS0012
{
  public static async Task Given_ForeignState_WithPublicSetter()
  {
    const string TestCode =
      """
      namespace OtherLib
      {
        public abstract class State<T> { }
      }

      public sealed class ForeignState : OtherLib.State<ForeignState>
      {
        public int Value { get; set; }
      }
      """;

    CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    AnalyzerReference.AddReferences(analyzerTest);
    await analyzerTest.RunAsync();
  }
}

public class Should_Trigger_TWS0012
{
  public static async Task Given_TimeWarpState_WithPublicSetter()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed class SampleState : State<SampleState>
      {
        public int PublicProperty { get; set; }

        public override void Initialize() { }
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS0012", DiagnosticSeverity.Error)
      .WithSpan(5, 14, 5, 28)
      .WithArguments("PublicProperty");

    await Run(TestCode, expectedDiagnostic);
  }

  public static async Task Given_TimeWarpState_WithProtectedSetter()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed class SampleState : State<SampleState>
      {
        public int Value { get; protected set; }

        public override void Initialize() { }
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS0012", DiagnosticSeverity.Error)
      .WithSpan(5, 14, 5, 19)
      .WithArguments("Value");

    await Run(TestCode, expectedDiagnostic);
  }

  public static async Task Given_PartialTimeWarpState_ReportsOnce()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed partial class SampleState : State<SampleState>
      {
        public int PublicProperty { get; set; }

        public override void Initialize() { }
      }

      public sealed partial class SampleState
      {
        public int Other => PublicProperty;
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS0012", DiagnosticSeverity.Error)
      .WithSpan(5, 14, 5, 28)
      .WithArguments("PublicProperty");

    await Run(TestCode, expectedDiagnostic);
  }

  private static async Task Run(string testCode, DiagnosticResult expectedDiagnostic)
  {
    CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = testCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    AnalyzerReference.AddReferences(analyzerTest);
    await analyzerTest.RunAsync();
  }
}

public class Should_Allow_Init_Internal_And_Positional_Properties
{
  public static async Task Given_Internal_Setter() =>
    await RunWithoutDiagnostics(
      """
      using TimeWarp.State;

      public sealed class SampleState : State<SampleState>
      {
        public int Value { get; internal set; }

        public override void Initialize() { }
      }
      """);

  public static async Task Given_Init_Setter() =>
    await RunWithoutDiagnostics(
      """
      using TimeWarp.State;

      public sealed class SampleState : State<SampleState>
      {
        public int Value { get; init; }

        public override void Initialize() { }
      }
      """);

  public static async Task Given_Protected_Setter_On_Abstract_State() =>
    await RunWithoutDiagnostics(
      """
      using TimeWarp.State;

      public abstract class SampleState : State<SampleState>
      {
        public int Value { get; protected set; }

        public override void Initialize() { }
      }
      """);

  public static async Task Given_Get_Only_Property() =>
    await RunWithoutDiagnostics(
      """
      using TimeWarp.State;

      public sealed class SampleState : State<SampleState>
      {
        public int Count { get; }

        public SampleState(int count) => Count = count;

        public override void Initialize() { }
      }
      """);

  private static async Task RunWithoutDiagnostics(string testCode)
  {
    CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = testCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    AnalyzerReference.AddReferences(analyzerTest);
    await analyzerTest.RunAsync();
  }
}

file static class AnalyzerReference
{
  public static void AddReferences(CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier> analyzerTest)
  {
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(@"TimeWarp.State.dll"));
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(@"TimeWarp.Mediator.Contracts.dll"));
  }
}
