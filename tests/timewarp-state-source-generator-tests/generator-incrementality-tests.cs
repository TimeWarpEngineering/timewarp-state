#region Purpose
// Incrementality tests, Mediator style, for TimeWarpStateGenerator: an unrelated edit leaves every fact step, the
// State graph and every emit step Cached or Unchanged, and adds no source again. A relevant edit is Modified and
// re-emits only what changed.
#endregion

#region Design
// Cached means the step did not run. Unchanged means it ran and produced an equal value, so everything after it is
// Cached. A CreateSyntaxProvider transform reruns for a changed tree and is Unchanged when its equatable fact is equal.
// The clone plan and the assembly fact read CompilationProvider, so they rerun on every edit. The tests pin that they
// come out Unchanged, which keeps State.Graph Cached.
// Output steps (RegisterSourceOutput) must be Cached after an unrelated edit, and the generated text must be identical.
// Per-item emission is pinned too. Adding a state property changes the clone plan, so the graph is Modified, but the
// dispatch methods it projects are Unchanged, so their outputs are Cached and only the clone file is re-added.
// The "edit in the same file" case adds a method far above the action. Methods are not cloned, so the clone plan
// stays equal. A fact that held a span would be Modified, and these facts hold none on the success path.
// Accepting Cached or Unchanged for steps fed by a transform keeps a Roslyn upgrade that reports the other reason
// from failing a test that still proves the output was not re-added.
// Measured on Microsoft.CodeAnalysis.CSharp 4.14.0 (task 142, 2026-11-06), unrelated file added: see Results in the
// task 142 kitchen.
#endregion

// ReSharper disable InconsistentNaming
namespace GeneratorIncrementality_;

internal static class IncrementalSources
{
  public const string Unrelated =
    """
    namespace App.Other;

    public static class Helper
    {
      public static int Twice(int value) => value * 2;
    }
    """;

  public const string ActionState =
    """
    namespace App;

    using TimeWarp.Mediator;
    using TimeWarp.State;

    public sealed partial class CounterState : State<CounterState>
    {
      public int Count { get; private set; }

      public override void Initialize() { }

      public static class AddActionSet
      {
        [CatalogAction(Description = "Add to the count.")]
        public sealed record Action(int Amount = 1) : IAction;
      }
    }
    """;

  public static string WithLeadingMethod(string source) =>
    source.Replace("  public int Count { get; private set; }", "  public int Count { get; private set; }\n\n  public string Describe() => \"counter\";");

  public static string WithLeadingProperty(string source) =>
    source.Replace("  public int Count { get; private set; }", "  public int Count { get; private set; }\n\n  public string Label { get; private set; } = \"counter\";");

  public static string WithChangedParameter(string source) =>
    source.Replace("Action(int Amount = 1)", "Action(int Amount = 1, bool Animate = false)");

  public const string PersistentState =
    """
    namespace App;

    using TimeWarp.Features.Persistence;
    using TimeWarp.State;

    [PersistentState(PersistentStateMethod.SessionStorage)]
    public sealed partial class WidgetState : State<WidgetState>
    {
      public int Size { get; private set; }

      public override void Initialize() { }
    }
    """;

  public static CSharpCompilation Both() =>
    GeneratorTestHost.CreateCompilation(ActionState).AddSyntaxTrees(GeneratorTestHost.Parse(PersistentState, "Widget.cs"));

  public static GeneratorDriver ReplaceCounter(GeneratorDriver driver, CSharpCompilation compilation, Func<string, string> edit, out CSharpCompilation edited)
  {
    SyntaxTree tree = compilation.SyntaxTrees.First();
    edited = compilation.ReplaceSyntaxTree(tree, GeneratorTestHost.Parse(edit(ActionState), tree.FilePath));
    return driver.RunGenerators(edited);
  }
}

public class Should_Stay_Cached
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = IncrementalSources.Both();
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);
    first.ShouldContain(text => text.StartsWith(CatalogEmitter.HintName, StringComparison.Ordinal));
    first.ShouldContain(text => text.StartsWith(ManifestEmitter.HintName, StringComparison.Ordinal));
    first.ShouldContain(text => text.StartsWith(CloneEmitter.HintName, StringComparison.Ordinal));
    first.ShouldContain(text => text.Contains("_Persistence.g.cs"));
    first.ShouldContain(text => text.Contains("_Method.g.cs"));

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(IncrementalSources.Unrelated, "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    foreach (string step in new[] { TrackingNames.Actions, TrackingNames.Persistence, TrackingNames.States, TrackingNames.Assembly, TrackingNames.ClonePlan })
      GeneratorTestHost.TrackedReasons(result, step).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason), step);

    GeneratorTestHost.TrackedReasons(result, TrackingNames.Graph).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    foreach (string step in new[] { TrackingNames.EmitDispatch, TrackingNames.EmitPersistence, TrackingNames.EmitCatalog, TrackingNames.EmitManifest, TrackingNames.EmitClone })
      GeneratorTestHost.TrackedReasons(result, step).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached, step);

    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }

  public static void Given_Edit_Above_The_Action_In_The_Same_File()
  {
    CSharpCompilation compilation = IncrementalSources.Both();
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);

    driver = IncrementalSources.ReplaceCounter(driver, compilation, IncrementalSources.WithLeadingMethod, out _);

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Actions).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.States).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Graph).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }
}

public class Should_Re_Emit_Only_What_Changed
{
  public static void Given_Action_Parameter_Changed()
  {
    CSharpCompilation compilation = IncrementalSources.Both();
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());

    driver = IncrementalSources.ReplaceCounter(driver, compilation, IncrementalSources.WithChangedParameter, out _);

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Actions).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Graph).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitDispatch).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitCatalog).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitManifest).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitPersistence).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.OutputReasons(result).ShouldContain(IncrementalStepRunReason.Modified);

    string[] texts = GeneratorTestHost.GeneratedTexts(driver);
    texts.ShouldContain(text => text.Contains("bool Animate = false"));
    texts.ShouldContain(text => text.StartsWith(CatalogEmitter.HintName, StringComparison.Ordinal) && text.Contains("\"Animate\""));
    texts.ShouldContain(text => text.StartsWith(ManifestEmitter.HintName, StringComparison.Ordinal) && text.Contains("Animate"));
  }

  public static void Given_State_Property_Added_Re_Plans_The_Clone_Only()
  {
    CSharpCompilation compilation = IncrementalSources.Both();
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());

    driver = IncrementalSources.ReplaceCounter(driver, compilation, IncrementalSources.WithLeadingProperty, out _);

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ClonePlan).ShouldAllBe(reason => reason == IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Graph).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitClone).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitDispatch).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitPersistence).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitCatalog).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitManifest).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.OutputReasons(result).Count(reason => reason == IncrementalStepRunReason.Modified).ShouldBe(1);
  }

  public static void Given_New_Persistent_State()
  {
    CSharpCompilation compilation = IncrementalSources.Both();
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(
      IncrementalSources.PersistentState.Replace("WidgetState", "GadgetState"), "Gadget.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Persistence).ShouldContain(IncrementalStepRunReason.New);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitPersistence).ShouldContain(IncrementalStepRunReason.New);
    GeneratorTestHost.TrackedReasons(result, TrackingNames.EmitDispatch).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    result.GeneratedSources.Count(source => source.HintName.EndsWith("_Persistence.g.cs", StringComparison.Ordinal)).ShouldBe(2);
  }
}
