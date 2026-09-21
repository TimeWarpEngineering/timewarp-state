#region Purpose
// Runs PersistenceStateSourceGenerator against in-memory syntax for generator tests.
#endregion

namespace TimeWarp.State.SourceGenerator.Tests;

internal static class PersistenceGeneratorTestDriver
{
  public static GeneratorDriverRunResult Run(string source)
  {
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
    CSharpCompilation compilation = CSharpCompilation.Create(
      assemblyName: "PersistenceGeneratorTests",
      syntaxTrees: [syntaxTree],
      references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    GeneratorDriver driver = CSharpGeneratorDriver.Create(new PersistenceStateSourceGenerator());
    driver = driver.RunGenerators(compilation);
    return driver.GetRunResult();
  }
}
