#region Purpose
// Plans per-type clone methods for StateCloneSourceGenerator.
#endregion

#region Design
// The planner is partial so each concern lives in its own file. This file owns the compilation, the slot table,
// and Plan. Type names are resolved once per compilation in StateCloneTypeCatalog. Subtype lists are cached per
// target definition. Generated clone text stays the same as the single-file planner.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{
  private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat;
  private const string UnsafeAccessor = "global::System.Runtime.CompilerServices.UnsafeAccessor";
  private const string UnsafeAccessorKind = "global::System.Runtime.CompilerServices.UnsafeAccessorKind";
  private const string GenericList = "global::System.Collections.Generic.List";
  private const string GenericDictionary = "global::System.Collections.Generic.Dictionary";

  private readonly Compilation Compilation;
  private readonly StateCloneTypeCatalog Catalog;
  private readonly Dictionary<ITypeSymbol, Slot> Slots = new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, Holder> Holders = new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, string?> MetadataProblems = new(SymbolEqualityComparer.Default);
  private readonly HashSet<string> UsedNames = new(StringComparer.Ordinal);
  private readonly HashSet<string> GenericCloneNames = new(StringComparer.Ordinal);
  private readonly List<INamedTypeSymbol> SourceTypes = [];
  private readonly Dictionary<IAssemblySymbol, List<INamedTypeSymbol>> AssemblyTypes = new(SymbolEqualityComparer.Default);
  private readonly List<Slot> Roots = [];
  private readonly List<INamedTypeSymbol> ICloneableStates = [];
  private readonly HashSet<INamedTypeSymbol> ICloneableStateSet = new(SymbolEqualityComparer.Default);
  private readonly List<INamedTypeSymbol> OpenGenericICloneableStates = [];
  private readonly Dictionary<INamedTypeSymbol, (List<INamedTypeSymbol> Implementations, List<INamedTypeSymbol> Derived)> ClosedSubtypeCache =
    new(SymbolEqualityComparer.Default);
  private readonly Dictionary<IAssemblySymbol, List<IAssemblySymbol>> AssembliesReferencing = new(SymbolEqualityComparer.Default);
  private readonly INamedTypeSymbol? StateType;
  private Compilation? FullCompilation;
  private CancellationToken PlanningCancellation;
  private int? JsonIgnoreAlways;
  private bool JsonIgnoreAlwaysResolved;

  public StateClonePlanner(Compilation compilation)
  {
    Compilation = compilation;
    Catalog = new StateCloneTypeCatalog(compilation);
    StateType = Catalog.StateType;
  }

  public string? Plan(SourceProductionContext sourceContext) =>
    Plan(sourceContext, sourceContext.CancellationToken);

  internal string? Plan(SourceProductionContext sourceContext, CancellationToken cancellationToken)
  {
    PlanningCancellation = cancellationToken;
    PlanningCancellation.ThrowIfCancellationRequested();
    if (StateType is null)
    {
      return null;
    }

    CollectTypes(Compilation.Assembly.GlobalNamespace, SourceTypes);
    foreach (INamedTypeSymbol type in SourceTypes)
    {
      PlanningCancellation.ThrowIfCancellationRequested();
      Consider(type);
    }

    CollectClosedGenerateCloneTypes();
    CollectClosedICloneableStates();
    PlanningCancellation.ThrowIfCancellationRequested();
    PropagateErrors();
    Report(sourceContext);
    PlanningCancellation.ThrowIfCancellationRequested();
    MaterializeDispatches();
    return Emit();
  }
}
