#region Purpose
// Shared host for the ActionSet method tests and the incrementality tests: builds a compilation that references
// TimeWarp.State and TimeWarp.Mediator, runs one or more generators with step tracking, and reads step run reasons.
#endregion

#region Design
// Step tracking is on so a test can read TrackedSteps by the names in TrackingNames. Reasons returns every output
// reason of every run of the named step, so ShouldAllBe(Cached or Unchanged) is the Mediator-style assertion.
// Driver and compilation share one parse options instance; generated trees must be parsed the same way.
// Platform references are built once; a MetadataReference is immutable.
#endregion

namespace TimeWarp.State.SourceGenerator.Tests;

internal static class GeneratorTestHost
{
  public static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

  private static readonly Lazy<MetadataReference[]> References = new(() =>
    ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
      .Split(Path.PathSeparator)
      .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.State.CatalogActionAttribute).Assembly.Location))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.Mediator.IAction).Assembly.Location))
      .ToArray());

  public static CSharpCompilation CreateCompilation(string source, string assemblyName = "GeneratorTests") =>
    CSharpCompilation.Create
    (
      assemblyName: assemblyName,
      syntaxTrees: [Parse(source, "Host.cs")],
      references: References.Value,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
    );

  public static SyntaxTree Parse(string source, string path) => CSharpSyntaxTree.ParseText(source, ParseOptions, path: path);

  public static GeneratorDriver CreateDriver(params IIncrementalGenerator[] generators) =>
    CSharpGeneratorDriver.Create
    (
      generators.Select(generator => generator.AsSourceGenerator()),
      additionalTexts: null,
      parseOptions: ParseOptions,
      optionsProvider: null,
      driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)
    );

  public static (GeneratorDriver Driver, Compilation Output, ImmutableArray<Diagnostic> Diagnostics) Run
  (
    Compilation compilation,
    params IIncrementalGenerator[] generators
  )
  {
    GeneratorDriver driver = CreateDriver(generators)
      .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> diagnostics);
    return (driver, output, diagnostics);
  }

  public static IncrementalStepRunReason[] Reasons(GeneratorRunResult result, string stepName) =>
    result.TrackedSteps.TryGetValue(stepName, out ImmutableArray<IncrementalGeneratorRunStep> steps)
      ? steps.SelectMany(step => step.Outputs).Select(output => output.Reason).ToArray()
      : [];

  /// <summary>Reasons for a step that must have run or been cached; an unknown step name fails instead of passing.</summary>
  public static IncrementalStepRunReason[] TrackedReasons(GeneratorRunResult result, string stepName)
  {
    IncrementalStepRunReason[] reasons = Reasons(result, stepName);
    reasons.ShouldNotBeEmpty($"step '{stepName}' was not tracked");
    return reasons;
  }

  public static IncrementalStepRunReason[] OutputReasons(GeneratorRunResult result)
  {
    IncrementalStepRunReason[] reasons = result.TrackedOutputSteps
      .SelectMany(pair => pair.Value)
      .SelectMany(step => step.Outputs)
      .Select(output => output.Reason)
      .ToArray();
    reasons.ShouldNotBeEmpty("no output step was tracked");
    return reasons;
  }

  public static string[] GeneratedTexts(GeneratorDriver driver) =>
    driver.GetRunResult().Results
      .SelectMany(result => result.GeneratedSources)
      .OrderBy(source => source.HintName, StringComparer.Ordinal)
      .Select(source => source.HintName + "\n" + source.SourceText)
      .ToArray();

  public static GeneratedSourceResult[] Sources(GeneratorDriver driver) =>
    driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources).ToArray();

  public static void ShouldCompile(Compilation output) =>
    output.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();

  public static bool IsCachedOrUnchanged(IncrementalStepRunReason reason) =>
    reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged;
}
