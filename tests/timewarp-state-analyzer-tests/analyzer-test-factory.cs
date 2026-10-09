#region Purpose
// Shared in-memory compilation setup for TimeWarp.State analyzer tests.
#endregion

namespace TimeWarp.State.Analyzer.Tests;

using Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerTestFactory
{
  // Microsoft.CodeAnalysis.Analyzer.Testing 1.1.4 has no ReferenceAssemblies.Net.Net110 yet, so pin the
  // net11 targeting pack explicitly. Keep the version in step with the Microsoft.* 11.0.x entries in
  // Directory.Packages.props (re-pin to 11.0.0 at GA).
  internal static readonly ReferenceAssemblies Net110 = new
  (
    targetFramework: "net11.0",
    referenceAssemblyPackage: new PackageIdentity("Microsoft.NETCore.App.Ref", "11.0.0-rc.1.26425.128"),
    referenceAssemblyPath: System.IO.Path.Combine("ref", "net11.0")
  );

  public static CSharpAnalyzerTest<TAnalyzer, FixieVerifier> Create<TAnalyzer>(string testCode)
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    CSharpAnalyzerTest<TAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = testCode,
      ReferenceAssemblies = Net110
    };

    AddLibraryReferences(analyzerTest);
    return analyzerTest;
  }

  public static void AddLibraryReferences<TAnalyzer>(CSharpAnalyzerTest<TAnalyzer, FixieVerifier> analyzerTest)
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    // Use net11 reference assemblies so the in-memory compilation's System.Runtime matches the
    // net11 TimeWarp.State (and TimeWarp.Mediator) assemblies referenced below (otherwise CS1705).
    analyzerTest.ReferenceAssemblies = Net110;

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));
  }
}
