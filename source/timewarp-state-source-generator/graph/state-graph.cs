#region Purpose
// The equatable State graph and the facts it is built from. Every emitter reads the graph; nothing in it holds a
// symbol, a syntax node or a Location.
#endregion

#region Design
// Facts are what the extractors produce per syntax node (ActionFact, PersistenceFact, StateFact) or per compilation
// (AssemblyFact, ClonePlanResult). StateGraphBuilder.Build merges them once into a StateGraph. Emitters are pure
// functions of graph parts, so the generator can project each part into its own pipeline step and the incremental
// driver compares parts by value: an edit that changes one action re-emits that action's method, not every file.
// Success models carry no location (task 141): an edit above an action in the same file leaves its fact equal.
// Failures carry an equatable LocationInfo, re-joined to the current tree when reported.
// ClonePlanResult keeps Diagnostic values. Their locations point into the compilation the plan came from, and
// Diagnostic equality compares descriptor, location and arguments, so an equal plan compares equal.
// StateKey is StateSymbolFacts.FullMetadataName, the runtime Type.FullName shape, so the manifest can join facts by
// state and DevTools can join manifest entries to live types.
#endregion

namespace TimeWarp.State.SourceGenerator;

/// <summary>
/// One nested <c>*ActionSet.Action</c>: its dispatch method or failure, catalog entry, manifest entry, and the
/// <c>[Reducer]</c> handler when one is emitted.
/// </summary>
internal sealed record ActionFact
(
  string StateKey,
  ActionSetMethodModel? Method,
  GeneratorDiagnosticInfo? Diagnostic,
  CatalogEntryModel? Catalog,
  ManifestActionModel Manifest,
  ReducerHandlerModel? Handler
);

/// <summary>One <c>[PersistentState]</c> class declaration: its Load model or failure, and the persistence method.</summary>
internal sealed record PersistenceFact
(
  PersistenceModel? Model,
  GeneratorDiagnosticInfo? Diagnostic,
  string StateKey,
  string? Method
);

/// <summary>One concrete state type (implements IState, not abstract).</summary>
internal sealed record StateFact
(
  string Key,
  string Name,
  string CloneKind
);

/// <summary>Per-compilation facts: the assembly name and whether TimeWarp.State has the manifest attribute.</summary>
internal sealed record AssemblyFact
(
  string? AssemblyName,
  bool ManifestAvailable
);

internal sealed record ClonePlanResult(string? Source, EquatableArray<Diagnostic> Diagnostics)
{
  public static readonly ClonePlanResult Empty = new(null, new EquatableArray<Diagnostic>([]));
}

internal sealed record CatalogModel
(
  string NamespaceName,
  EquatableArray<CatalogEntryModel> Entries
);

internal sealed record ManifestModel
(
  string NamespaceName,
  string AssemblyName,
  EquatableArray<ManifestStateModel> States
);

internal sealed record ManifestStateModel
(
  string Type,
  string Name,
  string? Persistent,
  string CloneKind,
  EquatableArray<ManifestActionModel> Actions
);

internal sealed record ManifestActionModel
(
  string Type,
  string Name,
  string? Method,
  EquatableArray<ManifestParameterModel> Parameters,
  string? Catalog,
  bool Tracked
);

internal sealed record ManifestParameterModel
(
  string Name,
  string Type,
  string? Default,
  bool Required
);

/// <summary>Everything the State emitters need, built once per compilation by <see cref="StateGraphBuilder"/>.</summary>
internal sealed record StateGraph
(
  EquatableArray<ActionSetMethodModel> DispatchMethods,
  EquatableArray<ReducerHandlerModel> ReducerHandlers,
  EquatableArray<PersistenceModel> PersistenceLoads,
  CatalogModel Catalog,
  ManifestModel? Manifest,
  ClonePlanResult ClonePlan,
  EquatableArray<GeneratorDiagnosticInfo> Diagnostics
);
