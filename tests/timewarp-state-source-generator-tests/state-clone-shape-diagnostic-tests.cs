#region Purpose
// Unsupported member shapes are TWSG002 at a source location, and metadata types are accepted or rejected by field visibility.
#endregion

#region Design
// Each [Input] names one entry in a shape table. Metadata cases build a second compilation for the referenced assembly.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Report_TWSG002_At_Source
{
  private static readonly Dictionary<string, string> Shapes = new()
  {
    ["ObjectMember"] =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public object Payload { get; set; } = new(); }
      """,
    ["PointerMember"] =
      """
      using TimeWarp.State;
      [GenerateClone] public unsafe class Holder { public int* Pointer; }
      """,
    ["OpenGenericState"] =
      """
      using TimeWarp.State;
      public class OpenState<T> : State<OpenState<T>> { public override void Initialize() { } }
      """,
    ["PrivateNestedState"] =
      """
      using TimeWarp.State;
      public class Outer { private sealed class InnerState : State<InnerState> { public override void Initialize() { } } }
      """,
    ["BclClassWithPrivateState"] =
      """
      using TimeWarp.State;
      [GenerateClone] public class Holder { public System.IO.MemoryStream Stream { get; set; } = new(); }
      """,
    ["InterfaceWithoutImplementation"] =
      """
      using TimeWarp.State;
      public interface IThing { }
      [GenerateClone] public class Holder { public IThing? Thing { get; set; } }
      """,
    ["ServiceWithoutCloneShared"] =
      """
      using Microsoft.Extensions.Logging;
      using TimeWarp.State;
      public sealed class ServiceState : State<ServiceState>
      {
        public ILogger<ServiceState> Logger { get; }
        public ServiceState(ILogger<ServiceState>? logger) { Logger = logger!; }
        public override void Initialize() { }
      }
      """,
    ["UncloneableDerivedType"] =
      """
      using TimeWarp.State;
      public class Shape { }
      public class Blob : Shape { public object Payload { get; set; } = new(); }
      [GenerateClone] public class Holder { public Shape? S { get; set; } }
      """,
    ["PrivateNestedSubclass"] =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      [GenerateClone] public class Holder
      {
        public Base? B { get; set; }
        private sealed class PrivBase : Base { public int W { get; set; } }
      }
      """,
    ["ProtectedNestedSubclass"] =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      public class Outer { protected class ProtBase : Base { } }
      [GenerateClone] public class Holder { public Base? B { get; set; } }
      """,
    ["PrivateImplementationBesidePublicOne"] =
      """
      using TimeWarp.State;
      public interface IItem { int V { get; } }
      public sealed class PublicItem : IItem { public int V { get; set; } }
      public class Outer { private sealed class PrivItem : IItem { public int V { get; set; } } }
      [GenerateClone] public class Holder { public IItem? I { get; set; } }
      """,
    ["GenericSubclass"] =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      public class Gen<T> : Base { public T? Value { get; set; } }
      [GenerateClone] public class Holder { public Base? B { get; set; } }
      """,
    ["GenericSubclassWithUndeterminedParameter"] =
      """
      using TimeWarp.State;
      public class GB<T> { public T? V { get; set; } }
      public class GX<T, U> : GB<T> { public U? W { get; set; } }
      [GenerateClone] public class Holder { public GB<int>? G { get; set; } }
      """,
    ["GenericSubclassWithFailingConstraint"] =
      """
      using TimeWarp.State;
      public abstract class AB<T> { public T? V { get; set; } }
      public class AInt : AB<int> { }
      public class ARef<T> : AB<T> where T : class { }
      [GenerateClone] public class Holder { public AB<int>? A { get; set; } }
      """,
    ["GenericImplementationWithUndeterminedParameter"] =
      """
      using TimeWarp.State;
      public interface IR<T> { T V { get; } }
      public class IntR : IR<int> { public int V { get; set; } }
      public class GR<T, U> : IR<T> { public T V { get; set; } = default!; public U? W { get; set; } }
      [GenerateClone] public class Holder { public IR<int>? R { get; set; } }
      """,
    ["FileLocalSubclass"] =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      file sealed class Local : Base { }
      [GenerateClone] public class Holder { public Base? B { get; set; } }
      """,
    ["FileLocalGenerateClone"] =
      """
      using TimeWarp.State;
      [GenerateClone] file class Holder { public int V { get; set; } }
      """,
    ["FileLocalState"] =
      """
      using TimeWarp.State;
      file sealed class LocalState : State<LocalState> { public override void Initialize() { } }
      """
  };

  [Input("ObjectMember")]
  [Input("PointerMember")]
  [Input("OpenGenericState")]
  [Input("PrivateNestedState")]
  [Input("BclClassWithPrivateState")]
  [Input("InterfaceWithoutImplementation")]
  [Input("ServiceWithoutCloneShared")]
  [Input("UncloneableDerivedType")]
  [Input("PrivateNestedSubclass")]
  [Input("ProtectedNestedSubclass")]
  [Input("PrivateImplementationBesidePublicOne")]
  [Input("GenericSubclass")]
  [Input("GenericSubclassWithUndeterminedParameter")]
  [Input("GenericSubclassWithFailingConstraint")]
  [Input("GenericImplementationWithUndeterminedParameter")]
  [Input("FileLocalSubclass")]
  [Input("FileLocalGenerateClone")]
  [Input("FileLocalState")]
  public static void Given_Shape(string shape)
  {
    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Shapes[shape], allowUnsafe: true);
    List<Diagnostic> errors = runResult.Diagnostics.Where(diagnostic => diagnostic.Id == StateCloneSourceGenerator.DiagnosticId).ToList();
    errors.ShouldNotBeEmpty();
    errors.ShouldAllBe(diagnostic => diagnostic.Location.IsInSource);
    // The generated file must not add compiler errors (for example CS0400 for a file-local type) on top of TWSG002.
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.Ordinal) == true)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }

  public static void Given_Hidden_Subtype_Names_It_At_The_Member()
  {
    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Shapes["PrivateNestedSubclass"]);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.Id.ShouldBe(StateCloneSourceGenerator.DiagnosticId);
    error.GetMessage().ShouldContain("member 'B'");
    error.GetMessage().ShouldContain("subtype 'Holder.PrivBase'");
  }

  public static void Given_Nested_Failure_Reports_Once_At_Member_And_Names_Root()
  {
    const string Source =
      """
      using System.Collections.Generic;
      using TimeWarp.State;

      public class Inner { public object Payload { get; set; } = new(); }

      public sealed class BoxState : State<BoxState>
      {
        public List<Inner> Items { get; set; } = new();
        public override void Initialize() { }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Source);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.Id.ShouldBe(StateCloneSourceGenerator.DiagnosticId);
    error.Location.IsInSource.ShouldBeTrue();
    string message = error.GetMessage();
    message.ShouldContain("'Inner'");
    message.ShouldContain("'Payload'");
    message.ShouldContain("reached from 'BoxState'");
  }
}

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
