// ReSharper disable InconsistentNaming
namespace TimeWarpStateActionAnalyzer_;

public class Should_Trigger_TW0001
{
  public static async Task Given_InvalidRecord()
  {
    const string TestCode =
      """
      using TimeWarp.State;
      using TimeWarp.Mediator;

      public record SampleInvalidRecordAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TW0001", DiagnosticSeverity.Error)
      .WithSpan(4, 15, 4, 40) // Assuming the error is at the record declaration
      .WithArguments("SampleInvalidRecordAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    // Use net10 reference assemblies so the in-memory compilation's System.Runtime matches the
    // net10 TimeWarp.State (and TimeWarp.Mediator) assemblies referenced below (otherwise CS1705).
    analyzerTest.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidClass()
  {
    const string TestCode =
      """
      using TimeWarp.State;
      using TimeWarp.Mediator;

      public class SampleInvalidClassAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(4, 14, 4, 38)
        .WithArguments("SampleInvalidClassAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    // Use net10 reference assemblies so the in-memory compilation's System.Runtime matches the
    // net10 TimeWarp.State (and TimeWarp.Mediator) assemblies referenced below (otherwise CS1705).
    analyzerTest.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidStruct()
  {
    const string TestCode =
      """
      using TimeWarp.State;
      using TimeWarp.Mediator;

      public struct SampleInvalidStructAction : IAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(4, 15, 4, 40)
        .WithArguments("SampleInvalidStructAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    // Use net10 reference assemblies so the in-memory compilation's System.Runtime matches the
    // net10 TimeWarp.State (and TimeWarp.Mediator) assemblies referenced below (otherwise CS1705).
    analyzerTest.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_InvalidDescendantClass()
  {
    const string TestCode =
      """
      using TimeWarp.State;
      using TimeWarp.Mediator;

      public abstract class AbstractAction: IAction { }
      public class SampleInvalidDescendantClassAction : AbstractAction { }
      """;

    DiagnosticResult expectedDiagnostic =
      new DiagnosticResult("TW0001", DiagnosticSeverity.Error).WithSpan(5, 14, 5, 48)
        .WithArguments("SampleInvalidDescendantClassAction");

    var analyzerTest = new CSharpAnalyzerTest<TimeWarpStateActionAnalyzer, FixieVerifier>
    {
      TestCode = TestCode
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    // Use net10 reference assemblies so the in-memory compilation's System.Runtime matches the
    // net10 TimeWarp.State (and TimeWarp.Mediator) assemblies referenced below (otherwise CS1705).
    analyzerTest.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
