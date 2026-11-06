#region Purpose
// ReducerAnalyzer: TWS0014 reports a [Reducer] Action the State generator will not emit a Handler for, with the reason.
#endregion

#region Design
// The analyzer test compilation does not run the State generator, so a valid [Reducer] has no Handler here and must
// still not be reported: the rule judges the source shape, not the presence of the type. The generated-handler case
// adds the Handler in a ".g.cs" file with the generator's [GeneratedCode] marker, which is what the analyzer sees on a
// real build. ReducerAttribute comes from the TimeWarp.State.dll metadata, so its handlerTypeName default is read the
// way a consumer's ProjectReference or PackageReference reads it. {|#0:...|} marks the attribute.
#endregion

// ReSharper disable InconsistentNaming
namespace ReducerAnalyzer_;

using static ReducerAnalyzerRunner;

public class Should_Not_Report
{
  public static async Task Given_Valid_Reducer()
  {
    await AnalyzerTestFactory.Create<ReducerAnalyzer>(State(
      """
        public static partial class DecrementActionSet
        {
          [Reducer]
          public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

          private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
        }
      """)).RunAsync();
  }

  public static async Task Given_The_Handler_The_Generator_Emitted()
  {
    const string Generated =
      """
      using TimeWarp.State;

      partial class CounterState
      {
        static partial class DecrementActionSet
        {
          [global::System.CodeDom.Compiler.GeneratedCode("TimeWarp.State.SourceGenerator", "1.0.0")]
          public sealed class Handler(IStore store) : StateActionHandler<Action>(store)
          {
            public override global::System.Threading.Tasks.ValueTask Handle(Action action, global::System.Threading.CancellationToken cancellationToken) => default;
          }
        }
      }
      """;

    await AnalyzerTestFactory.Create<ReducerAnalyzer>
    (
      ("State.cs", State(
        """
          public static partial class DecrementActionSet
          {
            [Reducer]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }
          }
        """)),
      ("Handler.g.cs", Generated)
    ).RunAsync();
  }

  public static async Task Given_Custom_Handler_Name()
  {
    await AnalyzerTestFactory.Create<ReducerAnalyzer>(State(
      """
        public static partial class DecrementActionSet
        {
          [Reducer("Reducer")]
          internal sealed class Action : IAction { }

          private static void Reduce(CounterState state, Action action) { }
        }
      """)).RunAsync();
  }
}

public class Should_Report_TWS0014
{
  public static async Task Given_Top_Level_Request()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      [{|#0:Reducer|}]
      public sealed class Action : IAction { }
      """;

    await RunAsync(TestCode, Expected("Action", "it is not the nested Action class of an *ActionSet inside a state"));
  }

  public static async Task Given_Action_Set_Not_Partial()
  {
    await RunAsync
    (
      State(
        """
          public static class DecrementActionSet
          {
            [{|#0:Reducer|}]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }
          }
        """),
      Expected
      (
        "CounterState.DecrementActionSet.Action",
        "'CounterState.DecrementActionSet' is not partial; add the partial modifier so the generated handler can join it"
      )
    );
  }

  public static async Task Given_No_Reduce_Method()
  {
    await RunAsync
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [{|#0:Reducer|}]
            public sealed class Action : IAction { }
          }
        """),
      Expected("CounterState.DecrementActionSet.Action", "the action set does not declare 'static void Reduce(CounterState state, Action action)'")
    );
  }

  public static async Task Given_Instance_Reduce_Method()
  {
    await RunAsync
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [{|#0:Reducer|}]
            public sealed class Action : IAction { }
          }

          private void Reduce(CounterState state, DecrementActionSet.Action action) { }
        """),
      Expected("CounterState.DecrementActionSet.Action", "the action set does not declare 'static void Reduce(CounterState state, Action action)'")
    );
  }

  public static async Task Given_Hand_Written_Handler()
  {
    await RunAsync
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [{|#0:Reducer|}]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }

            internal sealed class Handler(IStore store) : StateActionHandler<Action>(store)
            {
              public override System.Threading.Tasks.ValueTask Handle(Action action, System.Threading.CancellationToken cancellationToken) => default;
            }
          }
        """),
      Expected
      (
        "CounterState.DecrementActionSet.Action",
        "the action set already declares 'Handler'; remove it, or remove [Reducer] and keep the hand-written handler"
      )
    );
  }

  public static async Task Given_Dotted_Handler_Name()
  {
    await RunAsync
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [{|#0:Reducer("Other.Handler")|}]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }
          }
        """),
      Expected
      (
        "CounterState.DecrementActionSet.Action",
        "the handler name 'Other.Handler' is not a simple type name; the generator emits one type nested in the action set"
      )
    );
  }

  public static async Task Given_Private_Action()
  {
    await RunAsync
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [{|#0:Reducer|}]
            private sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }
          }
        """),
      Expected("CounterState.DecrementActionSet.Action", "the Action is private or protected, so the generated handler and Mediator cannot reach it")
    );
  }

  public static async Task Given_Action_Without_IAction()
  {
    await RunAsync
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [{|#0:Reducer|}]
            public sealed class Action { }

            private static void Reduce(CounterState state, Action action) { }
          }
        """),
      Expected("CounterState.DecrementActionSet.Action", "the Action does not implement IAction")
    );
  }
}

internal static class ReducerAnalyzerRunner
{
  public static string State(string actionSets) =>
    $$"""
    using TimeWarp.Mediator;
    using TimeWarp.State;

    public sealed partial class CounterState : State<CounterState>
    {
      public int Count { get; private set; }

      public override void Initialize() { }

    {{actionSets}}
    }
    """;

  public static DiagnosticResult Expected(string action, string reason) =>
    new DiagnosticResult(ReducerAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
      .WithLocation(0)
      .WithArguments(action, reason);

  public static async Task RunAsync(string testCode, params DiagnosticResult[] expected)
  {
    CSharpAnalyzerTest<ReducerAnalyzer, FixieVerifier> analyzerTest = AnalyzerTestFactory.Create<ReducerAnalyzer>(testCode);
    analyzerTest.ExpectedDiagnostics.AddRange(expected);
    await analyzerTest.RunAsync();
  }
}
