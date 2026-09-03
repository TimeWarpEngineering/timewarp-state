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
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
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
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
