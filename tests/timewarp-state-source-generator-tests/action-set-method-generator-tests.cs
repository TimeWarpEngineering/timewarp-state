#region Purpose
// ActionSetMethodSourceGenerator: every Action shape from the task 140 spike, required members, state shapes
// (nested, generic, global namespace, internal), TWSG003 cases, and that SG002 is gone.
#endregion

#region Design
// Each test compiles inline source through GeneratorTestHost with only the ActionSet method generator, then asserts
// the generated signature text and that the output compiles (or, for TWSG003, that nothing is emitted). The shapes
// that compiled before (explicit constructor, init-only property) pin their old signature, which is the non-breaking
// rule. Sources share one State<T> prelude so each test reads as the Action shape alone.
#endregion

// ReSharper disable InconsistentNaming
namespace ActionSetMethodSourceGenerator_;

internal static class ActionSetSources
{
  public const string Usings =
    """
    using TimeWarp.Mediator;
    using TimeWarp.State;

    """;

  public static string State(string actionSets, string stateName = "CounterState", string @namespace = "App") =>
    $$"""
    namespace {{@namespace}};

    {{Usings}}
    public sealed partial class {{stateName}} : State<{{stateName}}>
    {
      public override void Initialize() { }

    {{actionSets}}
    }
    """;

  public static (string Text, ImmutableArray<Diagnostic> Diagnostics, Compilation Output) Run(string source)
  {
    (GeneratorDriver driver, Compilation output, _) =
      GeneratorTestHost.Run(GeneratorTestHost.CreateCompilation(source), new ActionSetMethodSourceGenerator());
    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    string text = string.Join("\n", result.GeneratedSources.Select(generated => generated.SourceText.ToString()));
    return (text, result.Diagnostics, output);
  }
}

public class Should_Keep_Existing_Signatures
{
  public static void Given_Explicit_Constructor_With_Default()
  {
    (string text, ImmutableArray<Diagnostic> diagnostics, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class AddActionSet
        {
          public sealed class Action : IAction
          {
            public Action(int amount = 1) { Amount = amount; }
            public int Amount { get; }
          }
        }
      """));

    text.ShouldContain("public async Task Add(int amount = 1, CancellationToken? externalCancellationToken = null)");
    text.ShouldContain("new AddActionSet.Action(amount),");
    diagnostics.ShouldBeEmpty();
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Init_Only_Property_And_No_Constructor()
  {
    // The test app's IncrementCountActionSet shape. A plain init property is not a parameter: adding one would change
    // the signature of a method that compiles today.
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class IncrementCountActionSet
        {
          public sealed class Action : IAction
          {
            public int Amount { get; init; }
          }
        }
      """));

    text.ShouldContain("public async Task IncrementCount(CancellationToken? externalCancellationToken = null)");
    text.ShouldContain("new IncrementCountActionSet.Action(),");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Generated_Code_Without_SG002()
  {
    (_, ImmutableArray<Diagnostic> diagnostics, _) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class ResetActionSet
        {
          public sealed class Action : IAction;
        }
      """));

    diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "SG002");
  }
}

public class Should_Read_Primary_Constructors
{
  public static void Given_Positional_Record()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class AddActionSet
        {
          public sealed record Action(int Amount, bool Animate = false) : IAction;
        }
      """));

    text.ShouldContain("public async Task Add(int Amount, bool Animate = false, CancellationToken? externalCancellationToken = null)");
    text.ShouldContain("new AddActionSet.Action(Amount, Animate),");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Class_Primary_Constructor()
  {
    // Before task 141 this emitted Add(CancellationToken?) and `new Action()`: CS7036.
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class AddActionSet
        {
          public sealed class Action(int amount) : IAction
          {
            public int Amount => amount;
          }
        }
      """));

    text.ShouldContain("public async Task Add(int amount, CancellationToken? externalCancellationToken = null)");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Record_Struct()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class MoveActionSet
        {
          public readonly record struct Action(int X, int Y) : IAction;
        }
      """));

    text.ShouldContain("public async Task Move(int X, int Y, CancellationToken? externalCancellationToken = null)");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Primary_And_Explicit_Constructors_Prefers_Primary()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class SetActionSet
        {
          public sealed class Action(string name, int count) : IAction
          {
            public Action(string name) : this(name, 0) { }
            public string Name => name;
            public int Count => count;
          }
        }
      """));

    text.ShouldContain("public async Task Set(string name, int count, CancellationToken? externalCancellationToken = null)");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Partial_Action_Across_Declarations_Emits_One_Method()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static partial class RenameActionSet
        {
          public sealed partial class Action : IAction;
        }

        public static partial class RenameActionSet
        {
          public sealed partial class Action
          {
            public Action(string name) { Name = name; }
            public string Name { get; }
          }
        }
      """));

    text.Split("public async Task Rename(").Length.ShouldBe(2);
    text.ShouldContain("Rename(string name, CancellationToken? externalCancellationToken = null)");
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Turn_Required_Members_Into_Parameters
{
  public static void Given_Required_Init_Properties()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class RenameActionSet
        {
          public sealed class Action : IAction
          {
            public required string Name { get; init; }
            public required int Version { get; init; }
          }
        }
      """));

    text.ShouldContain("public async Task Rename(string name, int version, CancellationToken? externalCancellationToken = null)");
    text.ShouldContain("new RenameActionSet.Action() { Name = name, Version = version },");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Required_Member_And_Optional_Constructor_Parameter()
  {
    // Required members go before optional constructor parameters; the constructor still gets its arguments in order.
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class SaveActionSet
        {
          public sealed record Action(string Id, bool Force = false) : IAction
          {
            public required string Reason { get; init; }
          }
        }
      """));

    text.ShouldContain("public async Task Save(string Id, string reason, bool Force = false, CancellationToken? externalCancellationToken = null)");
    text.ShouldContain("new SaveActionSet.Action(Id, Force) { Reason = reason },");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_SetsRequiredMembers_Constructor_Skips_Required_Members()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class NameActionSet
        {
          public sealed class Action : IAction
          {
            [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
            public Action(string name) { Name = name; }
            public required string Name { get; init; }
          }
        }
      """));

    text.ShouldContain("public async Task Name(string name, CancellationToken? externalCancellationToken = null)");
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Merge_With_Any_State_Shape
{
  public static void Given_Nested_Generic_And_Global_Namespace_States()
  {
    const string Source =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class GlobalState : State<GlobalState>
      {
        public override void Initialize() { }
        public static class PingActionSet { public sealed class Action : IAction; }
      }

      namespace Outer.Inner
      {
        public static partial class Feature
        {
          public sealed partial class NestedState : State<NestedState>
          {
            public override void Initialize() { }
            public static class PingActionSet { public sealed record Action(int Count) : IAction; }
          }
        }

        public sealed partial class ListState<TItem> : State<ListState<TItem>>
        {
          public override void Initialize() { }
          public static class ClearActionSet { public sealed class Action : IAction; }
        }
      }
      """;

    (string text, ImmutableArray<Diagnostic> diagnostics, Compilation output) = ActionSetSources.Run(Source);

    diagnostics.ShouldBeEmpty();
    text.ShouldNotContain("namespace Global");
    text.ShouldContain("namespace Outer.Inner;");
    text.ShouldContain("partial class Feature");
    text.ShouldContain("partial class ListState<TItem>");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Internal_Parameter_Type_On_Public_State_Emits_Internal_Method()
  {
    // A public method would be CS0051; internal is the only form that compiles.
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        internal sealed record Secret(string Value);

        internal static class RevealActionSet
        {
          internal sealed record Action(Secret Secret) : IAction;
        }
      """));

    text.ShouldContain("internal async Task Reveal(global::App.CounterState.Secret Secret, CancellationToken? externalCancellationToken = null)");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Default_Naming_A_Constant_Inside_The_Action()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public enum Mode { Slow, Fast }

        public static class RunActionSet
        {
          public sealed class Action : IAction
          {
            public const int DefaultTimes = 3;
            public Action(int times = DefaultTimes, Mode mode = Mode.Fast, decimal rate = 1.5m) { }
          }
        }
      """));

    text.ShouldContain("Run(int times = 3, global::App.CounterState.Mode mode = (global::App.CounterState.Mode)(1), decimal rate = 1.5m,");
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Stay_Silent_Outside_States
{
  public static void Given_Record_Action_With_A_Hand_Written_Method()
  {
    // The test app's ApplicationState.FiveSecondTask shape: a record Action got no method before task 141, so the
    // app wrote one. Emitting another would be CS0111 (same signature) or CS0121 (a near overload).
    (string text, ImmutableArray<Diagnostic> diagnostics, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class WaitActionSet
        {
          public sealed record Action : IAction;
        }

        public System.Threading.Tasks.Task Wait(System.Threading.CancellationToken cancellationToken = default) =>
          Sender.Send(new WaitActionSet.Action(), cancellationToken).AsTask();
      """));

    text.ShouldBeEmpty();
    diagnostics.ShouldBeEmpty();
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Class_Action_With_An_Identical_Hand_Written_Method()
  {
    (string text, _, Compilation output) = ActionSetSources.Run(ActionSetSources.State(
      """
        public static class PingActionSet
        {
          public sealed class Action : IAction;
        }

        public async System.Threading.Tasks.Task Ping(System.Threading.CancellationToken? externalCancellationToken = null) =>
          await Sender.Send(new PingActionSet.Action(), externalCancellationToken ?? CancellationToken);
      """));

    text.ShouldBeEmpty();
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_ActionSet_In_A_Static_Docs_Class()
  {
    // Before task 141 this emitted `partial class Docs`: CS0260, CS0708 and CS0119.
    const string Source =
      """
      namespace App;

      using TimeWarp.Mediator;

      public static class Docs
      {
        public static class SampleActionSet
        {
          public sealed class Action : IAction;
        }
      }
      """;

    (string text, ImmutableArray<Diagnostic> diagnostics, Compilation output) = ActionSetSources.Run(Source);

    text.ShouldBeEmpty();
    diagnostics.ShouldBeEmpty();
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Report_TWSG003_Instead_Of_Broken_Code
{
  public static void Given_State_That_Is_Not_Partial()
  {
    const string Source =
      """
      namespace App;

      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed class PlainState : State<PlainState>
      {
        public override void Initialize() { }
        public static class PingActionSet { public sealed class Action : IAction; }
      }
      """;

    AssertSingleFailure(Source, "is not partial");
  }

  public static void Given_Parameter_Named_Like_The_Generated_Token()
  {
    AssertSingleFailure(ActionSetSources.State(
      """
        public static class WaitActionSet
        {
          public sealed record Action(System.Threading.CancellationToken externalCancellationToken) : IAction;
        }
      """), "externalCancellationToken");
  }

  public static void Given_Abstract_Action()
  {
    AssertSingleFailure(ActionSetSources.State(
      """
        public static class PingActionSet
        {
          public abstract class Action : IAction;
        }
      """), "abstract");
  }

  public static void Given_Private_Constructor()
  {
    AssertSingleFailure(ActionSetSources.State(
      """
        public static class PingActionSet
        {
          public sealed class Action : IAction
          {
            private Action(int value) { }
          }
        }
      """), "constructor is private or protected");
  }

  public static void Given_Required_Member_That_Collides_With_A_Constructor_Parameter()
  {
    AssertSingleFailure(ActionSetSources.State(
      """
        public static class TagActionSet
        {
          public sealed class Action(string name) : IAction
          {
            public required string Name { get; init; }
          }
        }
      """), "'name'");
  }

  private static void AssertSingleFailure(string source, string reason)
  {
    (string text, ImmutableArray<Diagnostic> diagnostics, _) = ActionSetSources.Run(source);

    Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
    diagnostic.Id.ShouldBe(StateGeneratorDiagnostics.CannotGenerateMemberId);
    diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
    diagnostic.GetMessage().ShouldContain(reason);
    diagnostic.Location.SourceTree.ShouldNotBeNull();
    text.ShouldBeEmpty();
  }
}
