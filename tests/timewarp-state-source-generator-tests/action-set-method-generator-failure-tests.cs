#region Purpose
// ActionSetMethodSourceGenerator failure shapes: TWSG003 for shapes the pre-141 generator served (a plain class
// Action in a class state), and silence for shapes it ignored, which compiled before (review round 1, M1).
#endregion

#region Design
// Split from action-set-method-generator-tests.cs for the 500-line cap. Uses the same ActionSetSources prelude. A
// TWSG003 case asserts one Error at a real tree location and no output; a silent case asserts no diagnostic and no
// output, which is what master produced for it.
#endregion

// ReSharper disable InconsistentNaming
namespace ActionSetMethodSourceGenerator_;

public class Should_Skip_Silently_Shapes_Not_Served_Before
{
  // Review round 1, M1: before task 141 the generator matched only a class Action in a class state. A record or
  // struct Action, or an ActionSet in a record or struct state, got no generated code and compiled. A shape problem
  // there must not become a TWSG003 Error; the generator emits nothing, as before.
  public static void Given_Record_Action_In_A_State_That_Is_Not_Partial()
  {
    AssertSilent(
      """
      namespace App;

      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed class PlainState : State<PlainState>
      {
        public override void Initialize() { }
        public static class PingActionSet { public sealed record Action : IAction; }
      }
      """);
  }

  public static void Given_Record_Action_Named_Like_A_State_Property()
  {
    AssertSilent(ActionSetSources.State(
      """
        public int Tok { get; private set; }

        public static class TokActionSet
        {
          public sealed record Action(int Value) : IAction;
        }
      """));
  }

  public static void Given_Record_Parameter_Named_Like_The_Generated_Token()
  {
    AssertSilent(ActionSetSources.State(
      """
        public static class WaitActionSet
        {
          public sealed record Action(System.Threading.CancellationToken externalCancellationToken) : IAction;
        }
      """));
  }

  public static void Given_Abstract_Record_Action()
  {
    AssertSilent(ActionSetSources.State(
      """
        public static class PingActionSet
        {
          public abstract record Action : IAction;
        }
      """));
  }

  public static void Given_Private_Record_Action()
  {
    AssertSilent(ActionSetSources.State(
      """
        public static class PingActionSet
        {
          private sealed record Action : IAction;
        }
      """));
  }

  public static void Given_Record_With_A_Private_Parameter_Type()
  {
    AssertSilent(ActionSetSources.State(
      """
        private sealed class Secret;

        // A private ActionSet, so the Action's own signature is consistent and this compiled before task 141.
        private static class HideActionSet
        {
          public sealed record Action(Secret Value) : IAction;
        }
      """));
  }

  public static void Given_Class_Action_In_A_Record_State_That_Is_Not_Partial()
  {
    AssertSilent(
      """
      namespace App;

      using TimeWarp.Mediator;

      public sealed record Holder : TimeWarp.State.IState
      {
        public System.Guid Guid { get; } = System.Guid.NewGuid();
        public void Initialize() { }
        public static class PingActionSet { public sealed class Action : IAction; }
      }
      """);
  }

  private static void AssertSilent(string source)
  {
    (string text, ImmutableArray<Diagnostic> diagnostics, _) = ActionSetSources.Run(source);

    diagnostics.ShouldBeEmpty();
    text.ShouldBeEmpty();
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
          public sealed class Action(System.Threading.CancellationToken externalCancellationToken) : IAction;
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
