#region Purpose
// Unsupported member shapes are TWSG002 at a source location, and the message names the offending member or type.
#endregion

#region Design
// Each [Input] names one entry in the Shapes table: the source and the message fragments that identify that shape
// (the member name plus the reason, or the subtype name). A bare "some TWSG002" would pass for the wrong reason.
// The guard in state-clone-shape-guard-tests.cs fails when an Input and a table key drift apart.
// Metadata-type cases live in state-clone-shape-metadata-tests.cs.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Report_TWSG002_At_Source
{
  internal static readonly Dictionary<string, (string Source, string[] MessageFragments)> Shapes = new()
  {
    ["ObjectMember"] =
      (
        """
        using TimeWarp.State;
        [GenerateClone] public class Holder { public object Payload { get; set; } = new(); }
        """,
        ["member 'Payload'", "System.Object can hold any runtime value"]
      ),
    ["PointerMember"] =
      (
        """
        using TimeWarp.State;
        [GenerateClone] public unsafe class Holder { public int* Pointer; }
        """,
        ["member 'Pointer'", "pointer types are not supported"]
      ),
    ["OpenGenericState"] =
      (
        """
        using TimeWarp.State;
        public class OpenState<T> : State<OpenState<T>> { public override void Initialize() { } }
        """,
        ["'OpenState<T>'", "the state is an open generic"]
      ),
    ["PrivateNestedState"] =
      (
        """
        using TimeWarp.State;
        public class Outer { private sealed class InnerState : State<InnerState> { public override void Initialize() { } } }
        """,
        ["'Outer.InnerState' is not accessible to generated code"]
      ),
    ["BclClassWithPrivateState"] =
      (
        """
        using TimeWarp.State;
        [GenerateClone] public class Holder { public System.IO.MemoryStream Stream { get; set; } = new(); }
        """,
        ["member 'Stream'", "'System.IO.MemoryStream' has private field"]
      ),
    ["InterfaceWithoutImplementation"] =
      (
        """
        using TimeWarp.State;
        public interface IThing { }
        [GenerateClone] public class Holder { public IThing? Thing { get; set; } }
        """,
        ["member 'Thing'", "interface or abstract type 'IThing' has no implementation"]
      ),
    ["ServiceWithoutCloneShared"] =
      (
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
        ["member 'Logger'", "interface or abstract type 'Microsoft.Extensions.Logging.ILogger<ServiceState>'"]
      ),
    ["UncloneableDerivedType"] =
      (
        """
        using TimeWarp.State;
        public class Shape { }
        public class Blob : Shape { public object Payload { get; set; } = new(); }
        [GenerateClone] public class Holder { public Shape? S { get; set; } }
        """,
        ["clone for 'Blob'", "reached from 'Holder'", "member 'Payload'"]
      ),
    ["ProtectedNestedSubclass"] =
      (
        """
        using TimeWarp.State;
        public class Base { public int V { get; set; } }
        public class Outer { protected class ProtBase : Base { } }
        [GenerateClone] public class Holder { public Base? B { get; set; } }
        """,
        ["member 'B'", "subtype 'Outer.ProtBase'"]
      ),
    ["PrivateImplementationBesidePublicOne"] =
      (
        """
        using TimeWarp.State;
        public interface IItem { int V { get; } }
        public sealed class PublicItem : IItem { public int V { get; set; } }
        public class Outer { private sealed class PrivItem : IItem { public int V { get; set; } } }
        [GenerateClone] public class Holder { public IItem? I { get; set; } }
        """,
        ["member 'I'", "subtype 'Outer.PrivItem'"]
      ),
    ["GenericSubclass"] =
      (
        """
        using TimeWarp.State;
        public class Base { public int V { get; set; } }
        public class Gen<T> : Base { public T? Value { get; set; } }
        [GenerateClone] public class Holder { public Base? B { get; set; } }
        """,
        ["member 'B'", "subtype 'Gen<T>'"]
      ),
    ["GenericSubclassWithUndeterminedParameter"] =
      (
        """
        using TimeWarp.State;
        public class GB<T> { public T? V { get; set; } }
        public class GX<T, U> : GB<T> { public U? W { get; set; } }
        [GenerateClone] public class Holder { public GB<int>? G { get; set; } }
        """,
        ["member 'G'", "subtype 'GX<T, U>'"]
      ),
    ["GenericSubclassWithFailingConstraint"] =
      (
        """
        using TimeWarp.State;
        public abstract class AB<T> { public T? V { get; set; } }
        public class AInt : AB<int> { }
        public class ARef<T> : AB<T> where T : class { }
        [GenerateClone] public class Holder { public AB<int>? A { get; set; } }
        """,
        ["member 'A'", "subtype 'ARef<T>'"]
      ),
    ["GenericImplementationWithUndeterminedParameter"] =
      (
        """
        using TimeWarp.State;
        public interface IR<T> { T V { get; } }
        public class IntR : IR<int> { public int V { get; set; } }
        public class GR<T, U> : IR<T> { public T V { get; set; } = default!; public U? W { get; set; } }
        [GenerateClone] public class Holder { public IR<int>? R { get; set; } }
        """,
        ["member 'R'", "subtype 'GR<T, U>'"]
      ),
    ["FileLocalSubclass"] =
      (
        """
        using TimeWarp.State;
        public class Base { public int V { get; set; } }
        file sealed class Local : Base { }
        [GenerateClone] public class Holder { public Base? B { get; set; } }
        """,
        ["member 'B'", "subtype 'Local'"]
      ),
    ["FileLocalGenerateClone"] =
      (
        """
        using TimeWarp.State;
        [GenerateClone] file class Holder { public int V { get; set; } }
        """,
        ["clone for 'Holder' because 'Holder' is not accessible to generated code"]
      ),
    ["FileLocalState"] =
      (
        """
        using TimeWarp.State;
        file sealed class LocalState : State<LocalState> { public override void Initialize() { } }
        """,
        ["clone for 'LocalState' because 'LocalState' is not accessible to generated code"]
      )
  };

  [Input("ObjectMember")]
  [Input("PointerMember")]
  [Input("OpenGenericState")]
  [Input("PrivateNestedState")]
  [Input("BclClassWithPrivateState")]
  [Input("InterfaceWithoutImplementation")]
  [Input("ServiceWithoutCloneShared")]
  [Input("UncloneableDerivedType")]
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
    (string source, string[] messageFragments) = Shapes[shape];
    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(source, allowUnsafe: true);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.Id.ShouldBe(StateGeneratorDiagnostics.CloneUnsupportedId);
    error.Location.IsInSource.ShouldBeTrue();
    string message = error.GetMessage();
    foreach (string fragment in messageFragments)
    {
      message.ShouldContain(fragment);
    }

    // The generated file must not add compiler errors (for example CS0400 for a file-local type) on top of TWSG002.
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.Ordinal) == true)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }

  // The only run of this shape: a private nested subtype of a member type is named at the member.
  public static void Given_Hidden_Subtype_Names_It_At_The_Member()
  {
    const string Source =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      [GenerateClone] public class Holder
      {
        public Base? B { get; set; }
        private sealed class PrivBase : Base { public int W { get; set; } }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) = StateCloneGeneratorTestDriver.Run(Source);
    Diagnostic error = runResult.Diagnostics.ShouldHaveSingleItem();
    error.Id.ShouldBe(StateGeneratorDiagnostics.CloneUnsupportedId);
    error.Location.IsInSource.ShouldBeTrue();
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
    error.Id.ShouldBe(StateGeneratorDiagnostics.CloneUnsupportedId);
    error.Location.IsInSource.ShouldBeTrue();
    string message = error.GetMessage();
    message.ShouldContain("'Inner'");
    message.ShouldContain("'Payload'");
    message.ShouldContain("reached from 'BoxState'");
  }
}
