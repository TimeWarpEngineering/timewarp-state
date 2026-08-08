// ReSharper disable InconsistentNaming
namespace StateReadOnlyPublicPropertiesAnalyzer_;

public class Should_Not_Trigger_StateReadOnlyPublicPropertiesRule
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
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
    };

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"Mediator.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}

public class Should_Trigger_StateReadOnlyPublicPropertiesRule
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

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("StateReadOnlyPublicPropertiesRule", DiagnosticSeverity.Error)
      .WithSpan(5, 14, 5, 28)
      .WithArguments("PublicProperty");

    CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"Mediator.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
