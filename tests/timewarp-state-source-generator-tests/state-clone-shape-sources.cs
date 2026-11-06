#region Purpose
// Shared lookup, constants, and compile checks for the table-driven clone-stage shape tests.
#endregion

#region Design
// The supported-shape dictionaries live in state-clone-shape-sources-{generics,collections,constructors,shared}.cs,
// one partial of this class each. Keys are unique across groups, so Find resolves a key without naming its group.
// ErrorsOf is the shared "the generated file compiles" assertion input: only compiler errors, rendered for failure output.
#endregion

namespace StateCloneSourceGenerator_;

internal static partial class StateCloneShapeSources
{
  public const string Item = "public class Item { public int V { get; set; } }";

  // Computed on use: static initializer order across the partial files is unspecified.
  public static IReadOnlyDictionary<string, string> All =>
    new[] { GenericShapes, CollectionShapes, ConstructorShapes, SharedShapes }
      .SelectMany(group => group)
      .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

  public static string Find(string shape) => All[shape];

  public static List<string> ErrorsOf(Compilation compilation) =>
    compilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ToList();

  // A supported shape: no generator diagnostic, one generated file, and the output compiles.
  public static void AssertCompiles(string source)
  {
    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(source);
    runResult.Diagnostics.Select(diagnostic => diagnostic.ToString()).ShouldBeEmpty();
    StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    ErrorsOf(outputCompilation).ShouldBeEmpty();
  }
}
