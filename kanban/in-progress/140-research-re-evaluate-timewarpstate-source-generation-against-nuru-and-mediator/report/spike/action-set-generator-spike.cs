#:package Microsoft.CodeAnalysis.CSharp@4.14.0
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

string dll = args[0];
var asm = Assembly.LoadFrom(dll);
IIncrementalGenerator Gen(string n) => (IIncrementalGenerator)Activator.CreateInstance(asm.GetType("TimeWarp.State.SourceGenerator." + n)!)!;

const string Stubs = """
namespace Lib { public interface IAction {} public interface ISender { System.Threading.Tasks.Task Send(object o, System.Threading.CancellationToken t); }
  public abstract class StateBase { protected ISender Sender = null!; protected System.Threading.CancellationToken CancellationToken; } }
""";

var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
void Run(string label, string src, string? edit = null)
{
  var trees = new List<SyntaxTree> { CSharpSyntaxTree.ParseText(Stubs), CSharpSyntaxTree.ParseText(src, path: "a.cs") };
  var comp = CSharpCompilation.Create("Spike", trees, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
  GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { Gen("ActionSetMethodSourceGenerator").AsSourceGenerator() }, driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
  driver = driver.RunGeneratorsAndUpdateCompilation(comp, out var outComp, out var genDiags);
  var errs = outComp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Id + " " + d.GetMessage()).Distinct().ToList();
  Console.WriteLine($"== {label}: generated={driver.GetRunResult().GeneratedTrees.Length} errors={errs.Count}");
  foreach (var e in errs.Take(3)) Console.WriteLine("   " + e);
  foreach (var g in driver.GetRunResult().GeneratedTrees) foreach (var l in g.ToString().Split('\n').Where(l => l.Contains("public async Task"))) Console.WriteLine("   gen: " + l.Trim());
  if (edit is not null)
  {
    var comp2 = outComp.RemoveAllSyntaxTrees().AddSyntaxTrees(trees).AddSyntaxTrees(CSharpSyntaxTree.ParseText(edit, path: "b.cs"));
    driver = driver.RunGenerators(comp2);
    var steps = driver.GetRunResult().Results[0].TrackedOutputSteps.SelectMany(kv => kv.Value).SelectMany(s => s.Outputs).Select(o => o.Reason.ToString());
    Console.WriteLine("   after unrelated edit, output step reasons: " + string.Join(",", steps));
  }
}

Run("ctor class", """
namespace App; using Lib;
public partial class CounterState : StateBase { public static class AddActionSet { public sealed class Action : IAction { public Action(int amount = 1) { Amount = amount; } public int Amount { get; } } } }
""", edit: "namespace App; class Unrelated { }");
Run("init-only props, no ctor", """
namespace App; using Lib;
public partial class CounterState : StateBase { public static class IncrementCountActionSet { public sealed class Action : IAction { public int Amount { get; init; } } } }
""");
Run("positional record", """
namespace App; using Lib;
public partial class CounterState : StateBase { public static class AddActionSet { public sealed record Action(int Amount) : IAction; } }
""");
Run("primary ctor class", """
namespace App; using Lib;
public partial class CounterState : StateBase { public static class AddActionSet { public sealed class Action(int amount) : IAction { public int Amount => amount; } } }
""");
Run("name match in non-state class", """
namespace App;
public static class Docs { public static class SampleActionSet { public sealed class Action { } } }
""");
