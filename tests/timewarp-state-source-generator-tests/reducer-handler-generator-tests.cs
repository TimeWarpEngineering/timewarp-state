#region Purpose
// [Reducer] handlers (TimeWarpStateGenerator, ReducerHandlerEmitter, task 144): the emitted Handler text, that it
// compiles, the shapes that emit nothing (ReducerAnalyzer reports those as TWS0014), and that the new emit step stays
// Cached after an unrelated edit.
#endregion

#region Design
// Each test compiles inline source through GeneratorTestHost and reads only the *_Reducer.g.cs sources. The test
// compilation references TimeWarp.State and TimeWarp.Mediator but does not run the Mediator generator, so these tests
// pin the State half: the type Mediator will name exists, derives from StateActionHandler<Action>, and calls Reduce.
// Runtime dispatch through the generated mediator is covered by the client integration tests (ReducerDispatch).
#endregion

// ReSharper disable InconsistentNaming
namespace ReducerHandlerGenerator_;

internal static class ReducerSources
{
  public static string State(string actionSets, string header = "public sealed partial class CounterState : State<CounterState>") =>
    $$"""
    namespace App;

    using TimeWarp.Mediator;
    using TimeWarp.State;

    {{header}}
    {
      public int Count { get; private set; }

      public override void Initialize() { }

    {{actionSets}}
    }
    """;

  public const string Decrement =
    """
      public static partial class DecrementActionSet
      {
        [Reducer]
        public sealed class Action(int amount) : IAction { public int Amount { get; } = amount; }

        private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
      }
    """;

  public static (GeneratedSourceResult[] Handlers, Compilation Output, GeneratorDriver Driver) Run(string source)
  {
    (GeneratorDriver driver, Compilation output, _) =
      GeneratorTestHost.Run(GeneratorTestHost.CreateCompilation(source), new TimeWarpStateGenerator());
    GeneratedSourceResult[] handlers = GeneratorTestHost.Sources(driver)
      .Where(generated => generated.HintName.EndsWith("_Reducer.g.cs", StringComparison.Ordinal))
      .ToArray();
    return (handlers, output, driver);
  }
}

public class Should_Emit_Handler
{
  public static void Given_Reducer_Action()
  {
    (GeneratedSourceResult[] handlers, Compilation output, _) = ReducerSources.Run(ReducerSources.State(ReducerSources.Decrement));

    GeneratedSourceResult handler = handlers.ShouldHaveSingleItem();
    handler.HintName.ShouldBe("App.CounterState+DecrementActionSet.Handler_Reducer.g.cs");
    string text = handler.SourceText.ToString();
    text.ShouldContain("static partial class DecrementActionSet");
    text.ShouldContain("[global::System.CodeDom.Compiler.GeneratedCode(\"TimeWarp.State.SourceGenerator\", \"1.0.0\")]");
    text.ShouldContain("public sealed class Handler(global::TimeWarp.State.IStore store)");
    text.ShouldContain(": global::TimeWarp.State.StateActionHandler<global::App.CounterState.DecrementActionSet.Action>(store)");
    text.ShouldContain("global::App.CounterState.DecrementActionSet.Reduce(Store.GetState<global::App.CounterState>(), action);");
    GeneratorTestHost.ShouldCompile(output);

    INamedTypeSymbol? emitted = output.GetTypeByMetadataName("App.CounterState+DecrementActionSet+Handler");
    emitted.ShouldNotBeNull();
    emitted.AllInterfaces.ShouldContain(type => type.OriginalDefinition.ToDisplayString() == "TimeWarp.Mediator.IRequestHandler<TRequest>");
  }

  public static void Given_Internal_Action_And_Custom_Name()
  {
    (GeneratedSourceResult[] handlers, Compilation output, _) = ReducerSources.Run(ReducerSources.State(
      """
        public static partial class ResetActionSet
        {
          [Reducer("Reducer")]
          internal sealed record Action : IAction;

          private static void Reduce(CounterState state, Action action) => state.Count = 0;
        }
      """));

    string text = handlers.ShouldHaveSingleItem().SourceText.ToString();
    text.ShouldContain("internal sealed class Reducer(global::TimeWarp.State.IStore store)");
    GeneratorTestHost.ShouldCompile(output);
    output.GetTypeByMetadataName("App.CounterState+ResetActionSet+Reducer").ShouldNotBeNull();
  }

  public static void Given_State_Nested_In_Static_Class_And_Non_Static_Action_Set()
  {
    const string Source =
      """
      namespace App;

      using TimeWarp.Mediator;
      using TimeWarp.State;

      public static partial class Feature
      {
        public sealed partial class CounterState : State<CounterState>
        {
          public int Count { get; private set; }

          public override void Initialize() { }

          public partial class DecrementActionSet
          {
            [Reducer]
            public sealed class Action : IAction { }

            private static void Reduce(CounterState state, Action action) => state.Count--;
          }
        }
      }
      """;

    (GeneratedSourceResult[] handlers, Compilation output, _) = ReducerSources.Run(Source);

    string text = handlers.ShouldHaveSingleItem().SourceText.ToString();
    text.ShouldContain("static partial class Feature");
    text.ShouldContain("partial class CounterState");
    text.ShouldNotContain("static partial class DecrementActionSet");
    text.ShouldContain("partial class DecrementActionSet");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Global_Namespace_State()
  {
    string source = ReducerSources.State(ReducerSources.Decrement).Replace("namespace App;", "");

    (GeneratedSourceResult[] handlers, Compilation output, _) = ReducerSources.Run(source);

    handlers.ShouldHaveSingleItem().HintName.ShouldBe("Global.CounterState+DecrementActionSet.Handler_Reducer.g.cs");
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Not_Emit_Handler
{
  public static void Given_No_Reducer_Attribute()
  {
    (GeneratedSourceResult[] handlers, _, _) = ReducerSources.Run(ReducerSources.State(ReducerSources.Decrement.Replace("[Reducer]", "")));

    handlers.ShouldBeEmpty();
  }

  public static void Given_Action_Set_Not_Partial()
  {
    (GeneratedSourceResult[] handlers, _, _) = ReducerSources.Run(ReducerSources.State(ReducerSources.Decrement.Replace("static partial class", "static class")));

    handlers.ShouldBeEmpty();
  }

  public static void Given_No_Reduce_Method()
  {
    (GeneratedSourceResult[] handlers, _, _) = ReducerSources.Run(ReducerSources.State(
      ReducerSources.Decrement.Replace("private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;", "")));

    handlers.ShouldBeEmpty();
  }

  public static void Given_Hand_Written_Handler()
  {
    (GeneratedSourceResult[] handlers, Compilation output, _) = ReducerSources.Run(ReducerSources.State(
      """
        public static partial class DecrementActionSet
        {
          [Reducer]
          public sealed class Action : IAction { }

          private static void Reduce(CounterState state, Action action) { }

          internal sealed class Handler(IStore store) : StateActionHandler<Action>(store)
          {
            public override System.Threading.Tasks.ValueTask Handle(Action action, System.Threading.CancellationToken cancellationToken) => default;
          }
        }
      """));

    handlers.ShouldBeEmpty();
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Stay_Cached
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(ReducerSources.State(ReducerSources.Decrement));
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);
    first.ShouldContain(text => text.Contains("_Reducer.g.cs"));

    const string Unrelated = "namespace App.Other; public static class Helper { public static int Twice(int value) => value * 2; }";
    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(Unrelated, "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Graph).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitReducer).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }

  public static void Given_Reduce_Body_Edited()
  {
    // Only the Reduce body changes, so the handler model is equal: the action fact reruns and comes out Unchanged.
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(ReducerSources.State(ReducerSources.Decrement));
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());

    SyntaxTree tree = compilation.SyntaxTrees.First();
    string edited = ReducerSources.State(ReducerSources.Decrement.Replace("state.Count -= action.Amount", "state.Count -= action.Amount * 2"));
    driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(tree, GeneratorTestHost.Parse(edited, tree.FilePath)));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Actions).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitReducer).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
  }
}
