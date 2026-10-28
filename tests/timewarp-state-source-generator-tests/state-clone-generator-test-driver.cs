#region Purpose
// Runs StateCloneSourceGenerator against in-memory source that references TimeWarp.State.
#endregion

#region Design
// Trusted platform assemblies plus TimeWarp.State and TimeWarp.Mediator let the generator see State<T>
// and let the test compile the emitted cloners. Diagnostics and generated text are both asserted.
#endregion

namespace TimeWarp.State.SourceGenerator.Tests;

internal static class StateCloneGeneratorTestDriver
{
  public static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) Run(string source, bool allowUnsafe = false)
  {
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
    IEnumerable<MetadataReference> references =
      ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
      .Split(Path.PathSeparator)
      .Select(path => MetadataReference.CreateFromFile(path))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.State.State<>).Assembly.Location))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.Mediator.IAction).Assembly.Location));

    CSharpCompilation compilation = CSharpCompilation.Create
    (
      assemblyName: "CloneGeneratorTests",
      syntaxTrees: [syntaxTree],
      references: references,
      options: new CSharpCompilationOptions
      (
        OutputKind.DynamicallyLinkedLibrary,
        nullableContextOptions: NullableContextOptions.Enable,
        allowUnsafe: allowUnsafe
      )
    );

    return Run(compilation, new StateCloneSourceGenerator());
  }

  public static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) Run
  (
    string source,
    IIncrementalGenerator generator,
    bool allowUnsafe = false
  )
  {
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
    CSharpCompilation compilation = CompilationFor(source: null, syntaxTree, allowUnsafe);
    return Run(compilation, generator);
  }

  private static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) Run
  (
    CSharpCompilation compilation,
    IIncrementalGenerator generator
  )
  {
    GeneratorDriver driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
    return (driver.GetRunResult(), outputCompilation);
  }

  private static CSharpCompilation CompilationFor(string? source, SyntaxTree? syntaxTree, bool allowUnsafe)
  {
    SyntaxTree tree = syntaxTree ?? CSharpSyntaxTree.ParseText(source!, new CSharpParseOptions(LanguageVersion.Latest));
    IEnumerable<MetadataReference> references =
      ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
      .Split(Path.PathSeparator)
      .Select(path => MetadataReference.CreateFromFile(path))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.State.State<>).Assembly.Location))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.Mediator.IAction).Assembly.Location));

    return CSharpCompilation.Create
    (
      assemblyName: "CloneGeneratorTests",
      syntaxTrees: [tree],
      references: references,
      options: new CSharpCompilationOptions
      (
        OutputKind.DynamicallyLinkedLibrary,
        nullableContextOptions: NullableContextOptions.Enable,
        allowUnsafe: allowUnsafe
      )
    );
  }

  public static string? CloneSource(GeneratorDriverRunResult runResult) =>
    runResult.Results
      .SelectMany(result => result.GeneratedSources)
      .Select(generated => generated.SourceText.ToString())
      .SingleOrDefault();

  public static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) RunWithMetadataBase(
    string baseSource,
    string derivedSource,
    bool referenceAssembly = false,
    string baseAssemblyName = "CloneGeneratorBase")
  {
    CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
    IEnumerable<MetadataReference> platform =
      ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
      .Split(Path.PathSeparator)
      .Select(path => MetadataReference.CreateFromFile(path))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.State.State<>).Assembly.Location))
      .Append(MetadataReference.CreateFromFile(typeof(TimeWarp.Mediator.IAction).Assembly.Location));

    CSharpCompilation baseCompilation = CSharpCompilation.Create
    (
      assemblyName: baseAssemblyName,
      syntaxTrees: [CSharpSyntaxTree.ParseText(baseSource, parseOptions)],
      references: platform,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );

    using MemoryStream baseImage = new();
    // A metadata-only image without private members is what a project reference compiles against
    // (ProduceReferenceAssembly): it carries [ReferenceAssembly] and drops private class fields.
    Microsoft.CodeAnalysis.Emit.EmitOptions emitOptions = new(metadataOnly: referenceAssembly, includePrivateMembers: !referenceAssembly);
    Microsoft.CodeAnalysis.Emit.EmitResult emit = baseCompilation.Emit(baseImage, options: emitOptions);
    if (!emit.Success)
    {
      throw new InvalidOperationException(string.Join(Environment.NewLine, emit.Diagnostics));
    }

    baseImage.Position = 0;
    IEnumerable<MetadataReference> references = platform.Append(MetadataReference.CreateFromStream(baseImage));
    CSharpCompilation compilation = CSharpCompilation.Create
    (
      assemblyName: "CloneGeneratorTests",
      syntaxTrees: [CSharpSyntaxTree.ParseText(derivedSource, parseOptions)],
      references: references,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
    );

    GeneratorDriver driver = CSharpGeneratorDriver.Create(new StateCloneSourceGenerator().AsSourceGenerator());
    driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
    return (driver.GetRunResult(), outputCompilation);
  }
}
