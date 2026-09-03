// ReSharper disable InconsistentNaming
namespace StateInheritanceAnalyzer_;

public class Should_Not_Trigger_StateInheritanceRules
{
  public static async Task Given_ForeignState_WithWrongTypeArg()
  {
    const string TestCode =
      """
      namespace OtherLib
      {
        public abstract class State<T> { }
      }

      public class OtherForeignState : OtherLib.State<OtherForeignState>
      {
      }

      public class ForeignState : OtherLib.State<OtherForeignState>
      {
      }
      """;

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
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

public class Should_Trigger_StateInheritanceTypeArgumentRule
{
  public static async Task Given_TimeWarpState_WithWrongTypeArg()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed class OtherState : State<OtherState>
      {
        public override void Initialize() { }
      }

      public sealed class WrongState : State<OtherState>
      {
        public override void Initialize() { }
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("StateInheritanceTypeArgumentRule", DiagnosticSeverity.Error)
      .WithSpan(8, 21, 8, 31);

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
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
