#region Purpose
// Tests for StateImplementationAnalyzer (TWS001): a TimeWarp.State state needs Clone or a parameterless constructor.
#endregion

#region Design
// Includes a foreign State<T> from another namespace to prove the rule only applies to TimeWarp.State. Each test
// builds its own CSharpAnalyzerTest with the pinned Net110 references.
#endregion

// ReSharper disable InconsistentNaming
namespace StateImplementationAnalyzer_;

public class Should_Not_Trigger_TWS001
{
  public static async Task Given_ForeignState_WithNoCloneOrCtor()
  {
    const string TestCode =
      """
      namespace OtherLib
      {
        public abstract class State<T> { }
      }

      public sealed class ForeignState : OtherLib.State<ForeignState>
      {
        public ForeignState(int value) { }
      }
      """;

    CSharpAnalyzerTest<StateImplementationAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}

public class Should_Trigger_TWS001
{
  public static async Task Given_TimeWarpState_WithoutCloneOrParameterlessCtor()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public class BadState : State<BadState>
      {
        public BadState(int value) { }

        public override void Initialize() { }
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS001", DiagnosticSeverity.Error)
      .WithSpan(3, 14, 3, 22)
      .WithArguments("BadState");

    CSharpAnalyzerTest<StateImplementationAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
