#region Purpose
// Incrementality tests, Mediator style, for all four State generators: an unrelated edit leaves every named step
// Cached or Unchanged and re-adds no source; a relevant edit is Modified and re-emits.
#endregion

#region Design
// Cached means the step did not run. Unchanged means it ran and produced an equal value, so everything after it is
// Cached. A CreateSyntaxProvider or ForAttributeWithMetadataName transform reruns when the compilation changes and is
// Unchanged when its equatable model is equal; that is the property these tests pin, which is why both reasons are
// accepted for the first step. Output steps (RegisterSourceOutput) must be Cached after an unrelated edit, and the
// generated text must be identical.
// The clone generator plans from CompilationProvider, so its plan step always reruns; the test pins that the plan is
// Unchanged and its output Cached, which is all Option 0 promises. Moving planning off CompilationProvider is Option 1.
// The "edit in the same file" cases add a member far above the action. A model that held a span would be Modified;
// these models hold none on the success path.
// Task 140 left open whether ForAttributeWithMetadataName defeats caching (Mediator's design note says so). The
// catalog test measures it for State: the Entries step is Cached after an unrelated edit.
// Measured on Microsoft.CodeAnalysis.CSharp 4.14.0 (task 141, 2026-11-06), unrelated file added:
//   ActionSet.Candidates Cached, ActionSet.Models Cached; Catalog.Entries Cached, Catalog.AssemblyName Unchanged,
//   Catalog.Collected Cached; Persistence.Candidates Cached, Persistence.Models Cached; Clone.Plan Unchanged;
//   every output step Cached. Edit above the action in the same file: ActionSet.Candidates Cached.
// The assertions accept Cached or Unchanged for steps fed by a transform so a Roslyn upgrade that reports the
// other reason does not fail a test that still proves the output was not re-added.
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

  public static string WithLeadingMember(string source) =>
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
}

public class ActionSetMethod_Should_Stay_Cached
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new ActionSetMethodSourceGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(IncrementalSources.Unrelated, "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ActionSetCandidates).ShouldNotBeEmpty();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ActionSetCandidates).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ActionSetModels).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }

  public static void Given_Edit_Above_The_Action_In_The_Same_File()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new ActionSetMethodSourceGenerator());

    SyntaxTree tree = compilation.SyntaxTrees.Single();
    driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(tree,
      GeneratorTestHost.Parse(IncrementalSources.WithLeadingMember(IncrementalSources.ActionState), tree.FilePath)));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ActionSetCandidates).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
  }

  public static void Given_Action_Parameter_Changed_Re_Emits()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new ActionSetMethodSourceGenerator());

    SyntaxTree tree = compilation.SyntaxTrees.Single();
    driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(tree,
      GeneratorTestHost.Parse(IncrementalSources.WithChangedParameter(IncrementalSources.ActionState), tree.FilePath)));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ActionSetModels).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.OutputReasons(result).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.GeneratedTexts(driver).ShouldContain(text => text.Contains("bool Animate = false"));
  }
}

public class ActionCatalog_Should_Stay_Cached
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new ActionCatalogSourceGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);
    first.ShouldNotBeEmpty();

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(IncrementalSources.Unrelated, "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.CatalogEntries).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.CatalogAssemblyName).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.CatalogCollected).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }

  public static void Given_Action_Parameter_Changed_Re_Emits()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new ActionCatalogSourceGenerator());

    SyntaxTree tree = compilation.SyntaxTrees.Single();
    driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(tree,
      GeneratorTestHost.Parse(IncrementalSources.WithChangedParameter(IncrementalSources.ActionState), tree.FilePath)));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.CatalogEntries).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.OutputReasons(result).ShouldContain(IncrementalStepRunReason.Modified);
    GeneratorTestHost.GeneratedTexts(driver).ShouldContain(text => text.Contains("\"Animate\""));
  }
}

public class Persistence_Should_Stay_Cached
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.PersistentState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new PersistenceStateSourceGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);
    first.ShouldNotBeEmpty();

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(IncrementalSources.Unrelated, "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.PersistenceCandidates).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.PersistenceModels).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }

  public static void Given_New_Persistent_State_Re_Emits()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.PersistentState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new PersistenceStateSourceGenerator());

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(
      IncrementalSources.PersistentState.Replace("WidgetState", "GadgetState"), "Gadget.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.PersistenceModels).ShouldContain(IncrementalStepRunReason.New);
    result.GeneratedSources.Length.ShouldBe(2);
  }
}

public class StateClone_Should_Not_Re_Add_Source
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new StateCloneSourceGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);
    first.ShouldNotBeEmpty();

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse(IncrementalSources.Unrelated, "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ClonePlan).ShouldAllBe(reason => reason == IncrementalStepRunReason.Unchanged);
    GeneratorTestHost.OutputReasons(result).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }

  public static void Given_State_Member_Added_Re_Plans()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(IncrementalSources.ActionState);
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new StateCloneSourceGenerator());

    SyntaxTree tree = compilation.SyntaxTrees.Single();
    driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(tree,
      GeneratorTestHost.Parse(IncrementalSources.WithLeadingMember(IncrementalSources.ActionState), tree.FilePath)));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.ClonePlan).ShouldAllBe(reason => reason == IncrementalStepRunReason.Modified);
    GeneratorTestHost.OutputReasons(result).ShouldContain(IncrementalStepRunReason.Modified);
  }
}
