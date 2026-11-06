#region Purpose
// The one State graph builder: merges the extractors' facts and the clone plan into a StateGraph.
#endregion

#region Design
// The only builder (task 140 lesson: do not copy Mediator's two graph builders). Analyzers do not build a graph.
// They share the per-symbol rules instead (facts/, linked as source).
// Build is pure and deterministic:
// - Dispatch methods and persistence loads keep fact order (syntax tree, then node order). Each one is emitted per item.
// - Catalog entries keep fact order. CatalogEmitter sorts them by name, as the old generator did.
// - Diagnostics: action failures, then persistence failures, in fact order. Clone diagnostics stay in the clone plan.
// - Manifest: states from StateFacts plus any state an action names (an abstract state's action sets still show).
//   Persistence attaches only to states already present. A [PersistentState] class that is not a state is no state.
//   States and actions are sorted by type name, ordinal. Null when the manifest attribute is unavailable or there is
//   no state, so nothing is emitted.
// A state declared twice (partial declarations each with a base list) is reported once by the extractor. Build
// still de-duplicates by key, defensively.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class StateGraphBuilder
{
  public static StateGraph Build
  (
    ImmutableArray<ActionFact> actions,
    ImmutableArray<PersistenceFact> persistence,
    ImmutableArray<StateFact> states,
    AssemblyFact assembly,
    ClonePlanResult clone
  )
  {
    string namespaceName = CatalogEmitter.GetNamespace(assembly.AssemblyName);

    List<GeneratorDiagnosticInfo> diagnostics = [];
    diagnostics.AddRange(actions.Where(fact => fact.Diagnostic is not null).Select(fact => fact.Diagnostic!));
    diagnostics.AddRange(persistence.Where(fact => fact.Diagnostic is not null).Select(fact => fact.Diagnostic!));

    return new StateGraph
    (
      DispatchMethods: new EquatableArray<ActionSetMethodModel>(actions.Where(fact => fact.Method is not null).Select(fact => fact.Method!)),
      PersistenceLoads: new EquatableArray<PersistenceModel>(persistence.Where(fact => fact.Model is not null).Select(fact => fact.Model!)),
      Catalog: new CatalogModel(namespaceName, new EquatableArray<CatalogEntryModel>(actions.Where(fact => fact.Catalog is not null).Select(fact => fact.Catalog!))),
      Manifest: assembly.ManifestAvailable ? BuildManifest(namespaceName, assembly.AssemblyName ?? string.Empty, actions, persistence, states) : null,
      ClonePlan: clone,
      Diagnostics: new EquatableArray<GeneratorDiagnosticInfo>(diagnostics)
    );
  }

  private static ManifestModel? BuildManifest
  (
    string namespaceName,
    string assemblyName,
    ImmutableArray<ActionFact> actions,
    ImmutableArray<PersistenceFact> persistence,
    ImmutableArray<StateFact> states
  )
  {
    Dictionary<string, (string Name, string Clone)> stateRows = new(StringComparer.Ordinal);
    foreach (StateFact state in states)
    {
      if (!stateRows.ContainsKey(state.Key)) stateRows.Add(state.Key, (state.Name, state.CloneKind));
    }

    Dictionary<string, List<ManifestActionModel>> actionRows = new(StringComparer.Ordinal);
    foreach (ActionFact action in actions)
    {
      if (!stateRows.ContainsKey(action.StateKey))
        stateRows.Add(action.StateKey, (SimpleName(action.StateKey), "generated"));

      if (!actionRows.TryGetValue(action.StateKey, out List<ManifestActionModel>? rows))
        actionRows.Add(action.StateKey, rows = []);
      rows.Add(action.Manifest);
    }

    if (stateRows.Count == 0) return null;

    Dictionary<string, string?> persistentRows = new(StringComparer.Ordinal);
    foreach (PersistenceFact fact in persistence)
    {
      if (stateRows.ContainsKey(fact.StateKey) && !persistentRows.ContainsKey(fact.StateKey))
        persistentRows.Add(fact.StateKey, fact.Method ?? "unknown");
    }

    List<ManifestStateModel> rowsOut = stateRows
      .OrderBy(pair => pair.Key, StringComparer.Ordinal)
      .Select(pair => new ManifestStateModel
      (
        pair.Key,
        pair.Value.Name,
        persistentRows.TryGetValue(pair.Key, out string? method) ? method : null,
        pair.Value.Clone,
        new EquatableArray<ManifestActionModel>
        (
          actionRows.TryGetValue(pair.Key, out List<ManifestActionModel>? rows)
            ? rows.OrderBy(row => row.Type, StringComparer.Ordinal)
            : []
        )
      ))
      .ToList();

    return new ManifestModel(namespaceName, assemblyName, new EquatableArray<ManifestStateModel>(rowsOut));
  }

  private static string SimpleName(string key)
  {
    int index = Math.Max(key.LastIndexOf('.'), key.LastIndexOf('+'));
    return index < 0 ? key : key.Substring(index + 1);
  }
}
