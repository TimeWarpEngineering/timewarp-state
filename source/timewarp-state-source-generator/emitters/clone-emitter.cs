#region Purpose
// The clone stage of the State graph: plans AOT-safe clone methods for State<T> and [GenerateClone] types plus the
// module initializers that register states, and emits the planned file with its TWSG002 diagnostics.
#endregion

#region Design
// External cloners, not partial members, so existing states stay non-partial. ICloneable is skipped and wins at
// runtime. The same compilation still emits a module initializer that roots those states' public constructors and
// public methods, and another that registers each nameable state with TryAddTransient<T>. One compilation produces
// one source file. TWSG002 is an error, because there is no reflection fallback.
// The planner stays a semantic stage (task 140, Option 1). It needs the whole compilation: states, their member graphs,
// and the implementations a dispatch switches on. So Plan runs as a named CompilationProvider.Select
// (TrackingNames.ClonePlan) and reruns on every compilation change. It returns an equatable ClonePlanResult. When the
// plan is equal, the step is Unchanged, the State graph stays Cached, and no source is added again. DI registration
// stays in the clone file: moving it is not needed for parity and would change generated text.
// The planner stops when the step's CancellationToken is cancelled.
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

internal static class CloneEmitter
{
  public const string HintName = "TimeWarpStateClones.g.cs";

  public static ClonePlanResult Plan(Compilation compilation, CancellationToken cancellationToken)
  {
    if (StateSymbolFacts.GetTimeWarpStateType(compilation) is null)
    {
      return ClonePlanResult.Empty;
    }

    List<Diagnostic> diagnostics = [];
    StateClonePlanner planner = new(compilation);
    string? source = planner.Plan(diagnostics, cancellationToken);
    return new ClonePlanResult(source is { Length: > 0 } ? source : null, new EquatableArray<Diagnostic>(diagnostics));
  }

  public static void Emit(ClonePlanResult result, SourceProductionContext sourceContext)
  {
    foreach (Diagnostic diagnostic in result.Diagnostics)
    {
      sourceContext.ReportDiagnostic(diagnostic);
    }

    if (result.Source is null)
    {
      return;
    }

    sourceContext.AddSource(HintName, SourceText.From(result.Source, Encoding.UTF8));
  }
}
