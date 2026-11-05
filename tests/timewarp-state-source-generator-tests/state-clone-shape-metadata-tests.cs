#region Purpose
// Metadata and reference-assembly handling: a type from another assembly is accepted or rejected by field visibility.
#endregion

#region Design
// Each case compiles a base assembly, emits it as an implementation or reference assembly image, and runs the generator
// against a second compilation that references it. Private fields are invisible in a reference assembly, so a type with
// only a computed property is rejected there with a ProduceReferenceAssembly hint (except for framework assemblies).
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Check_Metadata_Types
{
  public static void Given_Implementation_Base_With_Private_Field()
  {
    const string BaseSource =
      """
      namespace Foreign;
      public class Counter
      {
        private int Hidden;
        public void Bump() => Hidden++;
        public int Visible { get; set; }
      }
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public Foreign.Counter Counter { get; set; } = new(); }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.GetMessage().ShouldContain("private field 'Hidden'");
    error.Location.IsInSource.ShouldBeTrue();
  }

  public static void Given_Implementation_Type_With_Only_AutoProperties()
  {
    const string BaseSource =
      """
      namespace Foreign;
      public sealed class Dto
      {
        public string? Name { get; set; }
        public int Count { get; init; }
        public int Doubled => Count * 2;
      }
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public Foreign.Dto Dto { get; set; } = new(); }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource);
    runResult.Diagnostics.ShouldBeEmpty();
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }

  public static void Given_Reference_Assembly_Record()
  {
    const string BaseSource =
      """
      namespace Foreign;
      public sealed record Dto(string Name, int Count);
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public Foreign.Dto? Dto { get; set; } }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource, referenceAssembly: true);
    runResult.Diagnostics.ShouldBeEmpty();
  }

  public static void Given_Reference_Assembly_Type_With_Computed_Property()
  {
    const string BaseSource =
      """
      namespace Foreign;
      public sealed class Dto
      {
        private readonly System.Collections.Generic.List<int> Items = new();
        public int Count => Items.Count;
      }
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public Foreign.Dto? Dto { get; set; } }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource, referenceAssembly: true);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.GetMessage().ShouldContain("reference assembly");
    error.GetMessage().ShouldContain("ProduceReferenceAssembly=false");
  }

  public static void Given_Framework_Reference_Assembly_Omits_ProduceReferenceAssembly_Hint()
  {
    const string BaseSource =
      """
      namespace Foreign;
      public sealed class Dto
      {
        private readonly System.Collections.Generic.List<int> Items = new();
        public int Count => Items.Count;
      }
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public Foreign.Dto? Dto { get; set; } }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource, referenceAssembly: true, baseAssemblyName: "System.Foreign");
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.GetMessage().ShouldContain("reference assembly 'System.Foreign'");
    error.GetMessage().ShouldNotContain("ProduceReferenceAssembly");
  }

  private const string HierarchySource =
    """
    namespace Foreign;
    public abstract class Shape { public string Name { get; set; } = ""; }
    public class Circle : Shape { public double Radius { get; set; } }
    public class Animal { public string Name { get; set; } = ""; }
    public class Dog : Animal { public System.Collections.Generic.List<string> Tricks { get; set; } = new(); }
    """;

  private const string HierarchyHolder =
    """
    using TimeWarp.State;
    [GenerateClone] public class Holder { public Foreign.Shape? S { get; set; } public Foreign.Animal? A { get; set; } }
    """;

  [Input(false)]
  [Input(true)]
  public static void Given_Metadata_Hierarchy_Dispatches_To_Its_Subtypes(bool referenceAssembly)
  {
    (GeneratorDriverRunResult runResult, Compilation outputCompilation) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(HierarchySource, HierarchyHolder, referenceAssembly);
    runResult.Diagnostics.Select(diagnostic => diagnostic.ToString()).ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("case global::Foreign.Circle ");
    source.ShouldContain("case global::Foreign.Dog ");
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }

  public static void Given_Metadata_Base_With_Internal_Subtype()
  {
    const string BaseSource =
      """
      namespace Foreign;
      public class Animal { public string Name { get; set; } = ""; }
      internal sealed class Cat : Animal { }
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public Foreign.Animal? A { get; set; } }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.Location.IsInSource.ShouldBeTrue();
    error.GetMessage().ShouldContain("subtype 'Foreign.Cat'");
  }
}
