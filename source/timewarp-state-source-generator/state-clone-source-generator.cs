#region Purpose
// Emits AOT-safe clone methods for State<T> and [GenerateClone] types, plus a module initializer that registers states.
#endregion

#region Design
// External cloners, not partial members, so existing states stay non-partial. ICloneable is skipped and wins at
// runtime. One compilation produces one source file. TWSG002 is an error: there is no reflection fallback.
// The plan needs the whole compilation (states, their member graphs, and the implementations a dispatch switches on),
// so the only input is CompilationProvider. The generator reruns on every compilation change; the closed-generic
// syntax scan is limited to the names of [GenerateClone] generic definitions.
#endregion

namespace TimeWarp.State.SourceGenerator;

[Generator]
public sealed class StateCloneSourceGenerator : IIncrementalGenerator
{
  public const string DiagnosticId = "TWSG002";

  internal static readonly DiagnosticDescriptor UnsupportedRule =
    new
    (
      DiagnosticId,
      title: "State clone cannot be generated",
      messageFormat: "Cannot generate a clone for '{0}'{2} because {1}. Implement ICloneable on '{0}', mark the member [CloneShared] to copy it by reference, or change the unsupported member.",
      category: "Cloning",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The state clone source generator could not emit a clone. Implement ICloneable, mark the member [CloneShared] to copy it by reference, or change the member named in the message."
    );

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterSourceOutput(
      context.CompilationProvider,
      static (sourceContext, compilation) => Execute(compilation, sourceContext));
  }

  private static void Execute(Compilation compilation, SourceProductionContext sourceContext)
  {
    if (compilation.GetTypeByMetadataName("TimeWarp.State.State`1") is null)
    {
      return;
    }

    StateClonePlanner planner = new(compilation);
    string? source = planner.Plan(sourceContext);
    if (source is not { Length: > 0 })
    {
      return;
    }

    sourceContext.AddSource("TimeWarpStateClones.g.cs", SourceText.From(source, Encoding.UTF8));
  }
}
