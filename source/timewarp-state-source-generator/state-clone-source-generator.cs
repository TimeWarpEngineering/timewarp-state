#region Purpose
// Emits AOT-safe clone methods for State<T> and [GenerateClone] types, plus a module initializer that registers states.
#endregion

#region Design
// External cloners, not partial members, so existing states stay non-partial. ICloneable is skipped and wins at
// runtime. The same compilation still emits a module initializer that roots those states' public constructors and
// public methods, and another that registers each nameable state with TryAddTransient<T>.
// One compilation produces one source file. TWSG002 is an error: there is no reflection fallback.
// The plan needs the whole compilation (states, their member graphs, and the implementations a dispatch switches on),
// so the only input is CompilationProvider. The generator reruns on every compilation change. The closed-generic
// syntax scan covers [GenerateClone] definitions and open-generic ICloneable states. The planner stops when the
// step's CancellationToken is cancelled.
// Planning is a named Select over CompilationProvider (TrackingNames.ClonePlan) that returns an equatable
// ClonePlanResult: the source text plus the diagnostics. Planning still reruns on every compilation change (moving it
// off CompilationProvider is Option 1). When the plan is equal, the step is Unchanged and the output step is Cached,
// so an unrelated edit re-adds no source. state-clone-incrementality-tests pins both halves.
// Diagnostics are kept as Diagnostic values: their locations point into the same compilation the plan came from, and
// Diagnostic equality compares descriptor, location and arguments, so an equal plan compares equal.
//
// Analyzer contract:
// | Assumption                                                         | Guard                                       |
// |--------------------------------------------------------------------|---------------------------------------------|
// | a concrete State<T> has ICloneable or a constructor the clone calls | TWS0009 (analyzer); TWSG002 for members    |
// | State<T>'s T is the deriving class                                  | TWS0010 (analyzer)                         |
// | every reachable member can be cloned or is [CloneShared]            | generator: TWSG002                         |
// | public state properties are read-only                               | TWS0012 (analyzer)                         |
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
    IncrementalValueProvider<ClonePlanResult> plan = context.CompilationProvider
      .Select(static (compilation, cancellationToken) => Plan(compilation, cancellationToken))
      .WithTrackingName(TrackingNames.ClonePlan);

    context.RegisterSourceOutput(plan, static (sourceContext, result) => Execute(result, sourceContext));
  }

  private static ClonePlanResult Plan(Compilation compilation, CancellationToken cancellationToken)
  {
    if (compilation.GetTypeByMetadataName("TimeWarp.State.State`1") is null)
    {
      return ClonePlanResult.Empty;
    }

    List<Diagnostic> diagnostics = [];
    StateClonePlanner planner = new(compilation);
    string? source = planner.Plan(diagnostics, cancellationToken);
    return new ClonePlanResult(source is { Length: > 0 } ? source : null, new EquatableArray<Diagnostic>(diagnostics));
  }

  private static void Execute(ClonePlanResult result, SourceProductionContext sourceContext)
  {
    foreach (Diagnostic diagnostic in result.Diagnostics)
    {
      sourceContext.ReportDiagnostic(diagnostic);
    }

    if (result.Source is null)
    {
      return;
    }

    sourceContext.AddSource("TimeWarpStateClones.g.cs", SourceText.From(result.Source, Encoding.UTF8));
  }

  private sealed record ClonePlanResult(string? Source, EquatableArray<Diagnostic> Diagnostics)
  {
    public static readonly ClonePlanResult Empty = new(null, new EquatableArray<Diagnostic>([]));
  }
}
