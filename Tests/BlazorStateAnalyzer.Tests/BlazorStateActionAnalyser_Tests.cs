// ReSharper disable InconsistentNaming
namespace BlazorStateActionAnalyzer_;

public class Should_Trigger_TW0001
{
  public static async Task Given_InvalidRecord()
  {
    const string TestCode = 
      """
      using BlazorState;

      public record SampleInvalidRecordAction : IAction { }
      """;
    
    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TW0001", DiagnosticSeverity.Error)
      .WithSpan(3, 15, 3, 40) // Assuming the error is at the record declaration
      .WithArguments("SampleInvalidRecordAction");

    var analyzerTest = new CSharpAnalyzerTest<BlazorStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };
   
    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
        
    const string BlazorStateAssemblyPath = @"Blazor-State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(BlazorStateAssemblyPath));

    const string MediatRContractsAssemblyPath = @"MediatR.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatRContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }
  
  public static async Task Given_InvalidClass()
  {
    const string TestCode = 
      """
      using BlazorState;

      public class SampleInvalidClassAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(3, 14, 3, 38)
        .WithArguments("SampleInvalidClassAction");

    var analyzerTest = new CSharpAnalyzerTest<BlazorStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    
    const string BlazorStateAssemblyPath = @"Blazor-State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(BlazorStateAssemblyPath));

    const string MediatRContractsAssemblyPath = @"MediatR.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatRContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidStruct()
  {
    const string TestCode = 
      """
      using BlazorState;

      public struct SampleInvalidStructAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(3, 15, 3, 40)
        .WithArguments("SampleInvalidStructAction");

    var analyzerTest = new CSharpAnalyzerTest<BlazorStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    
    const string BlazorStateAssemblyPath = @"Blazor-State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(BlazorStateAssemblyPath));

    const string MediatRContractsAssemblyPath = @"MediatR.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatRContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidDescendantClass()
  {
    const string TestCode = 
      """
      using BlazorState;
      
      public abstract class AbstractAction: IAction { }
      public class SampleInvalidDescendantClassAction : AbstractAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(4, 14, 4, 48)
        .WithArguments("SampleInvalidDescendantClassAction");

    var analyzerTest = new CSharpAnalyzerTest<BlazorStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    
    const string BlazorStateAssemblyPath = @"Blazor-State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(BlazorStateAssemblyPath));

    const string MediatRContractsAssemblyPath = @"MediatR.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatRContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
