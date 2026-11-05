#region Purpose
// Golden parity: the State generator emits exactly what the four pre-142 generators emitted for every case in
// GoldenSources, hint names, text and diagnostics included.
#endregion

#region Design
// golden/<case>.verified.txt was written by the four pre-142 generators before the rewrite (task 142). Each file
// lists every generated source in hint-name order, then every generator diagnostic in location order. The State
// manifest is new output, so its hint name is left out here; manifest tests pin it.
// Set TIMEWARP_STATE_UPDATE_GOLDEN=1 to rewrite the files. Do that only for an intended output change, and review the
// diff like code.
// The files are read relative to this source file (CallerFilePath), so nothing is copied to the output folder.
#endregion

// ReSharper disable InconsistentNaming
namespace GoldenParity_;

internal static class Golden
{
  public const string UpdateVariable = "TIMEWARP_STATE_UPDATE_GOLDEN";

  public static IIncrementalGenerator[] Generators() => [new TimeWarpStateGenerator()];

  public static string Render(GeneratorDriver driver)
  {
    GeneratorDriverRunResult run = driver.GetRunResult();
    StringBuilder builder = new();
    foreach (GeneratedSourceResult source in run.Results
      .SelectMany(result => result.GeneratedSources)
      .Where(source => source.HintName != ManifestEmitter.HintName)
      .OrderBy(source => source.HintName, StringComparer.Ordinal))
    {
      builder.Append("// ==== ").Append(source.HintName).Append(" ====\n");
      builder.Append(source.SourceText.ToString().Replace("\r\n", "\n"));
      builder.Append('\n');
    }

    builder.Append("// ==== diagnostics ====\n");
    foreach (Diagnostic diagnostic in run.Diagnostics
      .OrderBy(diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.Ordinal)
      .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start)
      .ThenBy(diagnostic => diagnostic.Id, StringComparer.Ordinal))
    {
      FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
      builder.Append($"{diagnostic.Id} {diagnostic.Severity} {span.Path}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1}): {diagnostic.GetMessage()}\n");
    }

    return builder.ToString();
  }

  public static void Verify(string caseName, string actual, [CallerFilePath] string callerPath = "")
  {
    string directory = Path.Combine(Path.GetDirectoryName(callerPath)!, "golden");
    string path = Path.Combine(directory, caseName + ".verified.txt");
    if (Environment.GetEnvironmentVariable(UpdateVariable) == "1")
    {
      Directory.CreateDirectory(directory);
      File.WriteAllText(path, actual);
      return;
    }

    File.Exists(path).ShouldBeTrue($"golden file {path} is missing; set {UpdateVariable}=1 to write it");
    actual.ShouldBe(File.ReadAllText(path).Replace("\r\n", "\n"));
  }

  public static string RunReferenced(string source) =>
    Render(GeneratorTestHost.Run(GeneratorTestHost.CreateCompilation(source, "Golden.Tests"), Generators()).Driver);

  public static string RunBare(string source)
  {
    CSharpCompilation compilation = CSharpCompilation.Create
    (
      assemblyName: "Golden.Bare",
      syntaxTrees: [GeneratorTestHost.Parse(source, "Host.cs")],
      references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );
    return Render(GeneratorTestHost.Run(compilation, Generators()).Driver);
  }
}

public class Should_Match_Pre_142_Output
{
  public static void Given_Counter() => Golden.Verify("counter", Golden.RunReferenced(GoldenSources.Cases["counter"]));

  public static void Given_Shapes() => Golden.Verify("shapes", Golden.RunReferenced(GoldenSources.Cases["shapes"]));

  public static void Given_Failures() => Golden.Verify("failures", Golden.RunReferenced(GoldenSources.Cases["failures"]));

  public static void Given_Clone() => Golden.Verify("clone", Golden.RunReferenced(GoldenSources.Cases["clone"]));

  public static void Given_Persistence_Without_TimeWarp_State() =>
    Golden.Verify("persistence-bare", Golden.RunBare(GoldenSources.BareCases["persistence-bare"]));

  // Names every case key, so adding, renaming or dropping a case fails here until a Given_ test above covers it.
  public static void Given_Every_Case_Has_A_Test()
  {
    GoldenSources.Cases.Keys.Order(StringComparer.Ordinal).ShouldBe(["clone", "counter", "failures", "shapes"]);
    GoldenSources.BareCases.Keys.Order(StringComparer.Ordinal).ShouldBe(["persistence-bare"]);
  }
}
