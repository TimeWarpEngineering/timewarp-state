#region Purpose
// Table-driven StateCloneSourceGenerator coverage: every supported member shape compiles with no generator diagnostic
// and no compiler error, and every unsupported shape is TWSG002 at a source location.
#endregion

#region Design
// Each [Input] names one entry in a shape table, so a failure reports the shape. Supported shapes compile the
// generated file together with the source (CS0121, CS9035, CS1061, CS0266, CS8121, CS0111 all surface here).
// Runtime behavior of the same shapes is covered in timewarp-state-tests/cloning/generated-clone-shape-tests.cs.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Compile_Supported_Shape
{
  private const string Item = "public class Item { public int V { get; set; } }";

  private static readonly Dictionary<string, string> Shapes = new()
  {
    ["GenericWrapper"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Wrapper<T> { public T? Value { get; set; } }
      [GenerateClone] public class UsesWrapper { public Wrapper<Item> W { get; set; } = new(); }
      """,
    ["GenericStateBase"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public abstract class ListState<TState> : State<TState> where TState : ListState<TState>
      {
        public List<TState> History { get; set; } = new();
        public TState? Previous { get; set; }
      }
      public sealed class HistState : ListState<HistState>
      {
        public int Count { get; set; }
        public override void Initialize() { }
      }
      """,
    ["NestedGeneric"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      public class Outer<T> { public class Inner { public T? Value; public System.Collections.Generic.List<T> Values = new(); } }
      [GenerateClone] public class UsesNested { public Outer<Item>.Inner? I { get; set; } }
      """,
    ["Tuple"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class TupleHolder
      {
        public (string Name, Item It) T { get; set; }
        public (int, int, int, int, int, int, int, Item) Long { get; set; }
      }
      """,
    ["KeyValuePair"] =
      $$"""
      using System.Collections.Generic;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class KvpHolder { public List<KeyValuePair<string, Item>> Pairs { get; set; } = new(); }
      """,
    ["NullableStruct"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public struct Box { public List<int> Values; }
      [GenerateClone] public class NullableHolder { public Box? B { get; set; } }
      """,
    ["CollectionInterfaces"] =
      $$"""
      using System.Collections.Generic;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Interfaces
      {
        public IList<string> A { get; set; } = new List<string>();
        public IReadOnlyList<string> B { get; set; } = new List<string>();
        public ISet<int> C { get; set; } = new HashSet<int>();
        public IReadOnlySet<int> D { get; set; } = new HashSet<int>();
        public IEnumerable<Item> E { get; set; } = new List<Item>();
        public ICollection<int> F { get; set; } = new List<int>();
        public IReadOnlyCollection<Item> G { get; set; } = new List<Item>();
        public IDictionary<string, Item> H { get; set; } = new Dictionary<string, Item>();
        public IReadOnlyDictionary<string, int> I { get; set; } = new Dictionary<string, int>();
      }
      """,
    ["ImmutableAndFrozen"] =
      $$"""
      using System.Collections.Frozen;
      using System.Collections.Immutable;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Immutables
      {
        public ImmutableList<Item> A { get; set; } = ImmutableList<Item>.Empty;
        public ImmutableStack<Item> B { get; set; } = ImmutableStack<Item>.Empty;
        public ImmutableQueue<Item> C { get; set; } = ImmutableQueue<Item>.Empty;
        public ImmutableHashSet<Item> D { get; set; } = ImmutableHashSet<Item>.Empty;
        public ImmutableSortedSet<Item> E { get; set; } = ImmutableSortedSet<Item>.Empty;
        public ImmutableArray<Item> F { get; set; }
        public ImmutableDictionary<string, Item> G { get; set; } = ImmutableDictionary<string, Item>.Empty;
        public ImmutableSortedDictionary<string, Item> H { get; set; } = ImmutableSortedDictionary<string, Item>.Empty;
        public FrozenSet<Item> I { get; set; } = FrozenSet<Item>.Empty;
        public FrozenDictionary<string, Item> J { get; set; } = FrozenDictionary<string, Item>.Empty;
        public ImmutableList<int> Shared { get; set; } = ImmutableList<int>.Empty;
      }
      """,
    ["OtherBclCollections"] =
      $$"""
      using System.Collections.Concurrent;
      using System.Collections.Generic;
      using System.Collections.ObjectModel;
      using System.Text;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Others
      {
        public SortedDictionary<string, Item> A { get; set; } = new();
        public SortedList<string, int> B { get; set; } = new();
        public SortedSet<int> C { get; set; } = new();
        public LinkedList<Item> D { get; set; } = new();
        public ConcurrentDictionary<string, Item> E { get; set; } = new();
        public ReadOnlyCollection<Item>? F { get; set; }
        public ReadOnlyDictionary<string, Item>? G { get; set; }
        public StringBuilder H { get; set; } = new();
        public Stack<Item> I { get; set; } = new();
        public Queue<Item> J { get; set; } = new();
      }
      """,
    ["CollectionSubclass"] =
      $$"""
      using System.Collections.Generic;
      using TimeWarp.State;
      {{Item}}
      public class Tagged : List<Item> { public string Tag { get; set; } = ""; }
      public class TaggedMap : Dictionary<string, Item> { public int Version; }
      [GenerateClone] public class SubclassHolder { public Tagged T { get; set; } = new(); public TaggedMap M { get; set; } = new(); }
      """,
    ["Polymorphic"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public class Shape { public int X { get; set; } }
      public class Circle : Shape { public int Radius { get; set; } }
      public sealed class Ring : Circle { public int Inner { get; set; } }
      [GenerateClone] public class Drawing { public Shape? S { get; set; } public List<Shape> All { get; set; } = new(); }
      """,
    ["AmbiguousConstructor"] =
      """
      using TimeWarp.State;
      public class Money { public Money(decimal amount) { } public Money(string text) { } }
      public class Defaults { public Defaults(int count = 3, string name = "x", System.DayOfWeek day = System.DayOfWeek.Friday) { } }
      [GenerateClone] public class AmbiguousHolder { public Money? M { get; set; } public Defaults? D { get; set; } }
      """,
    ["PrivateConstructor"] =
      """
      using TimeWarp.State;
      public class Hidden { private Hidden() { } public static Hidden Create() => new(); public int N { get; set; } }
      [GenerateClone] public class HiddenHolder { public Hidden? H { get; set; } }
      """,
    ["RequiredMembers"] =
      """
      using TimeWarp.State;
      public class RequiredBase { public required string Name { get; set; } }
      public class RequiredField { public required int Count; }
      [GenerateClone] public class RequiredHolder : RequiredBase { public RequiredField? F { get; set; } }
      """,
    ["Records"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      public record PersonRecord(string Name, Item Tag) { public int Age { get; init; } }
      public readonly record struct Point(int X, Item Tag);
      [GenerateClone] public class RecordHolder { public PersonRecord? P { get; set; } public Point Q { get; set; } }
      """,
    ["NameCollision"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      namespace N
      {
        public class Common { public List<int> Shared = new(); }
        public class A { [GenerateClone] public class B : Common { } }
        [GenerateClone] public class A_B : Common { }
      }
      """,
    ["CloneableNonSealedBase"] =
      """
      using TimeWarp.State;
      public class Cl : System.ICloneable { public int V { get; set; } public object Clone() => new Cl { V = V }; }
      public class ClD : Cl { public int W { get; set; } }
      [GenerateClone] public class CloneableHolder { public Cl? C { get; set; } }
      """,
    ["PrivateCloneableSubclass"] =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      [GenerateClone] public class Holder
      {
        public Base? B { get; set; }
        private sealed class Hidden : Base, System.ICloneable { public object Clone() => new Hidden { V = V }; }
      }
      """,
    ["GenericSubclassOfGenericClass"] =
      """
      using TimeWarp.State;
      public class GB<T> { public T? V { get; set; } }
      public class GD<T> : GB<T> { public int W { get; set; } }
      public class GDD<T> : GD<T> { }
      [GenerateClone] public class GenericClassHolder { public GB<int>? G { get; set; } }
      """,
    ["GenericSubclassOfAbstractGenericClass"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public abstract class AB<T> { public T? V { get; set; } }
      public class AD<T> : AB<T> { }
      public class AInt : AB<int> { }
      public class AList<T> : AB<List<T>> { }
      public abstract record Result<T>;
      public sealed record Ok<T>(T Value) : Result<T>;
      public sealed record Err<T>(string Message) : Result<T>;
      [GenerateClone] public class AbstractGenericHolder
      {
        public AB<int>? A { get; set; }
        public AB<List<string>>? L { get; set; }
        public Result<List<string>>? R { get; set; }
      }
      """,
    ["GenericImplementationOfGenericInterface"] =
      """
      using TimeWarp.State;
      public interface IR<T> { T V { get; } }
      public class IntR : IR<int> { public int V { get; set; } }
      public class GR<T> : IR<T> { public T V { get; set; } = default!; }
      public struct SR<T> : IR<T> { public T V { get; set; } }
      [GenerateClone] public class GenericInterfaceHolder { public IR<int>? R { get; set; } }
      """,
    ["DependencyConstructorState"] =
      """
      using TimeWarp.State;
      public interface IClock { }
      public sealed class ClockState : State<ClockState>
      {
        [System.Runtime.Serialization.IgnoreDataMember]
        private readonly IClock? Clock;
        public ClockState(IClock? clock) { Clock = clock; }
        public int Ticks { get; set; }
        public override void Initialize() { }
      }
      """
  };

  [Input("GenericWrapper")]
  [Input("GenericStateBase")]
  [Input("NestedGeneric")]
  [Input("Tuple")]
  [Input("KeyValuePair")]
  [Input("NullableStruct")]
  [Input("CollectionInterfaces")]
  [Input("ImmutableAndFrozen")]
  [Input("OtherBclCollections")]
  [Input("CollectionSubclass")]
  [Input("Polymorphic")]
  [Input("AmbiguousConstructor")]
  [Input("PrivateConstructor")]
  [Input("RequiredMembers")]
  [Input("Records")]
  [Input("NameCollision")]
  [Input("DependencyConstructorState")]
  [Input("CloneableNonSealedBase")]
  [Input("PrivateCloneableSubclass")]
  [Input("GenericSubclassOfGenericClass")]
  [Input("GenericSubclassOfAbstractGenericClass")]
  [Input("GenericImplementationOfGenericInterface")]
  public static void Given_Shape(string shape)
  {
    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Shapes[shape]);
    runResult.Diagnostics.Select(diagnostic => diagnostic.ToString()).ShouldBeEmpty();
    StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }

  public static void Given_Generic_Field_Uses_Generic_Holder()
  {
    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Shapes["GenericWrapper"]);
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("internal static extern ref T0 ");
    source.ShouldContain("<global::Item>.F_Value_");
  }

  // A generic subtype of a generic member type is closed over the member's type arguments and gets its own case.
  [Input("GenericSubclassOfGenericClass", "global::GD<int>,global::GDD<int>")]
  [Input("GenericSubclassOfAbstractGenericClass", "global::AD<int>,global::AInt,global::AList<string>,global::Ok<global::System.Collections.Generic.List<string>>,global::Err<global::System.Collections.Generic.List<string>>")]
  [Input("GenericImplementationOfGenericInterface", "global::GR<int>,global::SR<int>,global::IntR")]
  public static void Given_Generic_Subtype_Of_Generic_Member_Dispatches_To_Closed_Type(string shape, string closedTypes)
  {
    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Shapes[shape]);
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    foreach (string closedType in closedTypes.Split(','))
    {
      source.ShouldContain($"case {closedType} ");
    }
  }

  public static void Given_Ambiguous_Overloads_Uses_Typed_Defaults()
  {
    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Shapes["AmbiguousConstructor"]);
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldMatch(@"new\(default\((decimal|string)\)!?\)");
    source.ShouldContain("(int)(3)");
    source.ShouldContain("(string)(\"x\")");
  }

  // ICloneable.Clone() is the type's own dispatch: a subclass that inherits Clone() may get the base type back, so the
  // result is cast to the declared type and no per-subclass cloner casts it to the subclass.
  public static void Given_Cloneable_Non_Sealed_Base_Casts_To_Declared_Type()
  {
    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Shapes["CloneableNonSealedBase"]);
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("global::Cl clone = (global::Cl)cloned;");
    source.ShouldNotContain("(global::ClD)");
    source.ShouldNotContain("Clone_ClD");
  }

  public static void Given_Immutable_Rebuild_Does_Not_Need_Linq()
  {
    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Shapes["ImmutableAndFrozen"]);
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldNotContain(".Select(");
  }
}

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
