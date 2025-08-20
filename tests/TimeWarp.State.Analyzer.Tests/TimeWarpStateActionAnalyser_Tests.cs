// ReSharper disable InconsistentNaming
namespace TimeWarpStateActionAnalyzer_;

public class Should_Trigger_TW0001
{
  public static async Task Given_InvalidRecord()
  {
    const string TestCode = 
      """
      using TimeWarp.State;

      public record SampleInvalidRecordAction : IAction { }
      """;
    
    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TW0001", DiagnosticSeverity.Error)
      .WithSpan(3, 15, 3, 40) // Assuming the error is at the record declaration
      .WithArguments("SampleInvalidRecordAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };
   
    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
        
    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string TimeWarpMediatorContractsAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpMediatorContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }
  
  public static async Task Given_InvalidClass()
  {
    const string TestCode = 
      """
      using TimeWarp.State;

      public class SampleInvalidClassAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(3, 14, 3, 38)
        .WithArguments("SampleInvalidClassAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    
    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string TimeWarpMediatorContractsAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpMediatorContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidStruct()
  {
    const string TestCode = 
      """
      using TimeWarp.State;

      public struct SampleInvalidStructAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(3, 15, 3, 40)
        .WithArguments("SampleInvalidStructAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    
    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string TimeWarpMediatorContractsAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpMediatorContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidDescendantClass()
  {
    const string TestCode = 
      """
      using TimeWarp.State;
      
      public abstract class AbstractAction: IAction { }
      public class SampleInvalidDescendantClassAction : AbstractAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(4, 14, 4, 48)
        .WithArguments("SampleInvalidDescendantClassAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    
    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string TimeWarpMediatorContractsAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpMediatorContractsAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
