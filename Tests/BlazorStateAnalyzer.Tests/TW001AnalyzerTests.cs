namespace BlazorStateAnalyzer.Tests;

public class TW0001AnalyzerTests
{
  public async Task TestThatTW0001IsTriggered()
  {
    const string TestCode = 
      """
      using BlazorState;

      public record SampleInvalidRecordAction : IAction { }
      public class SampleInvalidClassAction : IAction { }
      public struct SampleInvalidStructAction : IAction { }
      public class SampleInvalidDescendantClassAction : AbstractAction { }

      public abstract class AbstractAction: IAction { }

      """;

    DiagnosticResult[] expectedDiagnostics = new[]
    {
      // Replace with the appropriate line and column numbers
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(3, 15, 3, 40),
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(4, 14, 4, 38),
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(5, 15, 5, 40),
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(6, 14, 6, 48)
        .WithArguments("SampleInvalidDescendantClassAction")
    };

    var analyzerTest = new CSharpAnalyzerTest<BlazorStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    foreach (DiagnosticResult expectedDiagnostic in expectedDiagnostics)
    {
      analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    }
    
    const string BlazorStateAssemblyPath = @"Blazor-State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(BlazorStateAssemblyPath));

    const string MediatRContractsAssemblyPath = @"MediatR.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatRContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
