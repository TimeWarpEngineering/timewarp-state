#region Purpose
// State manifest (TimeWarpStateGenerator, ManifestEmitter): the JSON document, when it is emitted, and its three
// readers: StateManifest.Parse, StateManifest.FromAssembly on a loaded assembly, and an analyzer reading the
// assembly attribute from a referenced assembly's metadata.
#endregion

#region Design
// The JSON is pinned exactly for one representative state, so a format change is a deliberate test change. The
// round-trip test emits the generated assembly, loads it into a collectible context that shares TimeWarp.State with
// the test, and reads the manifest the way DevTools export does. The metadata test is what an architecture coverage
// analyzer does: it reads AttributeData from IAssemblySymbol.GetAttributes() on a compiled reference.
// [TrackAction] and LoadPersistentStateRequest (which the generated Load sends) live in TimeWarp.State.Plus, which
// the host does not reference, so the source declares stand-ins with the same metadata names.
#endregion

// ReSharper disable InconsistentNaming
namespace StateManifest_;

internal static class ManifestSources
{
  public const string Counter =
    """
    namespace TimeWarp.Features.ActionTracking
    {
      [System.AttributeUsage(System.AttributeTargets.Class)]
      public sealed class TrackActionAttribute : System.Attribute { }
    }

    namespace TimeWarp.State.Plus.PersistentState
    {
      public sealed class LoadPersistentStateRequest(System.Type stateType) : TimeWarp.Mediator.IRequest
      {
        public System.Type StateType { get; } = stateType;
      }
    }

    namespace App.Counter
    {
      using TimeWarp.Features.ActionTracking;
      using TimeWarp.Features.Persistence;
      using TimeWarp.Mediator;
      using TimeWarp.State;

      [PersistentState(PersistentStateMethod.LocalStorage)]
      public sealed partial class CounterState : State<CounterState>
      {
        public int Count { get; private set; }

        public override void Initialize() { }

        public static class AddActionSet
        {
          [CatalogAction(Description = "Add to the count.")]
          [TrackAction]
          public sealed record Action(int Amount, string Note = "x") : IAction;
        }

        public static class ResetActionSet
        {
          public sealed class Action : IAction { }
        }
      }

      public sealed class EmptyState : State<EmptyState>, System.ICloneable
      {
        public override void Initialize() { }
        public object Clone() => new EmptyState();
      }

      public abstract class BaseState<T> : State<T> where T : BaseState<T> { }
    }
    """;

  public const string ExpectedJson =
    """{"version":1,"assembly":"Manifest.Tests","states":[""" +
    """{"type":"App.Counter.CounterState","name":"CounterState","persistent":"LocalStorage","clone":"generated","actions":[""" +
    """{"type":"App.Counter.CounterState+AddActionSet+Action","name":"AddActionSet.Action","method":"Add","parameters":[""" +
    """{"name":"Amount","type":"int","default":null,"required":true},{"name":"Note","type":"string","default":"\"x\"","required":false}],""" +
    "\"catalog\":\"Counter.Add\",\"tracked\":true}," +
    """{"type":"App.Counter.CounterState+ResetActionSet+Action","name":"ResetActionSet.Action","method":"Reset","parameters":[],"catalog":null,"tracked":false}]},""" +
    """{"type":"App.Counter.EmptyState","name":"EmptyState","persistent":null,"clone":"cloneable","actions":[]}]}""";

  public static (GeneratorDriver Driver, Compilation Output) Run(string source, string assemblyName = "Manifest.Tests")
  {
    (GeneratorDriver driver, Compilation output, _) =
      GeneratorTestHost.Run(GeneratorTestHost.CreateCompilation(source, assemblyName), new TimeWarpStateGenerator());
    return (driver, output);
  }

  public static string? ManifestSource(GeneratorDriver driver) =>
    driver.GetRunResult().Results
      .SelectMany(result => result.GeneratedSources)
      .Where(source => source.HintName == ManifestEmitter.HintName)
      .Select(source => source.SourceText.ToString())
      .SingleOrDefault();

  public static byte[] Emit(Compilation compilation)
  {
    using MemoryStream image = new();
    Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(image);
    result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
    return image.ToArray();
  }
}

public class Should_Emit_Manifest
{
  public static void Given_States_And_Actions()
  {
    (GeneratorDriver driver, Compilation output) = ManifestSources.Run(ManifestSources.Counter);

    string source = ManifestSources.ManifestSource(driver).ShouldNotBeNull();
    source.ShouldContain("[assembly: global::TimeWarp.State.StateManifestAttribute(global::Manifest.Tests.TimeWarpStateManifest.Version, global::Manifest.Tests.TimeWarpStateManifest.Json)]");
    source.ShouldContain("internal static class TimeWarpStateManifest");
    source.ShouldContain("public const int Version = 1;");
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_States_The_Json_Is_Exact()
  {
    (GeneratorDriver driver, Compilation output) = ManifestSources.Run(ManifestSources.Counter);
    GeneratorTestHost.ShouldCompile(output);

    INamedTypeSymbol manifest = output.GetTypeByMetadataName("Manifest.Tests.TimeWarpStateManifest").ShouldNotBeNull();
    IFieldSymbol json = manifest.GetMembers("Json").OfType<IFieldSymbol>().Single();
    json.ConstantValue.ShouldBe(ManifestSources.ExpectedJson);
    ManifestSources.ManifestSource(driver).ShouldNotBeNull();
  }

  public static void Given_No_State_Emits_Nothing()
  {
    const string Source =
      """
      namespace App;

      public sealed class Plain { }
      """;

    (GeneratorDriver driver, _) = ManifestSources.Run(Source);
    ManifestSources.ManifestSource(driver).ShouldBeNull();
  }

  public static void Given_No_TimeWarp_State_Reference_Emits_Nothing()
  {
    CSharpCompilation compilation = CSharpCompilation.Create
    (
      assemblyName: "Manifest.Bare",
      syntaxTrees: [GeneratorTestHost.Parse("namespace App { public interface IState { } public sealed class S : IState { } }", "Host.cs")],
      references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    );

    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());
    ManifestSources.ManifestSource(driver).ShouldBeNull();
  }
}

public class Should_Read_Manifest
{
  public static void Given_Parse()
  {
    StateManifest manifest = StateManifest.Parse(ManifestSources.ExpectedJson);

    manifest.Version.ShouldBe(StateManifest.CurrentVersion);
    manifest.AssemblyName.ShouldBe("Manifest.Tests");
    manifest.States.Select(state => state.Name).ShouldBe(["CounterState", "EmptyState"]);

    StateManifestState counter = manifest.States[0];
    counter.Type.ShouldBe("App.Counter.CounterState");
    counter.Persistent.ShouldBe("LocalStorage");
    counter.CloneKind.ShouldBe("generated");
    counter.Actions.Count.ShouldBe(2);

    StateManifestAction add = counter.Actions[0];
    add.Name.ShouldBe("AddActionSet.Action");
    add.Method.ShouldBe("Add");
    add.CatalogName.ShouldBe("Counter.Add");
    add.Tracked.ShouldBeTrue();
    add.Parameters.ShouldBe([new StateManifestParameter("Amount", "int", null, true), new StateManifestParameter("Note", "string", "\"x\"", false)]);

    manifest.States[1].CloneKind.ShouldBe("cloneable");
    manifest.States[1].Actions.ShouldBeEmpty();
  }

  public static void Given_Newer_Version_With_Unknown_Fields()
  {
    StateManifest manifest = StateManifest.Parse
    (
      """{"version":2,"assembly":"A","extra":1,"states":[{"type":"A.S","name":"S","future":{},"actions":[{"type":"A.S+XActionSet+Action","name":"XActionSet.Action"}]}]}"""
    );

    manifest.Version.ShouldBe(2);
    StateManifestAction action = manifest.States.ShouldHaveSingleItem().Actions.ShouldHaveSingleItem();
    action.Method.ShouldBeNull();
    action.Parameters.ShouldBeEmpty();
    action.Tracked.ShouldBeFalse();
  }

  public static void Given_Not_A_Manifest_Throws()
  {
    Should.Throw<FormatException>(() => StateManifest.Parse("[]"));
    Should.Throw<FormatException>(() => StateManifest.Parse("""{"states":[]}"""));
  }

  public static void Given_Loaded_Assembly()
  {
    (_, Compilation output) = ManifestSources.Run(ManifestSources.Counter);
    byte[] image = ManifestSources.Emit(output);

    AssemblyLoadContext context = new("manifest-round-trip", isCollectible: true);
    try
    {
      Assembly loaded = context.LoadFromStream(new MemoryStream(image));
      StateManifest manifest = StateManifest.FromAssembly(loaded).ShouldNotBeNull();
      manifest.Json.ShouldBe(ManifestSources.ExpectedJson);
      manifest.States.Select(state => state.Type).ShouldBe(["App.Counter.CounterState", "App.Counter.EmptyState"]);
      loaded.GetType(manifest.States[0].Type).ShouldNotBeNull();
      loaded.GetType(manifest.States[0].Actions[0].Type).ShouldNotBeNull();
    }
    finally
    {
      context.Unload();
    }
  }

  public static void Given_Assembly_Without_States() =>
    StateManifest.FromAssembly(typeof(Should_Read_Manifest).Assembly).ShouldBeNull();

  // What a consumer analyzer (for example an architecture coverage check) does: read the attribute from metadata.
  public static void Given_Referenced_Assembly_Metadata()
  {
    (_, Compilation output) = ManifestSources.Run(ManifestSources.Counter);
    MetadataReference reference = MetadataReference.CreateFromImage(ManifestSources.Emit(output));

    CSharpCompilation consumer = GeneratorTestHost.CreateCompilation("namespace Consumer; public static class C { }", "Consumer")
      .AddReferences(reference);
    IAssemblySymbol referenced = (IAssemblySymbol)consumer.GetAssemblyOrModuleSymbol(reference).ShouldNotBeNull();

    AttributeData attribute = referenced.GetAttributes()
      .Single(data => data.AttributeClass?.ToDisplayString() == ManifestEmitter.AttributeMetadataName);
    attribute.ConstructorArguments[0].Value.ShouldBe(StateManifest.CurrentVersion);
    string json = (string)attribute.ConstructorArguments[1].Value!;
    StateManifest.Parse(json).States.Select(state => state.Name).ShouldBe(["CounterState", "EmptyState"]);
  }
}
