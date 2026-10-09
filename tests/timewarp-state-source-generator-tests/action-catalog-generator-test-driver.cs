#region Purpose
// Runs ActionSetMethodSourceGenerator + ActionCatalogSourceGenerator against in-memory source that
// references TimeWarp.State, so tests can assert both the emitted text and that it compiles.
#endregion

#region Design
// References every trusted platform assembly plus TimeWarp.State and TimeWarp.Mediator.Contracts so generated code can
// be compiled. The assembly name Catalog.Tests is fixed so expected output can name the generated namespace.
#endregion

namespace TimeWarp.State.SourceGenerator.Tests;

internal static class ActionCatalogGeneratorTestDriver
{
  public const string AssemblyName = "Catalog.Tests";

  public static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) Run(string source)
  {
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));

    IEnumerable<MetadataReference> references =
      ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
      .Split(Path.PathSeparator)
      .Select(path => MetadataReference.CreateFromFile(path))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.State.CatalogActionAttribute).Assembly.Location))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.Mediator.IAction).Assembly.Location));

    CSharpCompilation compilation = CSharpCompilation.Create
    (
      assemblyName: AssemblyName,
      syntaxTrees: [syntaxTree],
      references: references,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
    );

    GeneratorDriver driver = CSharpGeneratorDriver.Create
    (
      new ActionSetMethodSourceGenerator().AsSourceGenerator(),
      new ActionCatalogSourceGenerator().AsSourceGenerator()
    );
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
    return (driver.GetRunResult(), outputCompilation);
  }

  public static string CatalogSource(GeneratorDriverRunResult runResult) =>
    runResult.Results
      .SelectMany(result => result.GeneratedSources)
      .Single(generated => generated.HintName == ActionCatalogSourceGenerator.HintName)
      .SourceText
      .ToString();

  public static void ShouldCompile(Compilation outputCompilation) =>
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
}
