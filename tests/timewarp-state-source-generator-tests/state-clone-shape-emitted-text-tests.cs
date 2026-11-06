#region Purpose
// Asserts on the text the clone stage (CloneEmitter) emits for specific supported shapes.
#endregion

#region Design
// Compilation of every supported shape is covered by the per-group Should_Compile_* tests. These cases pin the
// generated text where a shape has a distinctive output contract: generic dispatch, typed defaults, the ICloneable
// cast, no LINQ in immutable rebuilds, and [CloneShared] assignment.
// Runtime behavior of the same shapes is covered in timewarp-state-tests/cloning/generated-clone-generics-tests.cs,
// generated-clone-collections-tests.cs, and generated-clone-constructors-tests.cs.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Emit_Shape_Text
{
  private static string CloneSourceOf(string shape) =>
    StateCloneGeneratorTestDriver.CloneSource(StateCloneGeneratorTestDriver.Run(StateCloneShapeSources.Find(shape)).RunResult)
      .ShouldNotBeNull();

  public static void Given_Generic_Field_Uses_Generic_Holder()
  {
    string source = CloneSourceOf("GenericWrapper");
    source.ShouldContain("internal static extern ref T0 ");
    source.ShouldContain("<global::Item>.F_Value_");
  }

  // A generic subtype of a generic member type is closed over the member's type arguments and gets its own case.
  [Input("GenericSubclassOfGenericClass", "global::GD<int>,global::GDD<int>")]
  [Input("GenericSubclassOfAbstractGenericClass", "global::AD<int>,global::AInt,global::AList<string>,global::Ok<global::System.Collections.Generic.List<string>>,global::Err<global::System.Collections.Generic.List<string>>")]
  [Input("GenericImplementationOfGenericInterface", "global::GR<int>,global::SR<int>,global::IntR")]
  public static void Given_Generic_Subtype_Of_Generic_Member_Dispatches_To_Closed_Type(string shape, string closedTypes)
  {
    string source = CloneSourceOf(shape);
    foreach (string closedType in closedTypes.Split(','))
    {
      source.ShouldContain($"case {closedType} ");
    }
  }

  public static void Given_Ambiguous_Overloads_Uses_Typed_Defaults()
  {
    string source = CloneSourceOf("AmbiguousConstructor");
    source.ShouldMatch(@"new\(default\((decimal|string)\)!?\)");
    source.ShouldContain("(int)(3)");
    source.ShouldContain("(string)(\"x\")");
  }

  // ICloneable.Clone() is the type's own dispatch: a subclass that inherits Clone() may get the base type back, so the
  // result is cast to the declared type and no per-subclass cloner casts it to the subclass.
  public static void Given_Cloneable_Non_Sealed_Base_Casts_To_Declared_Type()
  {
    string source = CloneSourceOf("CloneableNonSealedBase");
    source.ShouldContain("global::Cl clone = (global::Cl)cloned;");
    source.ShouldNotContain("(global::ClD)");
    source.ShouldNotContain("Clone_ClD");
  }

  public static void Given_Immutable_Rebuild_Does_Not_Need_Linq() =>
    CloneSourceOf("ImmutableAndFrozen").ShouldNotContain(".Select(");

  // [CloneShared] assigns the service from the source. The generator must not try to clone ILogger, HttpClient,
  // or an abstract navigation type.
  public static void Given_CloneShared_Assigns_The_Source_Reference()
  {
    string source = CloneSourceOf("CloneSharedServices");
    source.ShouldContain("F_Logger_");
    source.ShouldContain("F_HttpClient_");
    source.ShouldContain("F_Navigation_");
    source.ShouldNotContain("Clone_ILogger");
    source.ShouldNotContain("Clone_HttpClient");
    source.ShouldNotContain("Clone_AppNavigation");
  }

  // [CloneShared] wins over an ignore attribute, and also applies through [field: CloneShared] and on a private
  // field of a base class in the compilation.
  public static void Given_CloneShared_Wins_Over_Ignore_Attributes()
  {
    string source = CloneSourceOf("CloneSharedPrecedence");
    source.ShouldContain("F_Logger_");
    source.ShouldContain("F_Timer_");
    source.ShouldContain("F_Stopwatch_");
    source.ShouldContain("F_BaseClock_");
    source.ShouldNotContain("Clone_IClock");
  }
}
