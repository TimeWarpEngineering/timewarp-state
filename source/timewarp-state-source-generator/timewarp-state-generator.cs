#region Purpose
// The one TimeWarp.State incremental generator: extracts equatable facts, builds the State graph once, and runs the
// emitters for dispatch methods, the action catalog, persistence Load, clones and registration, and the State
// manifest.
#endregion

#region Design
// [Reducer] handlers (task 144, Option 3b) are one more emitter on the same graph. TimeWarp.Mediator dispatches to them
// through GeneratedHandlerAttribute; ReducerAnalyzer (TWS0014) reports a [Reducer] this emitter does not serve.
// Replaces the four pre-142 generators (ActionSet method, action catalog, persistence, clone). Each of those found
// its own inputs. Task 140 Option 1, decided by Steve 2026-11-06.
// Pipeline (every step named in TrackingNames, pinned by generator-incrementality-tests):
//   State.Actions       CreateSyntaxProvider: ActionFactExtractor. One ActionFact per *ActionSet.Action, including
//                       the [Reducer] handler (task 144) from the shared ReducerFacts rule.
//   State.Persistence   CreateSyntaxProvider: PersistenceFactExtractor. One PersistenceFact per [PersistentState].
//   State.States        CreateSyntaxProvider: StateFactExtractor. One StateFact per concrete IState type.
//   State.Assembly      CompilationProvider.Select: assembly name and whether the manifest attribute exists.
//   State.ClonePlan     CompilationProvider.Select: the clone planner, a semantic stage that reruns on every
//                       compilation change. Its equatable result keeps the graph Cached when the plan is equal.
//   State.Graph         the facts collected and combined, then StateGraphBuilder.Build, once.
//   State.Emit.*        projections of the graph, one per emitter. Dispatch, Reducer and Persistence are SelectMany,
//                       so each method, handler or Load is compared and emitted on its own. Catalog, Manifest and Clone are one value
//                       each. Diagnostics are re-joined to the current compilation's trees when reported.
// ForAttributeWithMetadataName is not used. Task 141 measured that it does not defeat caching here. The catalog is
// now one more part of the action fact, so one transform analyzes each action once.
// Output is identical to the old generators for every existing shape (golden-parity-tests). The manifest is new.
// Analyzers stay in their own project and read the same per-symbol rules: facts/ is compiled into both projects as
// linked source. There is one graph builder.
#endregion

namespace TimeWarp.State.SourceGenerator;

[Generator]
public sealed class TimeWarpStateGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<ActionFact> actions = context.SyntaxProvider
      .CreateSyntaxProvider
      (
        predicate: static (node, _) => ActionFactExtractor.IsCandidateSyntax(node),
        transform: static (syntaxContext, cancellationToken) => ActionFactExtractor.Extract(syntaxContext, cancellationToken)
      )
      .Where(static fact => fact is not null)
      .Select(static (fact, _) => fact!)
      .WithTrackingName(TrackingNames.Actions);

    IncrementalValuesProvider<PersistenceFact> persistence = context.SyntaxProvider
      .CreateSyntaxProvider
      (
        predicate: static (node, _) => PersistenceFactExtractor.IsCandidateSyntax(node),
        transform: static (syntaxContext, cancellationToken) => PersistenceFactExtractor.Extract(syntaxContext, cancellationToken)
      )
      .Where(static fact => fact is not null)
      .Select(static (fact, _) => fact!)
      .WithTrackingName(TrackingNames.Persistence);

    IncrementalValuesProvider<StateFact> states = context.SyntaxProvider
      .CreateSyntaxProvider
      (
        predicate: static (node, _) => StateFactExtractor.IsCandidateSyntax(node),
        transform: static (syntaxContext, cancellationToken) => StateFactExtractor.Extract(syntaxContext, cancellationToken)
      )
      .Where(static fact => fact is not null)
      .Select(static (fact, _) => fact!)
      .WithTrackingName(TrackingNames.States);

    IncrementalValueProvider<AssemblyFact> assembly = context.CompilationProvider
      .Select(static (compilation, _) => new AssemblyFact
      (
        compilation.AssemblyName,
        compilation.GetTypeByMetadataName(ManifestEmitter.AttributeMetadataName) is not null
      ))
      .WithTrackingName(TrackingNames.Assembly);

    IncrementalValueProvider<ClonePlanResult> clonePlan = context.CompilationProvider
      .Select(static (compilation, cancellationToken) => CloneEmitter.Plan(compilation, cancellationToken))
      .WithTrackingName(TrackingNames.ClonePlan);

    IncrementalValueProvider<StateGraph> graph = actions.Collect()
      .Combine(persistence.Collect())
      .Combine(states.Collect())
      .Combine(assembly)
      .Combine(clonePlan)
      .Select(static (input, _) => StateGraphBuilder.Build
      (
        input.Left.Left.Left.Left,
        input.Left.Left.Left.Right,
        input.Left.Left.Right,
        input.Left.Right,
        input.Right
      ))
      .WithTrackingName(TrackingNames.Graph);

    IncrementalValuesProvider<ActionSetMethodModel> dispatch = graph
      .SelectMany(static (value, _) => value.DispatchMethods)
      .WithTrackingName(TrackingNames.EmitDispatch);

    IncrementalValuesProvider<ReducerHandlerModel> reducers = graph
      .SelectMany(static (value, _) => value.ReducerHandlers)
      .WithTrackingName(TrackingNames.EmitReducer);

    IncrementalValuesProvider<PersistenceModel> loads = graph
      .SelectMany(static (value, _) => value.PersistenceLoads)
      .WithTrackingName(TrackingNames.EmitPersistence);

    IncrementalValueProvider<CatalogModel> catalog = graph
      .Select(static (value, _) => value.Catalog)
      .WithTrackingName(TrackingNames.EmitCatalog);

    IncrementalValueProvider<ManifestModel?> manifest = graph
      .Select(static (value, _) => value.Manifest)
      .WithTrackingName(TrackingNames.EmitManifest);

    IncrementalValueProvider<ClonePlanResult> clone = graph
      .Select(static (value, _) => value.ClonePlan)
      .WithTrackingName(TrackingNames.EmitClone);

    IncrementalValuesProvider<GeneratorDiagnosticInfo> diagnostics = graph
      .SelectMany(static (value, _) => value.Diagnostics)
      .WithTrackingName(TrackingNames.EmitDiagnostics);

    context.RegisterSourceOutput(dispatch, static (sourceContext, model) => DispatchMethodEmitter.Emit(model, sourceContext));
    context.RegisterSourceOutput(reducers, static (sourceContext, model) => ReducerHandlerEmitter.Emit(model, sourceContext));
    context.RegisterSourceOutput(loads, static (sourceContext, model) => PersistenceEmitter.Emit(model, sourceContext));
    context.RegisterSourceOutput(catalog, static (sourceContext, model) => CatalogEmitter.Emit(model, sourceContext));
    context.RegisterSourceOutput(manifest, static (sourceContext, model) => ManifestEmitter.Emit(model, sourceContext));
    context.RegisterSourceOutput(clone, static (sourceContext, result) => CloneEmitter.Emit(result, sourceContext));
    context.RegisterSourceOutput
    (
      diagnostics.Combine(context.CompilationProvider),
      static (sourceContext, pair) => sourceContext.ReportDiagnostic(pair.Left.ToDiagnostic(pair.Right))
    );
  }
}
