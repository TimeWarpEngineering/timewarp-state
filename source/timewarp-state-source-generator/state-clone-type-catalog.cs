#region Purpose
// Resolves planner metadata names once per compilation.
#endregion

#region Design
// GetTypeByMetadataName walks referenced assemblies. Collection and assignable-type tables resolve each name
// at construction and compare those symbols. Cross-assembly checks walk Name and ContainingNamespace instead.
// Namespace symbols belong to one module, so equality against Thread's namespace misses CancellationTokenSource
// in the reference pack. A null or polyfill result from GetTypeByMetadataName would also miss ICloneable,
// ReferenceAssemblyAttribute, and the shared BCL names. An empty metadata name does not call GetTypeByMetadataName.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class StateCloneTypeCatalog
{
  private static readonly string[] SharedMetadataNames =
  [
    "System.DateTimeOffset",
    "System.TimeSpan",
    "System.DateOnly",
    "System.TimeOnly",
    "System.Guid",
    "System.Half",
    "System.Int128",
    "System.UInt128",
    "System.Uri",
    "System.Version",
    "System.DBNull",
    "System.TimeZoneInfo",
    "System.Numerics.BigInteger"
  ];

  private readonly Dictionary<string, INamedTypeSymbol?> ByMetadataName = new(StringComparer.Ordinal);
  private readonly Compilation Compilation;

  public StateCloneTypeCatalog(Compilation compilation)
  {
    Compilation = compilation;
    StateType = Resolve("TimeWarp.State.State`1");
    JsonIgnoreCondition = Resolve("System.Text.Json.Serialization.JsonIgnoreCondition");
    NotTest = Resolve("TimeWarp.Fixie.NotTest");
    NotTestAttribute = Resolve("TimeWarp.Fixie.NotTestAttribute");

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
  public INamedTypeSymbol? JsonIgnoreCondition { get; }
  public INamedTypeSymbol? NotTest { get; }
  public INamedTypeSymbol? NotTestAttribute { get; }
  public INamedTypeSymbol? NonGenericEqualityComparer { get; }
  public INamedTypeSymbol? NonGenericComparer { get; }
  public INamedTypeSymbol? GenericEqualityComparer { get; }
  public INamedTypeSymbol? GenericComparer { get; }
  public INamedTypeSymbol? Enumerable { get; }
  public INamedTypeSymbol? Dictionary { get; }
  public INamedTypeSymbol? ReadOnlyDictionary { get; }
  public INamedTypeSymbol?[] AssignableTypes { get; }
  public (INamedTypeSymbol? Definition, CollectionKind Kind)[] InterfaceKinds { get; }
  public (INamedTypeSymbol? Definition, CollectionKind Kind, string MetadataName)[] ConcreteKinds { get; }

  public INamedTypeSymbol? Resolve(string metadataName)
  {
    if (string.IsNullOrEmpty(metadataName))
    {
      return null;
    }

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
    if (string.IsNullOrEmpty(metadataName))
    {
      return false;
    }

    INamedTypeSymbol? definition = Resolve(metadataName);
    return definition is not null && SymbolEqualityComparer.Default.Equals(type, definition);
  }

  public bool IsSharedNamedType(ITypeSymbol type)
  {
    foreach (string metadataName in SharedMetadataNames)
    {
      if (HasMetadataName(type, metadataName))
      {
        return true;
      }
    }

    return false;
  }

  public static bool HasReferenceAssemblyAttribute(IAssemblySymbol assembly)
  {
    foreach (AttributeData attribute in assembly.GetAttributes())
    {
      if (attribute.AttributeClass is { } attributeClass
          && HasMetadataName(attributeClass, "System.Runtime.CompilerServices.ReferenceAssemblyAttribute"))
      {
        return true;
      }
    }

    return false;
  }

  public bool ImplementsICloneable(ITypeSymbol type)
  {
    foreach (INamedTypeSymbol implemented in type.AllInterfaces)
    {
      if (HasMetadataName(implemented, "System.ICloneable")
          || HasMetadataName(implemented.OriginalDefinition, "System.ICloneable"))
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
      && InNamespace(type.ContainingNamespace, "System");
  }

  public bool InThreadingNamespace(ITypeSymbol type) =>
    InNamespace(type.ContainingNamespace, "System.Threading")
    || InNamespace(type.ContainingNamespace, "System.Threading.Tasks");

  // Non-generic metadata names only. Generic definitions carry an arity the plain name does not.
  public static bool HasMetadataName(ITypeSymbol type, string metadataName)
  {
    if (string.IsNullOrEmpty(metadataName))
    {
      return false;
    }

    int dot = metadataName.LastIndexOf('.');
    string name = dot < 0 ? metadataName : metadataName.Substring(dot + 1);
    if (type.Name != name || type is INamedTypeSymbol { Arity: > 0 })
    {
      return false;
    }

    string dottedNamespace = dot < 0 ? string.Empty : metadataName.Substring(0, dot);
    return InNamespace(type.ContainingNamespace, dottedNamespace);
  }

  public static bool InNamespace(INamespaceSymbol? namespaceSymbol, string dottedNamespace)
  {
    if (namespaceSymbol is null)
    {
      return false;
    }

    if (dottedNamespace.Length == 0)
    {
      return namespaceSymbol.IsGlobalNamespace;
    }

    int end = dottedNamespace.Length;
    INamespaceSymbol current = namespaceSymbol;
    while (end > 0)
    {
      if (current.IsGlobalNamespace)
      {
        return false;
      }

      int start = dottedNamespace.LastIndexOf('.', end - 1);
      int segmentStart = start + 1;
      if (!current.Name.AsSpan().Equals(dottedNamespace.AsSpan(segmentStart, end - segmentStart), StringComparison.Ordinal))
      {
        return false;
      }

      end = start;
      current = current.ContainingNamespace;
    }

    return current.IsGlobalNamespace;
  }

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
