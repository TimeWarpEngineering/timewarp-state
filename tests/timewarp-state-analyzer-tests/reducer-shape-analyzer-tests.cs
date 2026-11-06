#region Purpose
// Proves every State analyzer accepts the [Reducer] (short) action shape: TWS0001 nesting, TWS0004 to TWS0008 catalog,
// TWS0010/TWS0011 inheritance, and TWS0014 itself on a [Reducer] action that also carries [CatalogAction].
#endregion

#region Design
// Task 145, option 5. The short shape is a static partial *ActionSet holding a [Reducer] Action and a static Reduce,
// with no hand-written Handler. Each analyzer runs on the same sources twice: once as the user writes them, and once
// with the Handler the generator emits added as a ".g.cs" file carrying [GeneratedCode], which is what the analyzers
// see on a real build. The "still reports" tests prove the short shape does not hide a real fault from a rule.
// {|#0:...|} marks the expected location.
#endregion

// ReSharper disable InconsistentNaming
namespace ReducerShapeAnalyzers_;

using static ReducerShapeRunner;

public class Should_Not_Report
{
  public static async Task Given_Reducer_Action_Set_To_TWS0001()
  {
    await RunAsync<TimeWarpStateActionAnalyzer>(State(ReducerActionSet));
    await RunWithGeneratedHandlerAsync<TimeWarpStateActionAnalyzer>(State(ReducerActionSet));
  }

  public static async Task Given_Reducer_Action_Set_To_Catalog_Rules()
  {
    await RunAsync<CatalogActionAnalyzer>(State(CatalogedReducerActionSet));
    await RunWithGeneratedHandlerAsync<CatalogActionAnalyzer>(State(CatalogedReducerActionSet));
  }

  public static async Task Given_Reducer_Action_Set_To_Inheritance_Rules()
  {
    await RunAsync<StateInheritanceAnalyzer>(State(ReducerActionSet));
    await RunWithGeneratedHandlerAsync<StateInheritanceAnalyzer>(State(ReducerActionSet));
  }

  public static async Task Given_Cataloged_Reducer_To_TWS0014()
  {
    await RunAsync<ReducerAnalyzer>(State(CatalogedReducerActionSet));
    await RunWithGeneratedHandlerAsync<ReducerAnalyzer>(State(CatalogedReducerActionSet));
  }

  public static async Task Given_Long_And_Short_Shapes_Side_By_Side()
  {
    string source = State(ReducerActionSet + LongShapeActionSet);

    await RunAsync<TimeWarpStateActionAnalyzer>(source);
    await RunAsync<CatalogActionAnalyzer>(source);
    await RunAsync<StateInheritanceAnalyzer>(source);
    await RunAsync<ReducerAnalyzer>(source);
  }
}

public class Should_Still_Report_TWS0001
{
  public static async Task Given_Reducer_Action_Set_Outside_A_State()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public static partial class Commands
      {
        public static partial class DecrementActionSet
        {
          [Reducer]
          public sealed class {|#0:Action|} : IAction { }
        }
      }
      """;

    await RunAsync<TimeWarpStateActionAnalyzer>
    (
      TestCode,
      new DiagnosticResult(TimeWarpStateActionAnalyzer.NestActionInStateDiagnosticId, DiagnosticSeverity.Error)
        .WithLocation(0)
        .WithArguments("Action")
    );
  }
}

public class Should_Still_Report_Catalog_Rules
{
  public static async Task Given_Reducer_In_Generic_State_TWS0004()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState<T> : State<CounterState<T>>
      {
        public override void Initialize() { }

        public static partial class DecrementActionSet
        {
          [Reducer]
          [{|#0:CatalogAction(Description = "Decrement the counter.")|}]
          public sealed class Action : IAction { }

          private static void Reduce(CounterState<T> state, Action action) { }
        }
      }
      """;

    await RunAsync<CatalogActionAnalyzer>(TestCode, Catalog(CatalogActionAnalyzer.PlacementDiagnosticId, "CounterState<T>.DecrementActionSet.Action"));
  }

  public static async Task Given_Reducer_Without_Description_TWS0005()
  {
    await RunAsync<CatalogActionAnalyzer>
    (
      State(Cataloged("CatalogAction")),
      Catalog(CatalogActionAnalyzer.DescriptionDiagnosticId, DecrementAction)
    );
  }

  public static async Task Given_Two_Reducers_With_One_Catalog_Name_TWS0006()
  {
    await RunAsync<CatalogActionAnalyzer>
    (
      State(
        """
          public static partial class DecrementActionSet
          {
            [Reducer]
            [{|#0:CatalogAction(Description = "Decrement the counter.")|}]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }
          }

          public static partial class LowerActionSet
          {
            [Reducer]
            [{|#1:CatalogAction(Description = "Lower the counter.", Name = "Counter.Decrement")|}]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) { }
          }
        """),
      new DiagnosticResult(CatalogActionAnalyzer.DuplicateNameDiagnosticId, DiagnosticSeverity.Error).WithLocation(0).WithArguments("Counter.Decrement"),
      new DiagnosticResult(CatalogActionAnalyzer.DuplicateNameDiagnosticId, DiagnosticSeverity.Error).WithLocation(1).WithArguments("Counter.Decrement")
    );
  }

  public static async Task Given_Reducer_With_Two_Sentence_Description_TWS0007()
  {
    await RunAsync<CatalogActionAnalyzer>
    (
      State(Cataloged("CatalogAction(Description = \"Decrement the counter. Stops at zero.\")")),
      new DiagnosticResult(CatalogActionAnalyzer.PlainSentenceDiagnosticId, DiagnosticSeverity.Warning).WithLocation(0).WithArguments(DecrementAction)
    );
  }

  public static async Task Given_Reducer_With_Empty_DisplayName_TWS0008()
  {
    await RunAsync<CatalogActionAnalyzer>
    (
      State(Cataloged("CatalogAction(Description = \"Decrement the counter.\", DisplayName = \"\")")),
      Catalog(CatalogActionAnalyzer.DisplayNameDiagnosticId, DecrementAction)
    );
  }

  private static string Cataloged(string catalogAttribute) =>
    $$"""
      public static partial class DecrementActionSet
      {
        [Reducer]
        [{|#0:{{catalogAttribute}}|}]
        public sealed class Action : IAction { }

        private static void Reduce(CounterState state, Action action) { }
      }
    """;

  private static DiagnosticResult Catalog(string id, string action) =>
    new DiagnosticResult(id, DiagnosticSeverity.Error).WithLocation(0).WithArguments(action);
}

public class Should_Still_Report_Inheritance_Rules
{
  public static async Task Given_Reducer_In_State_With_Wrong_Type_Argument_TWS0010()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed class OtherState : State<OtherState>
      {
        public override void Initialize() { }
      }

      public sealed partial class {|#0:CounterState|} : State<OtherState>
      {
        public override void Initialize() { }

        public static partial class DecrementActionSet
        {
          [Reducer]
          public sealed class Action : IAction { }

          private static void Reduce(OtherState state, Action action) { }
        }
      }
      """;

    await RunAsync<StateInheritanceAnalyzer>
    (
      TestCode,
      new DiagnosticResult(StateInheritanceAnalyzer.InheritanceDiagnosticId, DiagnosticSeverity.Error).WithLocation(0)
    );
  }

  public static async Task Given_Reducer_In_Unsealed_State_TWS0011()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public partial class {|#0:CounterState|} : State<CounterState>
      {
        public int Count { get; private set; }

        public override void Initialize() { }

        public static partial class DecrementActionSet
        {
          [Reducer]
          public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

          private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
        }
      }
      """;

    await RunAsync<StateInheritanceAnalyzer>
    (
      TestCode,
      new DiagnosticResult(StateInheritanceAnalyzer.SealedDiagnosticId, DiagnosticSeverity.Warning)
        .WithLocation(0)
        .WithArguments("CounterState")
    );
  }
}

internal static class ReducerShapeRunner
{
  public const string DecrementAction = "CounterState.DecrementActionSet.Action";

  public const string ReducerActionSet =
    """
      public static partial class DecrementActionSet
      {
        [Reducer]
        public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

        private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
      }

    """;

  public const string CatalogedReducerActionSet =
    """
      public static partial class DecrementActionSet
      {
        [Reducer]
        [CatalogAction(Description = "Decrement the counter.", DisplayName = "Decrement", Permissions = new[] { "counter.write" })]
        public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

        private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
      }

    """;

  public const string LongShapeActionSet =
    """
      public static class IncrementActionSet
      {
        [CatalogAction(Description = "Increment the counter.")]
        public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

        internal sealed class Handler(IStore store) : StateActionHandler<Action>(store)
        {
          private CounterState CounterState => Store.GetState<CounterState>();

          public override System.Threading.Tasks.ValueTask Handle(Action action, System.Threading.CancellationToken cancellationToken)
          {
            CounterState.Count += action.Amount;
            return default;
          }
        }
      }

    """;

  // The Handler the State generator emits for DecrementActionSet (see reducer-handler-emitter.cs).
  private const string GeneratedHandler =
    """
    using TimeWarp.State;

    partial class CounterState
    {
      static partial class DecrementActionSet
      {
        [global::System.CodeDom.Compiler.GeneratedCode("TimeWarp.State.SourceGenerator", "1.0.0")]
        public sealed class Handler(global::TimeWarp.State.IStore store) : global::TimeWarp.State.StateActionHandler<Action>(store)
        {
          public override global::System.Threading.Tasks.ValueTask Handle(Action action, global::System.Threading.CancellationToken cancellationToken)
          {
            Reduce(Store.GetState<CounterState>(), action);
            return default;
          }
        }
      }
    }
    """;

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

  public static async Task RunAsync<TAnalyzer>(string testCode, params DiagnosticResult[] expected)
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    CSharpAnalyzerTest<TAnalyzer, FixieVerifier> analyzerTest = AnalyzerTestFactory.Create<TAnalyzer>(testCode);
    analyzerTest.ExpectedDiagnostics.AddRange(expected);
    await analyzerTest.RunAsync();
  }

  public static async Task RunWithGeneratedHandlerAsync<TAnalyzer>(string testCode)
    where TAnalyzer : DiagnosticAnalyzer, new()
  {
    CSharpAnalyzerTest<TAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<TAnalyzer>(("State.cs", testCode), ("Handler.g.cs", GeneratedHandler));
    await analyzerTest.RunAsync();
  }
}
