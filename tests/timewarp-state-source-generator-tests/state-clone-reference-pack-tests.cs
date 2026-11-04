#region Purpose
// Reference-pack coverage for threading types the implementation assemblies hide.
#endregion

#region Design
// TRUSTED_PLATFORM_ASSEMBLIES puts Thread, CancellationTokenSource, and SemaphoreSlim in one module, so namespace
// symbol equality matches them. The targeting pack splits those types across assemblies. This compilation must
// show that split, and the generated clone must still copy the members by reference with no TWSG002.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Share_Threading_Types_From_The_Reference_Pack
{
  public static void Given_CancellationTokenSource_And_SemaphoreSlim()
  {
    const string Source =
      """
      using System.Threading;
      using TimeWarp.State;

      [GenerateClone]
      public sealed class Holder
      {
        public CancellationTokenSource Tokens { get; set; } = new();
        public SemaphoreSlim Gate { get; set; } = new(1, 1);
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) =
      StateCloneGeneratorTestDriver.RunAgainstReferencePack(Source);

    INamedTypeSymbol thread = outputCompilation.GetTypeByMetadataName("System.Threading.Thread").ShouldNotBeNull();
    INamedTypeSymbol tokens = outputCompilation.GetTypeByMetadataName("System.Threading.CancellationTokenSource").ShouldNotBeNull();
    INamedTypeSymbol gate = outputCompilation.GetTypeByMetadataName("System.Threading.SemaphoreSlim").ShouldNotBeNull();
    SymbolEqualityComparer.Default.Equals(thread.ContainingNamespace, tokens.ContainingNamespace).ShouldBeFalse();
    SymbolEqualityComparer.Default.Equals(thread.ContainingNamespace, gate.ContainingNamespace).ShouldBeFalse();

    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("Name = \"<Tokens>k__BackingField\"");
    source.ShouldContain("Name = \"<Gate>k__BackingField\"");
    source.ShouldNotContain("Clone_System_Threading_CancellationTokenSource");
    source.ShouldNotContain("Clone_System_Threading_SemaphoreSlim");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }
}
