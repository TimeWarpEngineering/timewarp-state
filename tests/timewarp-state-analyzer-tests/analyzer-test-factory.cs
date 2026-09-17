#region Purpose
// Shared in-memory compilation setup for TimeWarp.State analyzer tests.
#endregion

namespace TimeWarp.State.Analyzer.Tests;

using Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerTestFactory
{
  public static CSharpAnalyzerTest<TAnalyzer, FixieVerifier> Create<TAnalyzer>(string testCode)
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    CSharpAnalyzerTest<TAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = testCode,
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
    };

    AddLibraryReferences(analyzerTest);
    return analyzerTest;
  }

  public static void AddLibraryReferences<TAnalyzer>(CSharpAnalyzerTest<TAnalyzer, FixieVerifier> analyzerTest)
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    // Use net10 reference assemblies so the in-memory compilation's System.Runtime matches the
    // net10 TimeWarp.State (and TimeWarp.Mediator) assemblies referenced below (otherwise CS1705).
    analyzerTest.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));
  }
}
