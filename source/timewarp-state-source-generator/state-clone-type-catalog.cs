#region Purpose
// Resolves planner metadata names once per compilation.
#endregion

#region Design
// GetTypeByMetadataName walks referenced assemblies. MatchConcrete and IsShared used to call it dozens of times
// per type. The catalog resolves each name when it is constructed and answers later questions by symbol equality.
// Namespace checks compare INamespaceSymbol values from the same compilation, not display strings.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class StateCloneTypeCatalog
{
  private readonly Dictionary<string, INamedTypeSymbol?> ByMetadataName = new(StringComparer.Ordinal);
  private readonly Compilation Compilation;

  public StateCloneTypeCatalog(Compilation compilation)
  {
    Compilation = compilation;
    StateType = Resolve("TimeWarp.State.State`1");
    ICloneable = Resolve("System.ICloneable");
    SystemNamespace = compilation.GetTypeByMetadataName("System.Object")?.ContainingNamespace;
    ValueTuple = Resolve("System.ValueTuple");
    ThreadingNamespace = compilation.GetTypeByMetadataName("System.Threading.Thread")?.ContainingNamespace;
    TasksNamespace = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")?.ContainingNamespace;
    JsonIgnoreCondition = Resolve("System.Text.Json.Serialization.JsonIgnoreCondition");
    ReferenceAssemblyAttribute = Resolve("System.Runtime.CompilerServices.ReferenceAssemblyAttribute");
    NotTest = Resolve("TimeWarp.Fixie.NotTest");
    NotTestAttribute = Resolve("TimeWarp.Fixie.NotTestAttribute");

    NamedTypes = new Dictionary<string, INamedTypeSymbol?>(StringComparer.Ordinal)
    {
      ["System.DateTimeOffset"] = Resolve("System.DateTimeOffset"),
      ["System.TimeSpan"] = Resolve("System.TimeSpan"),
      ["System.DateOnly"] = Resolve("System.DateOnly"),
      ["System.TimeOnly"] = Resolve("System.TimeOnly"),
      ["System.Guid"] = Resolve("System.Guid"),
      ["System.Half"] = Resolve("System.Half"),
      ["System.Int128"] = Resolve("System.Int128"),
      ["System.UInt128"] = Resolve("System.UInt128"),
      ["System.Uri"] = Resolve("System.Uri"),
      ["System.Version"] = Resolve("System.Version"),
      ["System.DBNull"] = Resolve("System.DBNull"),
      ["System.TimeZoneInfo"] = Resolve("System.TimeZoneInfo"),
      ["System.Numerics.BigInteger"] = Resolve("System.Numerics.BigInteger")
    };

    AssignableTypes =
    [
      Resolve("System.Delegate"),
      Resolve("System.Reflection.MemberInfo"),
      Resolve("System.Reflection.Assembly"),
      Resolve("System.Reflection.Module"),
      Resolve("System.Reflection.ParameterInfo"),
      Resolve("System.Globalization.CultureInfo"),
      Resolve("System.Text.RegularExpressions.Regex"),
      Resolve("System.Text.Encoding"),
      Resolve("System.IServiceProvider")
    ];

    NonGenericEqualityComparer = Resolve("System.Collections.IEqualityComparer");
    NonGenericComparer = Resolve("System.Collections.IComparer");
    GenericEqualityComparer = Resolve("System.Collections.Generic.IEqualityComparer`1");
    GenericComparer = Resolve("System.Collections.Generic.IComparer`1");
    Enumerable = Resolve("System.Collections.Generic.IEnumerable`1");
    Dictionary = Resolve("System.Collections.Generic.IDictionary`2");
    ReadOnlyDictionary = Resolve("System.Collections.Generic.IReadOnlyDictionary`2");

    InterfaceKinds = BuildInterfaceKinds();
    ConcreteKinds = BuildConcreteKinds();
  }

  public INamedTypeSymbol? StateType { get; }
  public INamedTypeSymbol? ICloneable { get; }
  public INamespaceSymbol? SystemNamespace { get; }
  public INamedTypeSymbol? ValueTuple { get; }
  public INamespaceSymbol? ThreadingNamespace { get; }
  public INamespaceSymbol? TasksNamespace { get; }
  public INamedTypeSymbol? JsonIgnoreCondition { get; }
  public INamedTypeSymbol? ReferenceAssemblyAttribute { get; }
  public INamedTypeSymbol? NotTest { get; }
  public INamedTypeSymbol? NotTestAttribute { get; }
  public INamedTypeSymbol? NonGenericEqualityComparer { get; }
  public INamedTypeSymbol? NonGenericComparer { get; }
  public INamedTypeSymbol? GenericEqualityComparer { get; }
  public INamedTypeSymbol? GenericComparer { get; }
  public INamedTypeSymbol? Enumerable { get; }
  public INamedTypeSymbol? Dictionary { get; }
  public INamedTypeSymbol? ReadOnlyDictionary { get; }
  public Dictionary<string, INamedTypeSymbol?> NamedTypes { get; }
  public INamedTypeSymbol?[] AssignableTypes { get; }
  public (INamedTypeSymbol? Definition, CollectionKind Kind)[] InterfaceKinds { get; }
  public (INamedTypeSymbol? Definition, CollectionKind Kind, string MetadataName)[] ConcreteKinds { get; }

  public INamedTypeSymbol? Resolve(string metadataName)
  {
    if (ByMetadataName.TryGetValue(metadataName, out INamedTypeSymbol? cached))
    {
      return cached;
    }

    INamedTypeSymbol? symbol = Compilation.GetTypeByMetadataName(metadataName);
    ByMetadataName.Add(metadataName, symbol);
    return symbol;
  }

  public bool SymbolEquals(INamedTypeSymbol type, string metadataName)
  {
    INamedTypeSymbol? definition = Resolve(metadataName);
    return definition is not null && SymbolEqualityComparer.Default.Equals(type, definition);
  }

  public bool Named(ITypeSymbol type, string metadataName)
  {
    INamedTypeSymbol? definition = Resolve(metadataName);
    return definition is not null && SymbolEqualityComparer.Default.Equals(type, definition);
  }

  public bool ImplementsICloneable(ITypeSymbol type)
  {
    if (ICloneable is null)
    {
      return false;
    }

    foreach (INamedTypeSymbol implemented in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(implemented, ICloneable)
          || SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, ICloneable))
      {
        return true;
      }
    }

    return false;
  }

  // IsTupleType is the named-element form, such as (string Name, Item It). TupleUnderlyingType
  // is the System.ValueTuple`N the planner already normalizes away before classification.
  public bool IsValueTuple(INamedTypeSymbol type)
  {
    if (type.IsTupleType)
    {
      return true;
    }

    return type.IsValueType
      && type.Arity > 0
      && type.Name == "ValueTuple"
      && SymbolEqualityComparer.Default.Equals(type.ContainingNamespace, SystemNamespace);
  }

  public bool InThreadingNamespace(ITypeSymbol type) =>
    SymbolEqualityComparer.Default.Equals(type.ContainingNamespace, ThreadingNamespace)
    || SymbolEqualityComparer.Default.Equals(type.ContainingNamespace, TasksNamespace);

  private (INamedTypeSymbol? Definition, CollectionKind Kind)[] BuildInterfaceKinds() =>
  [
    (Resolve("System.Collections.Generic.IDictionary`2"), CollectionKind.DictionaryInterface),
    (Resolve("System.Collections.Generic.IReadOnlyDictionary`2"), CollectionKind.DictionaryInterface),
    (Resolve("System.Collections.Generic.IList`1"), CollectionKind.EnumerableInterface),
    (Resolve("System.Collections.Generic.ICollection`1"), CollectionKind.EnumerableInterface),
    (Resolve("System.Collections.Generic.IReadOnlyList`1"), CollectionKind.EnumerableInterface),
    (Resolve("System.Collections.Generic.IReadOnlyCollection`1"), CollectionKind.EnumerableInterface),
    (Resolve("System.Collections.Generic.IEnumerable`1"), CollectionKind.EnumerableInterface),
    (Resolve("System.Collections.Generic.ISet`1"), CollectionKind.EnumerableInterface),
    (Resolve("System.Collections.Generic.IReadOnlySet`1"), CollectionKind.EnumerableInterface)
  ];

  private (INamedTypeSymbol? Definition, CollectionKind Kind, string MetadataName)[] BuildConcreteKinds() =>
  [
    Entry("System.Collections.Generic.List`1", CollectionKind.List),
    Entry("System.Collections.Generic.Dictionary`2", CollectionKind.Dictionary),
    Entry("System.Collections.Generic.HashSet`1", CollectionKind.HashSet),
    Entry("System.Collections.Generic.SortedSet`1", CollectionKind.SortedSet),
    Entry("System.Collections.Generic.SortedDictionary`2", CollectionKind.SortedDictionary),
    Entry("System.Collections.Generic.SortedList`2", CollectionKind.SortedList),
    Entry("System.Collections.Generic.LinkedList`1", CollectionKind.LinkedList),
    Entry("System.Collections.Generic.Stack`1", CollectionKind.Stack),
    Entry("System.Collections.Generic.Queue`1", CollectionKind.Queue),
    Entry("System.Collections.Concurrent.ConcurrentDictionary`2", CollectionKind.ConcurrentDictionary),
    Entry("System.Collections.ObjectModel.ObservableCollection`1", CollectionKind.ObservableCollection),
    Entry("System.Collections.ObjectModel.Collection`1", CollectionKind.Collection),
    Entry("System.Collections.ObjectModel.ReadOnlyCollection`1", CollectionKind.ReadOnlyCollection),
    Entry("System.Collections.ObjectModel.ReadOnlyDictionary`2", CollectionKind.ReadOnlyDictionary),
    Entry("System.Collections.Immutable.ImmutableArray`1", CollectionKind.ImmutableArray),
    Entry("System.Collections.Immutable.ImmutableList`1", CollectionKind.ImmutableList),
    Entry("System.Collections.Immutable.ImmutableDictionary`2", CollectionKind.ImmutableDictionary),
    Entry("System.Collections.Immutable.ImmutableSortedDictionary`2", CollectionKind.ImmutableSortedDictionary),
    Entry("System.Collections.Immutable.ImmutableHashSet`1", CollectionKind.ImmutableHashSet),
    Entry("System.Collections.Immutable.ImmutableSortedSet`1", CollectionKind.ImmutableSortedSet),
    Entry("System.Collections.Immutable.ImmutableQueue`1", CollectionKind.ImmutableQueue),
    Entry("System.Collections.Immutable.ImmutableStack`1", CollectionKind.ImmutableStack),
    Entry("System.Collections.Frozen.FrozenDictionary`2", CollectionKind.FrozenDictionary),
    Entry("System.Collections.Frozen.FrozenSet`1", CollectionKind.FrozenSet)
  ];

  private (INamedTypeSymbol? Definition, CollectionKind Kind, string MetadataName) Entry(string metadataName, CollectionKind kind) =>
    (Resolve(metadataName), kind, metadataName);
}
