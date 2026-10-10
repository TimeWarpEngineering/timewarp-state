#region Purpose
// Plans per-type clone methods for StateCloneSourceGenerator.
#endregion

#region Design
// Share immutable static types. Clone everything else with constructors plus UnsafeAccessor field writes.
// Accessors live in one holder class per declaring type definition. A generic declaring type gets a generic holder
// whose type parameters mirror the definition (the .NET 9+ generic UnsafeAccessor rule), so fields typed T resolve.
// Collections are rebuilt through their public API. Collection interfaces switch on the BCL and compilation types
// assignable to them and otherwise materialize into List<T>, HashSet<T> or Dictionary<TKey,TValue>.
// A non-sealed class dispatches on the runtime type: known derived types first, the exact cloner when the runtime
// type matches, then ICloneable, else a throw. Known derived types come from this compilation and, for a base declared
// in a non-framework referenced assembly, from that assembly and the referenced assemblies that reference it. A known
// subtype generated code cannot name (private, protected, file-local, another assembly's internal) is TWSG002 unless it
// implements ICloneable, so the throw is left for subtypes the generator never saw. A generic subtype is closed over the
// member type's arguments by unifying its base chain and interfaces with the member type (GD<T> : GB<T> for GB<int>
// dispatches to GD<int>); one whose type parameters are not determined, or whose constraints fail, is TWSG002 too.
// An ICloneable type is cloned by its own Clone(), cast to the declared type, and is never wrapped in that dispatch.
// Types from other assemblies are cloned only when their full field list is known: TimeWarp.State's own types, or an
// implementation assembly inspected with MetadataImportOptions.All. Reference assemblies hide private fields, so
// their types are accepted only when every instance property is an auto-property and every instance method is a
// constructor, an accessor or compiler-generated (records); other classes and managed structs are TWSG002.
// Cycles use a method name assigned before the body exists. Method names carry a stable hash so they never collide.
// Diagnostics are reported once, at the source-located member that cannot be cloned, for types reachable from a root.
// A member marked CloneShared (the field or its property) is assigned from the source and is not walked, so an
// injected service stays the same instance. IgnoreDataMember, NonSerialized and JsonIgnore leave the constructor
// value. CloneShared wins when a member has both.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class StateClonePlanner
{
  private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat;
  private const string UnsafeAccessor = "global::System.Runtime.CompilerServices.UnsafeAccessor";
  private const string UnsafeAccessorKind = "global::System.Runtime.CompilerServices.UnsafeAccessorKind";
  private const string GenericList = "global::System.Collections.Generic.List";
  private const string GenericDictionary = "global::System.Collections.Generic.Dictionary";

  private readonly Compilation Compilation;
  private readonly Dictionary<ITypeSymbol, Slot> Slots = new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, Holder> Holders = new(SymbolEqualityComparer.Default);
  private readonly Dictionary<INamedTypeSymbol, string?> MetadataProblems = new(SymbolEqualityComparer.Default);
  private readonly HashSet<string> UsedNames = new(StringComparer.Ordinal);
  private readonly HashSet<string> GenericCloneNames = new(StringComparer.Ordinal);
  private readonly List<INamedTypeSymbol> SourceTypes = [];
  private readonly Dictionary<IAssemblySymbol, List<INamedTypeSymbol>> AssemblyTypes = new(SymbolEqualityComparer.Default);
  private readonly List<Slot> Roots = [];
  private readonly INamedTypeSymbol? StateType;
  private Compilation? FullCompilation;

  public StateClonePlanner(Compilation compilation)
  {
    Compilation = compilation;
    StateType = compilation.GetTypeByMetadataName("TimeWarp.State.State`1");
  }

  public string? Plan(SourceProductionContext sourceContext)
  {
    if (StateType is null)
    {
      return null;
    }

    CollectTypes(Compilation.Assembly.GlobalNamespace, SourceTypes);
    foreach (INamedTypeSymbol type in SourceTypes)
    {
      Consider(type);
    }

    CollectClosedGenerateCloneTypes();
    PropagateErrors();
    Report(sourceContext);
    MaterializeDispatches();
    return Emit();
  }

  #region Roots

  private static void CollectTypes(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> types)
  {
    foreach (INamespaceSymbol child in namespaceSymbol.GetNamespaceMembers())
    {
      CollectTypes(child, types);
    }

    foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
    {
      CollectNestedTypes(type, types);
    }
  }

  private static void CollectNestedTypes(INamedTypeSymbol type, List<INamedTypeSymbol> types)
  {
    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      CollectNestedTypes(nested, types);
    }

    types.Add(type);
  }

  // Only generic definitions marked [GenerateClone] need closed constructions, so only their names are bound.
  private void CollectClosedGenerateCloneTypes()
  {
    if (GenericCloneNames.Count == 0)
    {
      return;
    }

    foreach (SyntaxTree tree in Compilation.SyntaxTrees)
    {
      SemanticModel? model = null;
      foreach (GenericNameSyntax genericName in tree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>())
      {
        if (!GenericCloneNames.Contains(genericName.Identifier.ValueText))
        {
          continue;
        }

        model ??= Compilation.GetSemanticModel(tree);
        if (model.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol symbol)
        {
          continue;
        }

        if (symbol.IsUnboundGenericType || symbol.TypeArguments.Any(ContainsTypeParameter))
        {
          continue;
        }

        if (HasGenerateClone(symbol.OriginalDefinition))
        {
          Consider(symbol);
        }
      }
    }
  }

  private static bool ContainsTypeParameter(ITypeSymbol type) =>
    type switch
    {
      ITypeParameterSymbol => true,
      IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
      INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter),
      _ => false
    };

  private void Consider(INamedTypeSymbol type)
  {
    if (type.TypeKind is TypeKind.Interface or TypeKind.Delegate or TypeKind.Enum)
    {
      return;
    }

    if (type.IsAbstract || type.SpecialType == SpecialType.System_Object)
    {
      return;
    }

    bool isState = IsState(type);
    if (type.IsGenericType && type.IsDefinition)
    {
      if (HasGenerateClone(type))
      {
        GenericCloneNames.Add(type.Name);
      }

      if (isState && !ImplementsICloneable(type))
      {
        Slot open = GetOrCreate(type);
        open.IsRoot = true;
        Fail(open, "the state is an open generic", LocationOf(type));
        Roots.Add(open);
      }

      return;
    }

    if (isState && ImplementsICloneable(type))
    {
      return;
    }

    if (!isState && !HasGenerateClone(type) && !HasGenerateClone(type.OriginalDefinition))
    {
      return;
    }

    Slot slot = Build(type);
    if (slot.IsRoot)
    {
      slot.RegisterState |= isState;
      return;
    }

    slot.IsRoot = true;
    slot.RegisterState = isState;
    Roots.Add(slot);
  }

  #endregion

  #region Slots

  private Slot Build(ITypeSymbol type)
  {
    type = Normalize(type);
    if (Slots.TryGetValue(type, out Slot? existing))
    {
      return existing;
    }

    Slot slot = GetOrCreate(type);
    Classify(slot, type);
    if (slot.Kind == SlotKind.Clone && !slot.IsDispatch && !slot.IsCloneable && type is INamedTypeSymbol named)
    {
      WrapForRuntimeType(slot, named);
    }

    return slot;
  }

  private static ITypeSymbol Normalize(ITypeSymbol type)
  {
    if (type is INamedTypeSymbol { IsTupleType: true, TupleUnderlyingType: { } underlying })
    {
      type = underlying;
    }

    return type.WithNullableAnnotation(NullableAnnotation.None);
  }

  private Slot GetOrCreate(ITypeSymbol type)
  {
    type = Normalize(type);
    if (Slots.TryGetValue(type, out Slot? existing))
    {
      return existing;
    }

    string fullyQualified = type.ToDisplayString(Format);
    Slot slot = new()
    {
      Type = type,
      FullyQualified = fullyQualified,
      MethodName = UniqueName("Clone_" + Sanitize(type.ToDisplayString()), fullyQualified),
      IsValueType = type.IsValueType,
      Kind = SlotKind.Pending
    };
    Slots.Add(type, slot);
    return slot;
  }

  private void Classify(Slot slot, ITypeSymbol type)
  {
    if (type is IErrorTypeSymbol || type.TypeKind == TypeKind.Error)
    {
      Fail(slot, "the type could not be resolved", LocationOf(type));
      return;
    }

    if (type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer || type is IPointerTypeSymbol)
    {
      Fail(slot, "pointer types are not supported", LocationOf(type));
      return;
    }

    if (IsShared(type))
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    if (type is IArrayTypeSymbol array)
    {
      ClassifyArray(slot, array);
      return;
    }

    if (type is not INamedTypeSymbol named)
    {
      Fail(slot, $"type '{type.ToDisplayString()}' is not supported", LocationOf(type));
      return;
    }

    if (!CanName(named))
    {
      Fail(slot, $"'{named.ToDisplayString()}' is not accessible to generated code", LocationOf(named));
      return;
    }

    if (ImplementsICloneable(named))
    {
      ClassifyCloneable(slot, named);
      return;
    }

    if (named.SpecialType == SpecialType.System_Object)
    {
      Fail(slot, "System.Object can hold any runtime value", LocationOf(named));
      return;
    }

    if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
    {
      ClassifyNullable(slot, named);
      return;
    }

    if (SymbolEquals(named.OriginalDefinition, "System.Collections.Generic.KeyValuePair`2"))
    {
      ClassifyKeyValuePair(slot, named);
      return;
    }

    if (IsValueTuple(named))
    {
      ClassifyTuple(slot, named);
      return;
    }

    if (SymbolEquals(named, "System.Text.StringBuilder"))
    {
      ClassifyStringBuilder(slot, named);
      return;
    }

    CollectionKind collectionKind = CollectionKindOf(named);
    if (collectionKind != CollectionKind.None)
    {
      ClassifyCollection(slot, named, collectionKind);
      return;
    }

    if (named.TypeKind == TypeKind.Interface || named.IsAbstract)
    {
      ClassifyInterface(slot, named);
      return;
    }

    if (named.IsRefLikeType)
    {
      Fail(slot, "ref structs are not supported", LocationOf(named));
      return;
    }

    if (named.IsValueType && named.IsUnmanagedType)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    ClassifyObject(slot, named);
  }

  // A non-sealed class can hold a subtype at run time. The old reflection cloner cloned by runtime type, so the
  // generated cloner dispatches: known derived types, then the exact cloner. A known BCL collection keeps
  // materializing unknown subclasses into itself; any other known subtype the switch cannot name is TWSG002, and a
  // subtype the generator never saw throws.
  private void WrapForRuntimeType(Slot slot, INamedTypeSymbol type)
  {
    if (type.TypeKind != TypeKind.Class || type.IsSealed || type.IsAbstract || type.IsStatic)
    {
      return;
    }

    bool knownCollection = MatchConcrete(type) != CollectionKind.None;
    if (!knownCollection && FindHiddenSubtype(type, implementations: false) is { } hidden)
    {
      Fail(slot, HiddenSubtypeDetail(hidden), LocationOf(hidden));
      return;
    }

    List<INamedTypeSymbol> derived = FindDerived(type);
    if (knownCollection && derived.Count == 0)
    {
      return;
    }

    string exactName = slot.MethodName + "_Exact";
    UsedNames.Add(exactName);
    slot.Exact = new Slot
    {
      Type = type,
      FullyQualified = slot.FullyQualified,
      MethodName = exactName,
      Kind = SlotKind.Clone,
      Statements = slot.Statements
    };
    slot.ExactIsFallback = knownCollection;
    slot.IsDispatch = true;
    slot.Statements = null;
    foreach (INamedTypeSymbol subtype in derived)
    {
      AddCase(slot, subtype, "typed", required: true, guard: false);
    }
  }

  #endregion

  #region Special shapes

  private void ClassifyArray(Slot slot, IArrayTypeSymbol array)
  {
    Slot element = Build(array.ElementType);
    slot.Dependencies.Add(element);
    if (element.Kind == SlotKind.Error)
    {
      Fail(slot, $"array element '{array.ElementType.ToDisplayString()}' cannot be cloned", LocationOf(array), element);
      return;
    }

    slot.Kind = SlotKind.Clone;
    string arrayName = array.ToDisplayString(Format);
    int rank = array.Rank;
    List<string> lines = [];
    lines.Add($"if (map.TryGet(source, out {arrayName}? existing))");
    lines.Add("{");
    lines.Add("  return existing;");
    lines.Add("}");
    for (int dimension = 0; dimension < rank; dimension++)
    {
      lines.Add($"int length{dimension} = source.GetLength({dimension});");
    }

    string lengths = string.Join(", ", Enumerable.Range(0, rank).Select(dimension => $"length{dimension}"));
    lines.Add($"{arrayName} clone = {NewArrayExpression(array, lengths)};");
    lines.Add("map.Add(source, clone);");
    if (element.Kind == SlotKind.Share)
    {
      lines.Add("global::System.Array.Copy(source, clone, source.Length);");
    }
    else
    {
      for (int dimension = 0; dimension < rank; dimension++)
      {
        lines.Add($"{new string(' ', dimension * 2)}for (int index{dimension} = 0; index{dimension} < length{dimension}; index{dimension}++)");
        lines.Add($"{new string(' ', dimension * 2)}{{");
      }

      string indexes = string.Join(", ", Enumerable.Range(0, rank).Select(dimension => $"index{dimension}"));
      string indent = new(' ', rank * 2);
      lines.Add($"{indent}clone[{indexes}] = {CopyExpression(element, $"source[{indexes}]")};");
      for (int dimension = rank - 1; dimension >= 0; dimension--)
      {
        lines.Add($"{new string(' ', dimension * 2)}}}");
      }
    }

    lines.Add("return clone;");
    slot.Statements = lines;
  }

  // new T[n] for a jagged element type T = U[] must be written new U[n][], not new U[][n].
  private static string NewArrayExpression(IArrayTypeSymbol array, string lengths)
  {
    ITypeSymbol element = array.ElementType;
    StringBuilder suffix = new();
    while (element is IArrayTypeSymbol inner)
    {
      suffix.Append('[').Append(new string(',', inner.Rank - 1)).Append(']');
      element = inner.ElementType;
    }

    return $"new {element.ToDisplayString(Format)}[{lengths}]{suffix}";
  }

  // ICloneable.Clone() is the type's own virtual dispatch, so the slot is never wrapped for the runtime type, and the
  // result is cast to the declared type: Clone() may return the base type for a subclass that does not override it.
  private void ClassifyCloneable(Slot slot, INamedTypeSymbol type)
  {
    slot.Kind = SlotKind.Clone;
    slot.IsCloneable = true;
    string name = type.ToDisplayString(Format);
    if (type.IsValueType)
    {
      slot.Statements =
      [
        $"return ({name})((global::System.ICloneable)source).Clone();"
      ];
      return;
    }

    slot.Statements =
    [
      $"if (map.TryGet(source, out {name}? existing))",
      "{",
      "  return existing;",
      "}",
      "object? cloned = ((global::System.ICloneable)source).Clone();",
      "if (cloned is null)",
      "{",
      "  return null!;",
      "}",
      $"{name} clone = ({name})cloned;",
      "map.Add(source, clone);",
      "return clone;"
    ];
  }

  private void ClassifyNullable(Slot slot, INamedTypeSymbol type)
  {
    Slot inner = Build(type.TypeArguments[0]);
    slot.Dependencies.Add(inner);
    if (inner.Kind == SlotKind.Error)
    {
      Fail(slot, $"'{type.TypeArguments[0].ToDisplayString()}' cannot be cloned", LocationOf(type), inner);
      return;
    }

    if (inner.Kind == SlotKind.Share)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.Statements =
    [
      "if (!source.HasValue)",
      "{",
      "  return null;",
      "}",
      $"return {CopyExpression(inner, "source.GetValueOrDefault()")};"
    ];
  }

  private void ClassifyKeyValuePair(Slot slot, INamedTypeSymbol type)
  {
    Slot key = Build(type.TypeArguments[0]);
    Slot value = Build(type.TypeArguments[1]);
    slot.Dependencies.Add(key);
    slot.Dependencies.Add(value);
    if (key.Kind == SlotKind.Error || value.Kind == SlotKind.Error)
    {
      Fail(slot, "a key or value cannot be cloned", LocationOf(type), key.Kind == SlotKind.Error ? key : value);
      return;
    }

    if (key.Kind == SlotKind.Share && value.Kind == SlotKind.Share)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.Statements =
    [
      $"return new {type.ToDisplayString(Format)}({CopyExpression(key, "source.Key")}, {CopyExpression(value, "source.Value")});"
    ];
  }

  private bool IsValueTuple(INamedTypeSymbol type) =>
    type.IsTupleType
    || type.IsValueType && type.ContainingNamespace?.ToDisplayString() == "System" && type.Name == "ValueTuple" && type.Arity > 0;

  // ValueTuple fields are public and mutable, so they are assigned directly; no accessor is needed.
  private void ClassifyTuple(Slot slot, INamedTypeSymbol type)
  {
    List<string> lines = [$"{type.ToDisplayString(Format)} clone = source;"];
    for (int index = 0; index < type.TypeArguments.Length; index++)
    {
      string field = index == 7 ? "Rest" : $"Item{index + 1}";
      Slot item = Build(type.TypeArguments[index]);
      slot.Dependencies.Add(item);
      if (item.Kind == SlotKind.Error)
      {
        Fail(slot, $"tuple element {field} cannot be cloned", LocationOf(type), item);
        return;
      }

      if (item.Kind != SlotKind.Share)
      {
        lines.Add($"clone.{field} = {CopyExpression(item, $"source.{field}")};");
      }
    }

    if (lines.Count == 1)
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    lines.Add("return clone;");
    slot.Kind = SlotKind.Clone;
    slot.Statements = lines;
  }

  private static void ClassifyStringBuilder(Slot slot, INamedTypeSymbol type)
  {
    string name = type.ToDisplayString(Format);
    slot.Kind = SlotKind.Clone;
    slot.Statements =
    [
      $"if (map.TryGet(source, out {name}? existing))",
      "{",
      "  return existing;",
      "}",
      $"{name} clone = new(source.ToString(), source.Capacity);",
      "map.Add(source, clone);",
      "return clone;"
    ];
  }

  #endregion

  #region Collections

  private void ClassifyCollection(Slot slot, INamedTypeSymbol type, CollectionKind kind)
  {
    if (IsDictionaryKind(kind))
    {
      if (!TryDictionaryArguments(type, out ITypeSymbol? key, out ITypeSymbol? value))
      {
        Fail(slot, "the dictionary type arguments could not be resolved", LocationOf(type));
        return;
      }

      Slot keySlot = Build(key!);
      Slot valueSlot = Build(value!);
      slot.Dependencies.Add(keySlot);
      slot.Dependencies.Add(valueSlot);
      if (keySlot.Kind == SlotKind.Error || valueSlot.Kind == SlotKind.Error)
      {
        Fail(slot, "a dictionary key or value cannot be cloned", LocationOf(type), keySlot.Kind == SlotKind.Error ? keySlot : valueSlot);
        return;
      }

      bool sharedElements = keySlot.Kind == SlotKind.Share && valueSlot.Kind == SlotKind.Share;
      if (sharedElements && kind is CollectionKind.ImmutableDictionary or CollectionKind.ImmutableSortedDictionary or CollectionKind.FrozenDictionary)
      {
        slot.Kind = SlotKind.Share;
        return;
      }

      if (kind == CollectionKind.DictionaryInterface)
      {
        ClassifyDictionaryInterface(slot, type, keySlot, valueSlot);
        return;
      }

      ClassifyDictionary(slot, type, kind, keySlot, valueSlot);
      return;
    }

    if (!TryElementType(type, out ITypeSymbol? elementType))
    {
      Fail(slot, "the collection element type could not be resolved", LocationOf(type));
      return;
    }

    Slot element = Build(elementType!);
    slot.Dependencies.Add(element);
    if (element.Kind == SlotKind.Error)
    {
      Fail(slot, $"element '{elementType!.ToDisplayString()}' cannot be cloned", LocationOf(type), element);
      return;
    }

    if (element.Kind == SlotKind.Share && IsImmutableKind(kind))
    {
      slot.Kind = SlotKind.Share;
      return;
    }

    if (kind == CollectionKind.EnumerableInterface)
    {
      ClassifyEnumerableInterface(slot, type, element);
      return;
    }

    ClassifyEnumerableCollection(slot, type, kind, element);
  }

  private static bool IsDictionaryKind(CollectionKind kind) =>
    kind is CollectionKind.Dictionary
      or CollectionKind.SortedDictionary
      or CollectionKind.SortedList
      or CollectionKind.ConcurrentDictionary
      or CollectionKind.ReadOnlyDictionary
      or CollectionKind.ImmutableDictionary
      or CollectionKind.ImmutableSortedDictionary
      or CollectionKind.FrozenDictionary
      or CollectionKind.DictionaryInterface;

  private static bool IsImmutableKind(CollectionKind kind) =>
    kind is CollectionKind.ImmutableArray
      or CollectionKind.ImmutableList
      or CollectionKind.ImmutableHashSet
      or CollectionKind.ImmutableSortedSet
      or CollectionKind.ImmutableQueue
      or CollectionKind.ImmutableStack
      or CollectionKind.FrozenSet;

  private void ClassifyDictionary(Slot slot, INamedTypeSymbol type, CollectionKind kind, Slot keySlot, Slot valueSlot)
  {
    string name = type.ToDisplayString(Format);
    string keyName = keySlot.FullyQualified;
    string valueName = valueSlot.FullyQualified;
    string keyCopy = CopyExpression(keySlot, "pair.Key");
    string valueCopy = CopyExpression(valueSlot, "pair.Value");
    bool exact = SymbolEquals(type.OriginalDefinition, DefinitionName(kind));
    List<string> lines =
    [
      $"if (map.TryGet(source, out {name}? existing))",
      "{",
      "  return existing;",
      "}"
    ];

    switch (kind)
    {
      case CollectionKind.ImmutableDictionary:
      case CollectionKind.ImmutableSortedDictionary:
        string factory = kind == CollectionKind.ImmutableDictionary ? "ImmutableDictionary" : "ImmutableSortedDictionary";
        lines.Add($"var builder = global::System.Collections.Immutable.{factory}.CreateBuilder<{keyName}, {valueName}>(source.KeyComparer, source.ValueComparer);");
        AppendForEachPair(lines, $"builder.Add({keyCopy}, {valueCopy});");
        lines.Add($"{name} clone = builder.ToImmutable();");
        lines.Add("map.Add(source, clone);");
        lines.Add("return clone;");
        break;
      case CollectionKind.FrozenDictionary:
        lines.Add($"{GenericDictionary}<{keyName}, {valueName}> items = new(source.Count, source.Comparer);");
        AppendForEachPair(lines, $"items.Add({keyCopy}, {valueCopy});");
        lines.Add($"{name} clone = global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(items, source.Comparer);");
        lines.Add("map.Add(source, clone);");
        lines.Add("return clone;");
        break;
      case CollectionKind.ReadOnlyDictionary:
        if (!exact)
        {
          Fail(slot, "a subclass of ReadOnlyDictionary cannot be rebuilt", LocationOf(type));
          return;
        }

        lines.Add($"{GenericDictionary}<{keyName}, {valueName}> items = new(source.Count);");
        lines.Add($"{name} clone = new(items);");
        lines.Add("map.Add(source, clone);");
        AppendForEachPair(lines, $"items.Add({keyCopy}, {valueCopy});");
        lines.Add("return clone;");
        break;
      default:
        if (exact)
        {
          lines.Add(kind switch
          {
            CollectionKind.SortedDictionary => $"{name} clone = new(source.Comparer);",
            CollectionKind.ConcurrentDictionary => $"{name} clone = new(source.Comparer);",
            _ => $"{name} clone = new(source.Count, source.Comparer);"
          });
        }
        else if (!TryEmitConstruction(type, lines, out string? problem))
        {
          Fail(slot, problem!, LocationOf(type));
          return;
        }

        lines.Add("map.Add(source, clone);");
        if (!exact)
        {
          AppendExtraFields(slot, type, lines, CollectionDefinition(type));
          if (slot.Kind == SlotKind.Error)
          {
            return;
          }

          lines.Add("clone.Clear();");
        }

        string add = kind == CollectionKind.ConcurrentDictionary ? "TryAdd" : "Add";
        AppendForEachPair(lines, $"clone.{add}({keyCopy}, {valueCopy});");
        lines.Add("return clone;");
        break;
    }

    slot.Kind = SlotKind.Clone;
    slot.Statements = lines;
  }

  private static void AppendForEachPair(List<string> lines, string statement)
  {
    lines.Add("foreach (var pair in source)");
    lines.Add("{");
    lines.Add($"  {statement}");
    lines.Add("}");
  }

  private static void AppendForEachItem(List<string> lines, string statement)
  {
    lines.Add("foreach (var item in source)");
    lines.Add("{");
    lines.Add($"  {statement}");
    lines.Add("}");
  }

  private void ClassifyEnumerableCollection(Slot slot, INamedTypeSymbol type, CollectionKind kind, Slot element)
  {
    string name = type.ToDisplayString(Format);
    string elementName = element.FullyQualified;
    string copy = CopyExpression(element, "item");
    bool exact = SymbolEquals(type.OriginalDefinition, DefinitionName(kind));
    List<string> lines = [];
    slot.Kind = SlotKind.Clone;
    slot.Statements = lines;
    if (kind == CollectionKind.ImmutableArray)
    {
      lines.Add("if (source.IsDefault)");
      lines.Add("{");
      lines.Add("  return default;");
      lines.Add("}");
      lines.Add($"var builder = global::System.Collections.Immutable.ImmutableArray.CreateBuilder<{elementName}>(source.Length);");
      AppendForEachItem(lines, $"builder.Add({copy});");
      lines.Add("return builder.MoveToImmutable();");
      return;
    }

    lines.Add($"if (map.TryGet(source, out {name}? existing))");
    lines.Add("{");
    lines.Add("  return existing;");
    lines.Add("}");
    switch (kind)
    {
      case CollectionKind.ImmutableList:
      case CollectionKind.ImmutableHashSet:
      case CollectionKind.ImmutableSortedSet:
        string factory = kind switch
        {
          CollectionKind.ImmutableHashSet => $"ImmutableHashSet.CreateBuilder<{elementName}>(source.KeyComparer)",
          CollectionKind.ImmutableSortedSet => $"ImmutableSortedSet.CreateBuilder<{elementName}>(source.KeyComparer)",
          _ => $"ImmutableList.CreateBuilder<{elementName}>()"
        };
        lines.Add($"var builder = global::System.Collections.Immutable.{factory};");
        AppendForEachItem(lines, $"builder.Add({copy});");
        lines.Add($"{name} clone = builder.ToImmutable();");
        lines.Add("map.Add(source, clone);");
        lines.Add("return clone;");
        return;
      case CollectionKind.ImmutableQueue:
        lines.Add($"{name} clone = {name}.Empty;");
        AppendForEachItem(lines, $"clone = clone.Enqueue({copy});");
        lines.Add("map.Add(source, clone);");
        lines.Add("return clone;");
        return;
      case CollectionKind.ImmutableStack:
        // Enumeration runs top to bottom, so push the copies back in reverse to keep the top on top.
        lines.Add($"{GenericList}<{elementName}> buffer = new();");
        AppendForEachItem(lines, $"buffer.Add({copy});");
        lines.Add($"{name} clone = {name}.Empty;");
        lines.Add("for (int index = buffer.Count - 1; index >= 0; index--)");
        lines.Add("{");
        lines.Add("  clone = clone.Push(buffer[index]);");
        lines.Add("}");
        lines.Add("map.Add(source, clone);");
        lines.Add("return clone;");
        return;
      case CollectionKind.FrozenSet:
        lines.Add($"{GenericList}<{elementName}> items = new(source.Count);");
        AppendForEachItem(lines, $"items.Add({copy});");
        lines.Add($"{name} clone = global::System.Collections.Frozen.FrozenSet.ToFrozenSet(items, source.Comparer);");
        lines.Add("map.Add(source, clone);");
        lines.Add("return clone;");
        return;
      case CollectionKind.ReadOnlyCollection:
        if (!exact)
        {
          Fail(slot, "a subclass of ReadOnlyCollection cannot be rebuilt", LocationOf(type));
          return;
        }

        lines.Add($"{GenericList}<{elementName}> items = new(source.Count);");
        lines.Add($"{name} clone = new(items);");
        lines.Add("map.Add(source, clone);");
        AppendForEachItem(lines, $"items.Add({copy});");
        lines.Add("return clone;");
        return;
    }

    if (exact)
    {
      lines.Add(kind switch
      {
        CollectionKind.List or CollectionKind.Stack or CollectionKind.Queue => $"{name} clone = new(source.Count);",
        CollectionKind.HashSet or CollectionKind.SortedSet => $"{name} clone = new(source.Comparer);",
        _ => $"{name} clone = new();"
      });
    }
    else if (!TryEmitConstruction(type, lines, out string? problem))
    {
      Fail(slot, problem!, LocationOf(type));
      return;
    }

    lines.Add("map.Add(source, clone);");
    if (!exact)
    {
      AppendExtraFields(slot, type, lines, CollectionDefinition(type));
      if (slot.Kind == SlotKind.Error)
      {
        return;
      }

      lines.Add("clone.Clear();");
    }

    switch (kind)
    {
      case CollectionKind.Stack:
        lines.Add($"{elementName}[] buffer = source.ToArray();");
        lines.Add("for (int index = buffer.Length - 1; index >= 0; index--)");
        lines.Add("{");
        lines.Add($"  clone.Push({CopyExpression(element, "buffer[index]")});");
        lines.Add("}");
        break;
      case CollectionKind.Queue:
        AppendForEachItem(lines, $"clone.Enqueue({copy});");
        break;
      case CollectionKind.LinkedList:
        AppendForEachItem(lines, $"clone.AddLast({copy});");
        break;
      case CollectionKind.List when exact && element.Kind == SlotKind.Share:
        lines.Add("clone.AddRange(source);");
        break;
      default:
        AppendForEachItem(lines, $"clone.Add({copy});");
        break;
    }

    lines.Add("return clone;");
  }

  private void ClassifyEnumerableInterface(Slot slot, INamedTypeSymbol type, Slot element)
  {
    slot.Kind = SlotKind.Clone;
    slot.IsDispatch = true;
    ITypeSymbol elementType = element.Type;
    AddCase(slot, Compilation.CreateArrayTypeSymbol(elementType), "array", required: false, guard: false);
    foreach (string metadataName in EnumerableCaseTypes)
    {
      AddConstructedCase(slot, metadataName, [elementType]);
    }

    foreach (INamedTypeSymbol implementation in FindImplementations(type))
    {
      AddCase(slot, implementation, "typed", required: true, guard: false);
    }

    bool isSet = SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.ISet`1")
      || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IReadOnlySet`1");
    string definition = isSet ? "System.Collections.Generic.HashSet`1" : "System.Collections.Generic.List`1";
    INamedTypeSymbol? fallback = Compilation.GetTypeByMetadataName(definition)?.Construct(elementType);
    if (fallback is null || !Compilation.HasImplicitConversion(fallback, type))
    {
      return;
    }

    slot.Fallback =
    [
      "{",
      $"  {fallback.ToDisplayString(Format)} materialized = new();",
      "  map.Add(source, materialized);",
      "  foreach (var item in source)",
      "  {",
      $"    materialized.Add({CopyExpression(element, "item")});",
      "  }",
      "  return materialized;",
      "}"
    ];
  }

  private void ClassifyDictionaryInterface(Slot slot, INamedTypeSymbol type, Slot keySlot, Slot valueSlot)
  {
    slot.Kind = SlotKind.Clone;
    slot.IsDispatch = true;
    ITypeSymbol[] arguments = [keySlot.Type, valueSlot.Type];
    foreach (string metadataName in DictionaryCaseTypes)
    {
      AddConstructedCase(slot, metadataName, arguments);
    }

    foreach (INamedTypeSymbol implementation in FindImplementations(type))
    {
      AddCase(slot, implementation, "typed", required: true, guard: false);
    }

    INamedTypeSymbol? fallback = Compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")?.Construct(arguments);
    if (fallback is null || !Compilation.HasImplicitConversion(fallback, type))
    {
      return;
    }

    slot.Fallback =
    [
      "{",
      $"  {fallback.ToDisplayString(Format)} materialized = new();",
      "  map.Add(source, materialized);",
      "  foreach (var pair in source)",
      "  {",
      $"    materialized.Add({CopyExpression(keySlot, "pair.Key")}, {CopyExpression(valueSlot, "pair.Value")});",
      "  }",
      "  return materialized;",
      "}"
    ];
  }

  private static readonly string[] EnumerableCaseTypes =
  [
    "System.Collections.Generic.List`1",
    "System.Collections.Generic.HashSet`1",
    "System.Collections.Generic.SortedSet`1",
    "System.Collections.Generic.LinkedList`1",
    "System.Collections.Generic.Queue`1",
    "System.Collections.Generic.Stack`1",
    "System.Collections.ObjectModel.ObservableCollection`1",
    "System.Collections.ObjectModel.Collection`1",
    "System.Collections.ObjectModel.ReadOnlyCollection`1",
    "System.Collections.Immutable.ImmutableArray`1",
    "System.Collections.Immutable.ImmutableList`1",
    "System.Collections.Immutable.ImmutableHashSet`1",
    "System.Collections.Immutable.ImmutableSortedSet`1",
    "System.Collections.Immutable.ImmutableQueue`1",
    "System.Collections.Immutable.ImmutableStack`1",
    "System.Collections.Frozen.FrozenSet`1"
  ];

  private static readonly string[] DictionaryCaseTypes =
  [
    "System.Collections.Generic.Dictionary`2",
    "System.Collections.Generic.SortedDictionary`2",
    "System.Collections.Generic.SortedList`2",
    "System.Collections.Concurrent.ConcurrentDictionary`2",
    "System.Collections.ObjectModel.ReadOnlyDictionary`2",
    "System.Collections.Immutable.ImmutableDictionary`2",
    "System.Collections.Immutable.ImmutableSortedDictionary`2",
    "System.Collections.Frozen.FrozenDictionary`2"
  ];

  // BCL cases are optional: a type that cannot be cloned here falls through to the materializing fallback. A
  // non-sealed BCL type is matched only by exact runtime type, so an unknown subclass also falls through.
  private void AddConstructedCase(Slot parent, string metadataName, ITypeSymbol[] arguments)
  {
    INamedTypeSymbol? definition = Compilation.GetTypeByMetadataName(metadataName);
    if (definition is null || definition.Arity != arguments.Length)
    {
      return;
    }

    INamedTypeSymbol constructed = definition.Construct(arguments);
    bool guard = constructed.TypeKind == TypeKind.Class && !constructed.IsSealed && !constructed.IsAbstract;
    AddCase(parent, constructed, "typed", required: false, guard);
  }

  private void AddCase(Slot parent, ITypeSymbol type, string variable, bool required, bool guard)
  {
    if (!CanName(type) || !Compilation.HasImplicitConversion(type, parent.Type))
    {
      return;
    }

    Slot slot = Build(type);
    if (slot.Kind == SlotKind.Error && !required)
    {
      return;
    }

    parent.Dependencies.Add(slot);
    parent.Cases.Add(new CaseRequest(type, variable, 0, guard));
  }

  #endregion

  #region Interfaces and objects

  private void ClassifyInterface(Slot slot, INamedTypeSymbol type)
  {
    if (FindHiddenSubtype(type, implementations: true) is { } hidden)
    {
      Fail(slot, HiddenSubtypeDetail(hidden), LocationOf(hidden));
      return;
    }

    List<INamedTypeSymbol> implementations = FindImplementations(type);
    if (implementations.Count == 0 && !AssignableTo(type, "System.ICloneable"))
    {
      Fail(slot, $"interface or abstract type '{type.ToDisplayString()}' has no implementation the generator can see", LocationOf(type));
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.IsDispatch = true;
    foreach (INamedTypeSymbol implementation in implementations)
    {
      AddCase(slot, implementation, "typed", required: true, guard: false);
    }
  }

  private void ClassifyObject(Slot slot, INamedTypeSymbol type)
  {
    List<string> lines = [];
    string name = type.ToDisplayString(Format);
    if (!type.IsValueType)
    {
      lines.Add($"if (map.TryGet(source, out {name}? existing))");
      lines.Add("{");
      lines.Add("  return existing;");
      lines.Add("}");
      if (!TryEmitConstruction(type, lines, out string? problem))
      {
        Fail(slot, problem!, LocationOf(type));
        return;
      }

      lines.Add("map.Add(source, clone);");
    }
    else
    {
      lines.Add($"{name} clone = source;");
    }

    bool copied = AppendExtraFields(slot, type, lines, stopBefore: null);
    if (slot.Kind == SlotKind.Error)
    {
      return;
    }

    if (type.IsValueType && !copied)
    {
      slot.Kind = SlotKind.Share;
      slot.Statements = null;
      return;
    }

    slot.Kind = SlotKind.Clone;
    lines.Add("return clone;");
    slot.Statements = lines;
  }

  private bool AppendExtraFields(Slot slot, INamedTypeSymbol type, List<string> lines, INamedTypeSymbol? stopBefore)
  {
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, stopBefore))
    {
      if (IsFromSource(current))
      {
        continue;
      }

      string? problem = MetadataProblem(current);
      if (problem is not null)
      {
        Fail(slot, problem, SourceLocation(type));
        return false;
      }
    }

    bool copied = false;
    HashSet<ISymbol> copiedMembers = new(SymbolEqualityComparer.Default);
    foreach (IFieldSymbol field in EnumerateFields(type, stopBefore))
    {
      if (field.AssociatedSymbol is not null)
      {
        copiedMembers.Add(field.AssociatedSymbol);
      }

      bool shared = IsCloneShared(field);
      if (!shared && IsIgnored(field))
      {
        continue;
      }

      Member member = new(field.Type, field.OriginalDefinition.Type, field.Name, field.ContainingType, FieldLocation(field));
      if (shared)
      {
        if (!TryAppendSharedMember(slot, type, lines, member, ref copied))
        {
          return copied;
        }

        continue;
      }

      if (!TryAppendMember(slot, type, lines, member, ref copied))
      {
        return copied;
      }
    }

    // Referenced assemblies do not expose <Prop>k__BackingField through GetMembers. The getter is still
    // CompilerGenerated, and the field name is fixed, so copy those properties the same way.
    foreach ((IPropertySymbol property, INamedTypeSymbol declaringType) in EnumerateAutoProperties(type, stopBefore))
    {
      if (!copiedMembers.Add(property))
      {
        continue;
      }

      bool shared = HasCloneSharedAttribute(property);
      if (!shared && HasIgnoreAttribute(property))
      {
        continue;
      }

      Location? location = property.Locations.FirstOrDefault(candidate => candidate.IsInSource);
      Member member = new(property.Type, property.OriginalDefinition.Type, $"<{property.Name}>k__BackingField", declaringType, location);
      if (shared)
      {
        if (!TryAppendSharedMember(slot, type, lines, member, ref copied))
        {
          return copied;
        }

        continue;
      }

      if (!TryAppendMember(slot, type, lines, member, ref copied))
      {
        return copied;
      }
    }

    return copied;
  }

  private bool TryAppendMember(Slot slot, INamedTypeSymbol type, List<string> lines, Member member, ref bool copied)
  {
    Location? location = member.Location is { IsInSource: true } ? member.Location : SourceLocation(type);
    Slot fieldSlot = Build(member.Type);
    slot.Dependencies.Add(fieldSlot);
    if (fieldSlot.Kind == SlotKind.Error)
    {
      Fail(slot, $"member '{DisplayMemberName(member.FieldName)}' of type '{member.Type.ToDisplayString()}' cannot be cloned ({fieldSlot.Detail})", location, fieldSlot, member: true);
      return false;
    }

    if (type.IsValueType && fieldSlot.Kind == SlotKind.Share)
    {
      return true;
    }

    string? accessor = FieldAccessor(member.DeclaringType, member.FieldName, member.DefinitionType, out string? problem);
    if (accessor is null)
    {
      Fail(slot, $"member '{DisplayMemberName(member.FieldName)}' cannot be written ({problem})", location, member: true);
      return false;
    }

    string read = type.IsValueType ? $"{accessor}(ref source)" : $"{accessor}(source)";
    string write = type.IsValueType ? $"{accessor}(ref clone)" : $"{accessor}(clone)";
    lines.Add($"{write} = {CopyExpression(fieldSlot, read)};");
    copied = true;
    return true;
  }

  // Copy the reference (or the whole value) without classifying the member type. Classification is what rejects
  // an interface with no visible implementation and a framework type whose private fields cannot be seen.
  private bool TryAppendSharedMember(Slot slot, INamedTypeSymbol type, List<string> lines, Member member, ref bool copied)
  {
    Location? location = member.Location is { IsInSource: true } ? member.Location : SourceLocation(type);
    string? accessor = FieldAccessor(member.DeclaringType, member.FieldName, member.DefinitionType, out string? problem);
    if (accessor is null)
    {
      Fail(slot, $"member '{DisplayMemberName(member.FieldName)}' cannot be written ({problem})", location, member: true);
      return false;
    }

    string read = type.IsValueType ? $"{accessor}(ref source)" : $"{accessor}(source)";
    string write = type.IsValueType ? $"{accessor}(ref clone)" : $"{accessor}(clone)";
    lines.Add($"{write} = {read};");
    copied = true;
    return true;
  }

  private static string DisplayMemberName(string fieldName)
  {
    int end = fieldName.IndexOf('>');
    return fieldName.Length > 0 && fieldName[0] == '<' && end > 1 ? fieldName.Substring(1, end - 1) : fieldName;
  }

  #endregion

  #region Construction

  // Parameterless constructor first (any accessibility, as the reflection cloner did), else the accessible
  // constructor with the fewest parameters, else any constructor through UnsafeAccessor. Arguments are typed
  // defaults (or the declared default value) so overloads of the same arity stay unambiguous. A constructor that
  // rejects default arguments throws at run time; implement ICloneable on that type.
  private bool TryEmitConstruction(INamedTypeSymbol type, List<string> lines, out string? problem)
  {
    string name = type.ToDisplayString(Format);
    List<IMethodSymbol> candidates = type.InstanceConstructors
      .Where(constructor => constructor.Parameters.All(parameter => parameter.RefKind == RefKind.None && CanNameDeep(parameter.Type)))
      .ToList();
    IMethodSymbol? parameterless = candidates.FirstOrDefault(constructor => constructor.Parameters.Length == 0);
    IMethodSymbol? accessible = parameterless is not null && IsAccessible(parameterless)
      ? parameterless
      : parameterless is null
        ? candidates.Where(IsAccessible).OrderBy(constructor => constructor.Parameters.Length).FirstOrDefault()
        : null;

    if (accessible is not null)
    {
      List<string>? required = RequiredInitializer(type, accessible);
      if (required is not null)
      {
        string arguments = string.Join(", ", accessible.Parameters.Select(ArgumentFor));
        if (required.Count == 0)
        {
          lines.Add($"{name} clone = new({arguments});");
        }
        else
        {
          lines.Add($"{name} clone = new({arguments})");
          lines.Add("{");
          foreach (string member in required)
          {
            lines.Add($"  {member} = default!,");
          }

          lines.Add("};");
        }

        problem = null;
        return true;
      }
    }

    IMethodSymbol? any = parameterless ?? accessible ?? candidates.OrderBy(constructor => constructor.Parameters.Length).FirstOrDefault();
    if (any is null)
    {
      problem = "it has no constructor the generated clone can call";
      return false;
    }

    string? call = ConstructorAccessor(type, any, out problem);
    if (call is null)
    {
      return false;
    }

    lines.Add($"{name} clone = {call}({string.Join(", ", any.Parameters.Select(ArgumentFor))});");
    return true;
  }

  private static string ArgumentFor(IParameterSymbol parameter)
  {
    string typeName = parameter.Type.ToDisplayString(Format);
    if (parameter.HasExplicitDefaultValue
        && parameter.ExplicitDefaultValue is { } value
        && IsFinite(value)
        && SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false) is { Length: > 0 } literal)
    {
      return $"({typeName})({literal})";
    }

    return parameter.Type.IsValueType ? $"default({typeName})" : $"default({typeName})!";
  }

  private static bool IsFinite(object value) =>
    value switch
    {
      double number => !double.IsNaN(number) && !double.IsInfinity(number),
      float number => !float.IsNaN(number) && !float.IsInfinity(number),
      _ => true
    };

  private List<string>? RequiredInitializer(INamedTypeSymbol type, IMethodSymbol constructor)
  {
    if (constructor.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "SetsRequiredMembersAttribute"))
    {
      return [];
    }

    List<string> names = [];
    HashSet<string> seen = new(StringComparer.Ordinal);
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, null))
    {
      foreach (ISymbol member in current.GetMembers())
      {
        switch (member)
        {
          case IPropertySymbol { IsRequired: true } property when seen.Add(property.Name):
            if (property.SetMethod is null)
            {
              return null;
            }

            names.Add(property.Name);
            break;
          case IFieldSymbol { IsRequired: true } field when seen.Add(field.Name):
            if (field.IsReadOnly)
            {
              return null;
            }

            names.Add(field.Name);
            break;
        }
      }
    }

    return names;
  }

  private bool IsAccessible(ISymbol symbol)
  {
    return symbol.DeclaredAccessibility switch
    {
      Accessibility.Public => true,
      Accessibility.Internal or Accessibility.ProtectedOrInternal =>
        SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, Compilation.Assembly),
      _ => false
    };
  }

  #endregion

  #region Accessors

  private string? FieldAccessor(INamedTypeSymbol declaringType, string fieldName, ITypeSymbol definitionType, out string? problem)
  {
    Holder? holder = GetHolder(declaringType.OriginalDefinition, out problem);
    if (holder is null)
    {
      return null;
    }

    if (!CanNameDeep(definitionType))
    {
      problem = $"its type '{definitionType.ToDisplayString()}' is not accessible to generated code";
      return null;
    }

    if (!holder.Methods.TryGetValue("field:" + fieldName, out string? method))
    {
      method = $"F_{Sanitize(DisplayMemberName(fieldName))}_{StableHash(fieldName)}";
      holder.Methods.Add("field:" + fieldName, method);
      string literal = fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"");
      string reference = holder.Definition.IsReferenceType ? string.Empty : "ref ";
      holder.Members.Add(
        $"[{UnsafeAccessor}({UnsafeAccessorKind}.Field, Name = \"{literal}\")] " +
        $"internal static extern ref {TypeText(definitionType, holder.Map)} {method}({reference}{holder.Self} target);");
    }

    return holder.Name + HolderArguments(declaringType) + "." + method;
  }

  private string? ConstructorAccessor(INamedTypeSymbol type, IMethodSymbol constructor, out string? problem)
  {
    Holder? holder = GetHolder(type.OriginalDefinition, out problem);
    if (holder is null)
    {
      return null;
    }

    IMethodSymbol definition = constructor.OriginalDefinition;
    string parameters = string.Join(", ", definition.Parameters.Select((parameter, index) => $"{TypeText(parameter.Type, holder.Map)} p{index}"));
    string key = "ctor:" + parameters;
    if (!holder.Methods.TryGetValue(key, out string? method))
    {
      method = $"New_{StableHash(key)}";
      holder.Methods.Add(key, method);
      holder.Members.Add(
        $"[{UnsafeAccessor}({UnsafeAccessorKind}.Constructor)] internal static extern {holder.Self} {method}({parameters});");
    }

    return holder.Name + HolderArguments(type) + "." + method;
  }

  private Holder? GetHolder(INamedTypeSymbol definition, out string? problem)
  {
    if (Holders.TryGetValue(definition, out Holder? existing))
    {
      problem = existing.Problem;
      return existing.Problem is null ? existing : null;
    }

    List<ITypeParameterSymbol> parameters = AllTypeParameters(definition);
    Dictionary<ITypeParameterSymbol, string> map = new(SymbolEqualityComparer.Default);
    for (int index = 0; index < parameters.Count; index++)
    {
      map[parameters[index]] = $"T{index}";
    }

    Holder holder = new()
    {
      Definition = definition,
      Map = map,
      Name = UniqueName("Access_" + Sanitize(definition.ToDisplayString()), "holder:" + definition.ToDisplayString(Format)),
      TypeParameters = parameters.Count == 0 ? string.Empty : "<" + string.Join(", ", parameters.Select(parameter => map[parameter])) + ">",
      Self = TypeText(definition, map)
    };

    if (!CanName(definition))
    {
      holder.Problem = $"'{definition.ToDisplayString()}' is not accessible to generated code";
    }

    foreach (ITypeParameterSymbol parameter in parameters)
    {
      if (parameter.ConstraintTypes.Any(constraint => !CanNameDeep(constraint)))
      {
        holder.Problem = $"a constraint on '{definition.ToDisplayString()}' is not accessible to generated code";
      }

      string? clause = ConstraintClause(parameter, map);
      if (clause is not null)
      {
        holder.Constraints.Add(clause);
      }
    }

    Holders.Add(definition, holder);
    problem = holder.Problem;
    return holder.Problem is null ? holder : null;
  }

  private static string? ConstraintClause(ITypeParameterSymbol parameter, Dictionary<ITypeParameterSymbol, string> map)
  {
    List<string> parts = [];
    if (parameter.HasReferenceTypeConstraint)
    {
      parts.Add("class");
    }
    else if (parameter.HasUnmanagedTypeConstraint)
    {
      parts.Add("unmanaged");
    }
    else if (parameter.HasValueTypeConstraint)
    {
      parts.Add("struct");
    }
    else if (parameter.HasNotNullConstraint)
    {
      parts.Add("notnull");
    }

    parts.AddRange(parameter.ConstraintTypes.Select(constraint => TypeText(constraint, map)));
    if (parameter.HasConstructorConstraint && !parameter.HasValueTypeConstraint)
    {
      parts.Add("new()");
    }

    if (parameter.AllowsRefLikeType)
    {
      parts.Add("allows ref struct");
    }

    return parts.Count == 0 ? null : $"where {map[parameter]} : {string.Join(", ", parts)}";
  }

  private static List<ITypeParameterSymbol> AllTypeParameters(INamedTypeSymbol definition)
  {
    List<ITypeParameterSymbol> parameters = definition.ContainingType is { } containing ? AllTypeParameters(containing) : [];
    parameters.AddRange(definition.TypeParameters);
    return parameters;
  }

  private static List<ITypeSymbol> AllTypeArguments(INamedTypeSymbol type)
  {
    List<ITypeSymbol> arguments = type.ContainingType is { } containing ? AllTypeArguments(containing) : [];
    arguments.AddRange(type.TypeArguments);
    return arguments;
  }

  private static string HolderArguments(INamedTypeSymbol type)
  {
    List<ITypeSymbol> arguments = AllTypeArguments(type);
    return arguments.Count == 0 ? string.Empty : "<" + string.Join(", ", arguments.Select(argument => argument.ToDisplayString(Format))) + ">";
  }

  // Writes a type with the holder's type parameter names. Nullable reference annotations are dropped: the runtime
  // matches the field signature, which has none.
  private static string TypeText(ITypeSymbol type, Dictionary<ITypeParameterSymbol, string> map)
  {
    switch (type)
    {
      case ITypeParameterSymbol parameter:
        return map.TryGetValue(parameter, out string? name) ? name : parameter.Name;
      case IArrayTypeSymbol array:
        StringBuilder suffix = new();
        ITypeSymbol element = array;
        while (element is IArrayTypeSymbol current)
        {
          suffix.Append('[').Append(new string(',', current.Rank - 1)).Append(']');
          element = current.ElementType;
        }

        return TypeText(element, map) + suffix;
      case IPointerTypeSymbol pointer:
        return TypeText(pointer.PointedAtType, map) + "*";
      case INamedTypeSymbol named:
        if (named.IsTupleType && named.TupleUnderlyingType is { } underlying)
        {
          named = underlying;
        }

        string prefix = named.ContainingType is { } containingType
          ? TypeText(containingType, map) + "."
          : named.ContainingNamespace is null || named.ContainingNamespace.IsGlobalNamespace
            ? "global::"
            : "global::" + named.ContainingNamespace.ToDisplayString() + ".";
        string identifier = SyntaxFacts.GetKeywordKind(named.Name) == SyntaxKind.None ? named.Name : "@" + named.Name;
        string arguments = named.TypeArguments.Length == 0
          ? string.Empty
          : "<" + string.Join(", ", named.TypeArguments.Select(argument => TypeText(argument, map))) + ">";
        return prefix + identifier + arguments;
      default:
        return type.ToDisplayString(Format);
    }
  }

  #endregion

  #region Metadata types

  private bool IsFromSource(INamedTypeSymbol type) =>
    SymbolEqualityComparer.Default.Equals(type.OriginalDefinition.ContainingAssembly, Compilation.Assembly);

  // A type from another assembly is cloned field by field only when its full field list is known. TimeWarp.State's
  // own bases are trusted. An implementation assembly is re-imported with private members to check for hidden state.
  // A reference assembly strips private class fields, and only keeps placeholders for struct fields, so its classes
  // and managed structs are accepted only when nothing visible could hold hidden state (ReferenceAssemblyProblem:
  // auto-properties, constructors, accessors and compiler-generated record members); the rest are TWSG002.
  private string? MetadataProblem(INamedTypeSymbol type)
  {
    INamedTypeSymbol definition = type.OriginalDefinition;
    if (MetadataProblems.TryGetValue(definition, out string? cached))
    {
      return cached;
    }

    string? problem = FindMetadataProblem(definition);
    MetadataProblems.Add(definition, problem);
    return problem;
  }

  private string? FindMetadataProblem(INamedTypeSymbol definition)
  {
    IAssemblySymbol assembly = definition.ContainingAssembly;
    if (IsTimeWarpStateAssembly(assembly))
    {
      return null;
    }

    if (assembly.GetAttributes().Any(attribute =>
          attribute.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.ReferenceAssemblyAttribute"))
    {
      return ReferenceAssemblyProblem(definition, assembly);
    }

    INamedTypeSymbol? full = FullView(definition);
    if (full is null)
    {
      return $"the fields of '{definition.ToDisplayString()}' in '{assembly.Name}' could not be inspected";
    }

    HashSet<string> visibleFields = new(
      definition.GetMembers().OfType<IFieldSymbol>().Select(field => field.Name),
      StringComparer.Ordinal);
    HashSet<string> events = new(full.GetMembers().OfType<IEventSymbol>().Select(item => item.Name), StringComparer.Ordinal);
    foreach (IFieldSymbol field in full.GetMembers().OfType<IFieldSymbol>())
    {
      if (field.IsStatic || field.IsConst || visibleFields.Contains(field.Name) || HasIgnoreAttribute(field) || events.Contains(field.Name))
      {
        continue;
      }

      string propertyName = DisplayMemberName(field.Name);
      if (propertyName != field.Name)
      {
        IPropertySymbol? property = definition.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
        if (property is not null && IsAutoProperty(property))
        {
          continue;
        }

        IPropertySymbol? hidden = full.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
        if (hidden is not null && HasIgnoreAttribute(hidden))
        {
          continue;
        }
      }

      return $"'{definition.ToDisplayString()}' has private field '{field.Name}' that generated code cannot see";
    }

    return null;
  }

  // TimeWarp.State's own bases (State<T>, TimeWarpCacheableState<T>, ...) keep only ignored private state.
  private static readonly HashSet<string> TimeWarpStateAssemblies = new(StringComparer.Ordinal)
  {
    "TimeWarp.State",
    "TimeWarp.State.Blazor",
    "TimeWarp.State.Plus",
    "TimeWarp.State.Policies",
    "TimeWarp.State.Telemetry",
    "timewarp-state-plus"
  };

  private static bool IsTimeWarpStateAssembly(IAssemblySymbol assembly) => TimeWarpStateAssemblies.Contains(assembly.Name);

  // A reference assembly (a project reference, or a framework reference pack) hides private class fields. A type is
  // accepted only when nothing visible could hold or use hidden state: every instance property is an auto-property and
  // every instance method is a constructor, an accessor, or compiler-generated (records).
  private static string? ReferenceAssemblyProblem(INamedTypeSymbol definition, IAssemblySymbol assembly)
  {
    string? offending = null;
    foreach (ISymbol member in definition.GetMembers())
    {
      if (member.IsStatic || member.IsImplicitlyDeclared)
      {
        continue;
      }

      if (member is IPropertySymbol property && !IsAutoProperty(property))
      {
        offending = $"property '{property.Name}' is not an auto-property";
        break;
      }

      if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation or MethodKind.Destructor } method
          && !HasCompilerGenerated(method))
      {
        offending = $"method '{method.Name}' may use them";
        break;
      }
    }

    if (offending is null)
    {
      return null;
    }

    string problem = $"'{definition.ToDisplayString()}' comes from reference assembly '{assembly.Name}', which hides private fields, " +
      $"and {offending}";
    return IsFrameworkAssembly(assembly)
      ? problem
      : problem + $" (build '{assembly.Name}' with ProduceReferenceAssembly=false so the generator can inspect its fields)";
  }

  private INamedTypeSymbol? FullView(INamedTypeSymbol definition)
  {
    if (Compilation.GetMetadataReference(definition.ContainingAssembly) is not { } reference)
    {
      return null;
    }

    FullCompilation ??= Compilation.WithOptions(Compilation.Options.WithMetadataImportOptions(MetadataImportOptions.All));
    if (FullCompilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
    {
      return null;
    }

    return assembly.GetTypeByMetadataName(MetadataName(definition));
  }

  private static string MetadataName(INamedTypeSymbol type)
  {
    if (type.ContainingType is { } containing)
    {
      return MetadataName(containing) + "+" + type.MetadataName;
    }

    return type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace
      ? type.MetadataName
      : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;
  }

  #endregion

  #region Symbol helpers

  private bool CanName(ISymbol symbol)
  {
    for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
    {
      // A file-local type is visible only in its own file, never in the generated one.
      if (current is INamedTypeSymbol { IsFileLocal: true })
      {
        return false;
      }

      if (current.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected)
      {
        return false;
      }

      if (current.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedAndInternal or Accessibility.ProtectedOrInternal
          && !SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, Compilation.Assembly))
      {
        return false;
      }
    }

    return true;
  }

  private bool CanNameDeep(ITypeSymbol type) =>
    type switch
    {
      ITypeParameterSymbol => true,
      IArrayTypeSymbol array => CanNameDeep(array.ElementType),
      IPointerTypeSymbol pointer => CanNameDeep(pointer.PointedAtType),
      INamedTypeSymbol named => CanName(named) && AllTypeArguments(named).All(CanNameDeep),
      _ => CanName(type)
    };

  private IEnumerable<(IPropertySymbol Property, INamedTypeSymbol DeclaringType)> EnumerateAutoProperties(
    INamedTypeSymbol type,
    INamedTypeSymbol? stopBefore)
  {
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, stopBefore))
    {
      foreach (IPropertySymbol property in current.GetMembers().OfType<IPropertySymbol>())
      {
        if (IsAutoProperty(property))
        {
          yield return (property, current);
        }
      }
    }
  }

  private static bool IsAutoProperty(IPropertySymbol property)
  {
    if (property.IsStatic || property.Parameters.Length > 0)
    {
      return false;
    }

    return HasCompilerGenerated(property.GetMethod) || HasCompilerGenerated(property.SetMethod);
  }

  private static bool HasCompilerGenerated(IMethodSymbol? method) =>
    method?.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "CompilerGeneratedAttribute") == true;

  private static IEnumerable<INamedTypeSymbol> EnumerateHierarchy(INamedTypeSymbol type, INamedTypeSymbol? stopBefore)
  {
    for (INamedTypeSymbol? current = type; current is not null && current.SpecialType is not (SpecialType.System_Object or SpecialType.System_ValueType); current = current.BaseType)
    {
      if (stopBefore is not null && SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, stopBefore))
      {
        yield break;
      }

      yield return current;
    }
  }

  private IEnumerable<IFieldSymbol> EnumerateFields(INamedTypeSymbol type, INamedTypeSymbol? stopBefore)
  {
    foreach (INamedTypeSymbol current in EnumerateHierarchy(type, stopBefore))
    {
      foreach (ISymbol member in current.GetMembers())
      {
        // Implicit fields are auto-property and field-like event backing fields. DeepCloner copies those.
        // Ignore attributes on the associated property or event still drop the field in IsIgnored.
        // Metadata assemblies omit those fields from GetMembers; EnumerateAutoProperties covers that case.
        if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst)
        {
          yield return field;
        }
      }
    }
  }

  private static bool IsIgnored(IFieldSymbol field)
  {
    if (HasIgnoreAttribute(field))
    {
      return true;
    }

    return field.AssociatedSymbol is not null && HasIgnoreAttribute(field.AssociatedSymbol);
  }

  private static bool IsCloneShared(IFieldSymbol field)
  {
    if (HasCloneSharedAttribute(field))
    {
      return true;
    }

    return field.AssociatedSymbol is not null && HasCloneSharedAttribute(field.AssociatedSymbol);
  }

  private static bool HasIgnoreAttribute(ISymbol symbol)
  {
    foreach (AttributeData attribute in symbol.GetAttributes())
    {
      if (attribute.AttributeClass?.Name is "IgnoreDataMemberAttribute" or "NonSerializedAttribute" or "JsonIgnoreAttribute")
      {
        return true;
      }
    }

    return false;
  }

  private static bool HasCloneSharedAttribute(ISymbol symbol)
  {
    foreach (AttributeData attribute in symbol.GetAttributes())
    {
      if (attribute.AttributeClass?.Name is "CloneShared" or "CloneSharedAttribute")
      {
        return true;
      }
    }

    return false;
  }

  // Concrete known types (classes and structs) that implement an interface or derive from an abstract class.
  private List<INamedTypeSymbol> FindImplementations(INamedTypeSymbol target) => FindSubtypes(target, implementations: true);

  private List<INamedTypeSymbol> FindDerived(INamedTypeSymbol target) => FindSubtypes(target, implementations: false);

  private List<INamedTypeSymbol> FindSubtypes(INamedTypeSymbol target, bool implementations) =>
    Subtypes(target, implementations)
      .Where(CanCase)
      .OrderByDescending(Depth)
      .ToList();

  // A known subtype the generated switch cannot name or close would reach the runtime throw, so it fails the build
  // instead, unless it implements ICloneable (the dispatch's ICloneable case clones it).
  private INamedTypeSymbol? FindHiddenSubtype(INamedTypeSymbol target, bool implementations) =>
    Subtypes(target, implementations)
      .FirstOrDefault(type => !CanCase(type) && !ImplementsICloneable(type));

  // Concrete subtypes of target among the candidate types. A generic definition whose base chain or interfaces reach
  // target's definition (GD<T> : GB<T> for GB<int>) is closed over target's type arguments (GD<int>) when they
  // determine every type parameter and satisfy the constraints; otherwise the definition itself is returned, which
  // CanCase rejects, so it is TWSG002.
  private IEnumerable<INamedTypeSymbol> Subtypes(INamedTypeSymbol target, bool implementations)
  {
    foreach (INamedTypeSymbol type in CandidateTypes(target))
    {
      if (IsConcreteSubtype(type, target, implementations))
      {
        yield return type;
        continue;
      }

      if (CloseOver(type, target, implementations) is { } closed)
      {
        yield return closed;
      }
    }
  }

  // For a generic definition that can reach target, the construction that is a target subtype, or the definition
  // itself when its type parameters are not determined by target or its constraints do not hold. Null when no
  // construction of type can be a target.
  private INamedTypeSymbol? CloseOver(INamedTypeSymbol type, INamedTypeSymbol target, bool implementations)
  {
    if (!type.IsGenericType || !type.IsDefinition || !target.IsGenericType || type.IsAbstract
        || SymbolEqualityComparer.Default.Equals(type, target.OriginalDefinition)
        || (implementations ? type.TypeKind is not (TypeKind.Class or TypeKind.Struct) : type.TypeKind != TypeKind.Class))
    {
      return null;
    }

    INamedTypeSymbol? result = null;
    foreach (INamedTypeSymbol view in SupertypeViews(type, target.OriginalDefinition, implementations))
    {
      Dictionary<ITypeParameterSymbol, ITypeSymbol> map = new(SymbolEqualityComparer.Default);
      if (!Unify(view, target, type, map))
      {
        continue;
      }

      if (TryConstruct(type, map) is { } closed && IsConcreteSubtype(closed, target, implementations))
      {
        return closed;
      }

      result = type;
    }

    return result;
  }

  // The supertypes of a generic definition (base chain, plus interfaces for an interface target) that share target's
  // definition, still written in the definition's own type parameters (GB<T> for GD<T> : GB<T>).
  private static IEnumerable<INamedTypeSymbol> SupertypeViews(INamedTypeSymbol type, INamedTypeSymbol targetDefinition, bool implementations)
  {
    for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, targetDefinition))
      {
        yield return current;
      }
    }

    if (!implementations || targetDefinition.TypeKind != TypeKind.Interface)
    {
      yield break;
    }

    foreach (INamedTypeSymbol implemented in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, targetDefinition))
      {
        yield return implemented;
      }
    }
  }

  // Binds owner's type parameters so that pattern equals actual. False when the shapes cannot match, which means no
  // construction of owner is ever a target (X<T> : GB<List<T>> for GB<int>).
  private static bool Unify(ITypeSymbol pattern, ITypeSymbol actual, INamedTypeSymbol owner, Dictionary<ITypeParameterSymbol, ITypeSymbol> map)
  {
    if (pattern is ITypeParameterSymbol parameter && SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, owner))
    {
      if (map.TryGetValue(parameter, out ITypeSymbol? bound))
      {
        return SymbolEqualityComparer.Default.Equals(bound, actual);
      }

      map.Add(parameter, actual);
      return true;
    }

    switch (pattern)
    {
      case IArrayTypeSymbol patternArray when actual is IArrayTypeSymbol actualArray:
        return patternArray.Rank == actualArray.Rank && Unify(patternArray.ElementType, actualArray.ElementType, owner, map);
      case INamedTypeSymbol patternNamed when patternNamed.IsGenericType && actual is INamedTypeSymbol actualNamed:
        if (!SymbolEqualityComparer.Default.Equals(patternNamed.OriginalDefinition, actualNamed.OriginalDefinition))
        {
          return false;
        }

        List<ITypeSymbol> patternArguments = AllTypeArguments(patternNamed);
        List<ITypeSymbol> actualArguments = AllTypeArguments(actualNamed);
        if (patternArguments.Count != actualArguments.Count)
        {
          return false;
        }

        for (int index = 0; index < patternArguments.Count; index++)
        {
          if (!Unify(patternArguments[index], actualArguments[index], owner, map))
          {
            return false;
          }
        }

        return true;
      default:
        return SymbolEqualityComparer.Default.Equals(pattern, actual);
    }
  }

  // Constructs a top-level generic definition when every type parameter is bound to a closed type that meets its
  // constraints; null otherwise.
  private INamedTypeSymbol? TryConstruct(INamedTypeSymbol definition, Dictionary<ITypeParameterSymbol, ITypeSymbol> map)
  {
    if (definition.ContainingType is { IsGenericType: true })
    {
      return null;
    }

    ITypeSymbol[] arguments = new ITypeSymbol[definition.TypeParameters.Length];
    for (int index = 0; index < arguments.Length; index++)
    {
      if (!map.TryGetValue(definition.TypeParameters[index], out ITypeSymbol? argument) || ContainsTypeParameter(argument))
      {
        return null;
      }

      arguments[index] = argument;
    }

    INamedTypeSymbol constructed = definition.Construct(arguments);
    for (int index = 0; index < arguments.Length; index++)
    {
      if (!SatisfiesConstraints(definition.TypeParameters[index], arguments[index], constructed))
      {
        return null;
      }
    }

    return constructed;
  }

  private bool SatisfiesConstraints(ITypeParameterSymbol parameter, ITypeSymbol argument, INamedTypeSymbol constructed)
  {
    bool nullableValue = argument.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    if (parameter.HasReferenceTypeConstraint && !argument.IsReferenceType)
    {
      return false;
    }

    if (parameter.HasValueTypeConstraint && (!argument.IsValueType || nullableValue))
    {
      return false;
    }

    if (parameter.HasUnmanagedTypeConstraint && (!argument.IsUnmanagedType || nullableValue))
    {
      return false;
    }

    if (parameter.HasConstructorConstraint && !HasPublicParameterlessConstructor(argument))
    {
      return false;
    }

    if (argument.IsRefLikeType)
    {
      return false;
    }

    foreach (ITypeSymbol constraint in SubstitutedConstraints(parameter, constructed))
    {
      Conversion conversion = Compilation.ClassifyConversion(argument, constraint);
      if (!conversion.Exists || !(conversion.IsIdentity || conversion.IsReference || conversion.IsBoxing))
      {
        return false;
      }
    }

    return true;
  }

  // The constraint types of parameter with the constructed type's arguments substituted for the definition's type
  // parameters (IComparable<T> becomes IComparable<int>). A shape Substitute does not rewrite keeps its type parameters,
  // so the conversion check fails and the subtype stays TWSG002.
  private static IEnumerable<ITypeSymbol> SubstitutedConstraints(ITypeParameterSymbol parameter, INamedTypeSymbol constructed)
  {
    Dictionary<ITypeParameterSymbol, ITypeSymbol> map = new(SymbolEqualityComparer.Default);
    INamedTypeSymbol definition = constructed.OriginalDefinition;
    for (int index = 0; index < definition.TypeParameters.Length; index++)
    {
      map[definition.TypeParameters[index]] = constructed.TypeArguments[index];
    }

    foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
    {
      yield return Substitute(constraint, map);
    }
  }

  private static ITypeSymbol Substitute(ITypeSymbol type, Dictionary<ITypeParameterSymbol, ITypeSymbol> map) =>
    type switch
    {
      ITypeParameterSymbol parameter when map.TryGetValue(parameter, out ITypeSymbol? bound) => bound,
      INamedTypeSymbol { IsGenericType: true, ContainingType: null or { IsGenericType: false } } named =>
        named.OriginalDefinition.Construct(named.TypeArguments.Select(argument => Substitute(argument, map)).ToArray()),
      _ => type
    };

  private static bool HasPublicParameterlessConstructor(ITypeSymbol type)
  {
    if (type.IsValueType)
    {
      return true;
    }

    return type is INamedTypeSymbol { IsAbstract: false } named
      && named.InstanceConstructors.Any(constructor => constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility == Accessibility.Public);
  }

  private static string HiddenSubtypeDetail(INamedTypeSymbol hidden) =>
    $"its subtype '{hidden.ToDisplayString()}' is private, protected, file-local, internal to another assembly, or generic " +
    "with type parameters the member type does not determine, so generated code cannot clone it; implement ICloneable " +
    "on that subtype, or make it accessible and closable from the member type";

  private bool CanCase(INamedTypeSymbol type) => CanName(type) && !(type.IsGenericType && type.IsDefinition);

  private static bool IsConcreteSubtype(INamedTypeSymbol type, INamedTypeSymbol target, bool implementations)
  {
    if (type.IsAbstract || SymbolEqualityComparer.Default.Equals(type, target))
    {
      return false;
    }

    if (!implementations)
    {
      return type.TypeKind == TypeKind.Class && Inherits(type, target);
    }

    return type.TypeKind is TypeKind.Class or TypeKind.Struct
      && (type.AllInterfaces.Any(implemented => SymbolEqualityComparer.Default.Equals(implemented, target)) || Inherits(type, target));
  }

  // Source types, plus, for a target declared in a non-framework referenced assembly, the types of that assembly and of
  // the non-framework referenced assemblies that reference it (a contracts project and its siblings). A subtype in an
  // assembly this compilation does not reference cannot be seen, and stays a runtime throw.
  private IEnumerable<INamedTypeSymbol> CandidateTypes(INamedTypeSymbol target)
  {
    IAssemblySymbol? declaring = target.OriginalDefinition.ContainingAssembly;
    if (declaring is null || SymbolEqualityComparer.Default.Equals(declaring, Compilation.Assembly) || IsFrameworkAssembly(declaring))
    {
      return SourceTypes;
    }

    IEnumerable<INamedTypeSymbol> candidates = SourceTypes.Concat(TypesOf(declaring));
    foreach (IAssemblySymbol referenced in Compilation.SourceModule.ReferencedAssemblySymbols)
    {
      if (SymbolEqualityComparer.Default.Equals(referenced, declaring) || IsFrameworkAssembly(referenced))
      {
        continue;
      }

      if (referenced.Modules.Any(module => module.ReferencedAssemblySymbols.Contains(declaring, SymbolEqualityComparer.Default)))
      {
        candidates = candidates.Concat(TypesOf(referenced));
      }
    }

    return candidates;
  }

  private List<INamedTypeSymbol> TypesOf(IAssemblySymbol assembly)
  {
    if (!AssemblyTypes.TryGetValue(assembly, out List<INamedTypeSymbol>? types))
    {
      types = [];
      CollectTypes(assembly.GlobalNamespace, types);
      AssemblyTypes.Add(assembly, types);
    }

    return types;
  }

  // Framework assemblies (the BCL and Microsoft.* packages): too large to scan for subtypes, and a ProduceReferenceAssembly
  // hint is not actionable for them.
  private static bool IsFrameworkAssembly(IAssemblySymbol assembly)
  {
    string name = assembly.Name;
    return name is "mscorlib" or "netstandard" or "System" or "Microsoft"
      || name.StartsWith("System.", StringComparison.Ordinal)
      || name.StartsWith("Microsoft.", StringComparison.Ordinal);
  }

  private static bool Inherits(INamedTypeSymbol type, ITypeSymbol target)
  {
    for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, target))
      {
        return true;
      }
    }

    return false;
  }

  private static int Depth(INamedTypeSymbol type)
  {
    int depth = 0;
    for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      depth++;
    }

    return depth;
  }

  private bool TryElementType(INamedTypeSymbol type, out ITypeSymbol? element)
  {
    foreach (string metadataName in new[]
    {
      "System.Collections.Generic.IEnumerable`1",
      "System.Collections.Generic.ICollection`1",
      "System.Collections.Generic.IList`1"
    })
    {
      INamedTypeSymbol? definition = Compilation.GetTypeByMetadataName(metadataName);
      if (definition is null)
      {
        continue;
      }

      INamedTypeSymbol? implemented = SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition)
        ? type
        : type.AllInterfaces.FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, definition));
      if (implemented is not null)
      {
        element = implemented.TypeArguments[0];
        return true;
      }
    }

    element = null;
    return false;
  }

  private bool TryDictionaryArguments(INamedTypeSymbol type, out ITypeSymbol? key, out ITypeSymbol? value)
  {
    foreach (string metadataName in new[]
    {
      "System.Collections.Generic.IDictionary`2",
      "System.Collections.Generic.IReadOnlyDictionary`2"
    })
    {
      INamedTypeSymbol? definition = Compilation.GetTypeByMetadataName(metadataName);
      if (definition is null)
      {
        continue;
      }

      INamedTypeSymbol? implemented = SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition)
        ? type
        : type.AllInterfaces.FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, definition));
      if (implemented is not null)
      {
        key = implemented.TypeArguments[0];
        value = implemented.TypeArguments[1];
        return true;
      }
    }

    key = null;
    value = null;
    return false;
  }

  private INamedTypeSymbol? CollectionDefinition(INamedTypeSymbol type)
  {
    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      if (MatchConcrete(current) != CollectionKind.None)
      {
        return current.OriginalDefinition;
      }
    }

    return null;
  }

  private CollectionKind CollectionKindOf(INamedTypeSymbol type)
  {
    if (type.TypeKind == TypeKind.Interface)
    {
      return MatchInterface(type);
    }

    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      CollectionKind kind = MatchConcrete(current);
      if (kind != CollectionKind.None)
      {
        return kind;
      }
    }

    return CollectionKind.None;
  }

  private CollectionKind MatchInterface(INamedTypeSymbol type)
  {
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IDictionary`2")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IReadOnlyDictionary`2"))
    {
      return CollectionKind.DictionaryInterface;
    }

    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IList`1")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.ICollection`1")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IReadOnlyList`1")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IReadOnlyCollection`1")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IEnumerable`1")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.ISet`1")
        || SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.IReadOnlySet`1"))
    {
      return CollectionKind.EnumerableInterface;
    }

    return CollectionKind.None;
  }

  private static readonly (string MetadataName, CollectionKind Kind)[] ConcreteCollections =
  [
    ("System.Collections.Generic.List`1", CollectionKind.List),
    ("System.Collections.Generic.Dictionary`2", CollectionKind.Dictionary),
    ("System.Collections.Generic.HashSet`1", CollectionKind.HashSet),
    ("System.Collections.Generic.SortedSet`1", CollectionKind.SortedSet),
    ("System.Collections.Generic.SortedDictionary`2", CollectionKind.SortedDictionary),
    ("System.Collections.Generic.SortedList`2", CollectionKind.SortedList),
    ("System.Collections.Generic.LinkedList`1", CollectionKind.LinkedList),
    ("System.Collections.Generic.Stack`1", CollectionKind.Stack),
    ("System.Collections.Generic.Queue`1", CollectionKind.Queue),
    ("System.Collections.Concurrent.ConcurrentDictionary`2", CollectionKind.ConcurrentDictionary),
    ("System.Collections.ObjectModel.ObservableCollection`1", CollectionKind.ObservableCollection),
    ("System.Collections.ObjectModel.Collection`1", CollectionKind.Collection),
    ("System.Collections.ObjectModel.ReadOnlyCollection`1", CollectionKind.ReadOnlyCollection),
    ("System.Collections.ObjectModel.ReadOnlyDictionary`2", CollectionKind.ReadOnlyDictionary),
    ("System.Collections.Immutable.ImmutableArray`1", CollectionKind.ImmutableArray),
    ("System.Collections.Immutable.ImmutableList`1", CollectionKind.ImmutableList),
    ("System.Collections.Immutable.ImmutableDictionary`2", CollectionKind.ImmutableDictionary),
    ("System.Collections.Immutable.ImmutableSortedDictionary`2", CollectionKind.ImmutableSortedDictionary),
    ("System.Collections.Immutable.ImmutableHashSet`1", CollectionKind.ImmutableHashSet),
    ("System.Collections.Immutable.ImmutableSortedSet`1", CollectionKind.ImmutableSortedSet),
    ("System.Collections.Immutable.ImmutableQueue`1", CollectionKind.ImmutableQueue),
    ("System.Collections.Immutable.ImmutableStack`1", CollectionKind.ImmutableStack),
    ("System.Collections.Frozen.FrozenDictionary`2", CollectionKind.FrozenDictionary),
    ("System.Collections.Frozen.FrozenSet`1", CollectionKind.FrozenSet)
  ];

  private CollectionKind MatchConcrete(INamedTypeSymbol type)
  {
    foreach ((string metadataName, CollectionKind kind) in ConcreteCollections)
    {
      if (SymbolEquals(type.OriginalDefinition, metadataName))
      {
        return kind;
      }
    }

    return CollectionKind.None;
  }

  private static string DefinitionName(CollectionKind kind) =>
    ConcreteCollections.FirstOrDefault(entry => entry.Kind == kind).MetadataName ?? string.Empty;

  private bool SymbolEquals(INamedTypeSymbol type, string metadataName)
  {
    if (metadataName.Length == 0)
    {
      return false;
    }

    INamedTypeSymbol? definition = Compilation.GetTypeByMetadataName(metadataName);
    return definition is not null && SymbolEqualityComparer.Default.Equals(type, definition);
  }

  private bool IsShared(ITypeSymbol type)
  {
    if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
    {
      return IsShared(nullable.TypeArguments[0]);
    }

    if (type.TypeKind is TypeKind.Enum or TypeKind.Delegate)
    {
      return true;
    }

    if (type.SpecialType is SpecialType.System_Boolean
        or SpecialType.System_Byte
        or SpecialType.System_SByte
        or SpecialType.System_Int16
        or SpecialType.System_UInt16
        or SpecialType.System_Int32
        or SpecialType.System_UInt32
        or SpecialType.System_Int64
        or SpecialType.System_UInt64
        or SpecialType.System_Single
        or SpecialType.System_Double
        or SpecialType.System_Char
        or SpecialType.System_String
        or SpecialType.System_Decimal
        or SpecialType.System_DateTime
        or SpecialType.System_IntPtr
        or SpecialType.System_UIntPtr)
    {
      return true;
    }

    if (Named(type, "System", "DateTimeOffset")
        || Named(type, "System", "TimeSpan")
        || Named(type, "System", "DateOnly")
        || Named(type, "System", "TimeOnly")
        || Named(type, "System", "Guid")
        || Named(type, "System", "Half")
        || Named(type, "System", "Int128")
        || Named(type, "System", "UInt128")
        || Named(type, "System", "Uri")
        || Named(type, "System", "Version")
        || Named(type, "System", "DBNull")
        || Named(type, "System", "TimeZoneInfo")
        || Named(type, "System.Numerics", "BigInteger"))
    {
      return true;
    }

    if (AssignableTo(type, "System.Delegate")
        || AssignableTo(type, "System.Reflection.MemberInfo")
        || AssignableTo(type, "System.Reflection.Assembly")
        || AssignableTo(type, "System.Reflection.Module")
        || AssignableTo(type, "System.Reflection.ParameterInfo")
        || AssignableTo(type, "System.Globalization.CultureInfo")
        || AssignableTo(type, "System.Text.RegularExpressions.Regex")
        || AssignableTo(type, "System.Text.Encoding")
        || AssignableTo(type, "System.IServiceProvider"))
    {
      return true;
    }

    // Synchronization primitives, timers, tasks, cancellation tokens and sources are identity-bound.
    string? namespaceName = type.ContainingNamespace?.ToDisplayString();
    if (namespaceName is "System.Threading" or "System.Threading.Tasks")
    {
      return true;
    }

    return IsComparer(type);
  }

  private bool IsComparer(ITypeSymbol type)
  {
    if (type.IsValueType)
    {
      return false;
    }

    if (AssignableTo(type, "System.Collections.IEqualityComparer") || AssignableTo(type, "System.Collections.IComparer"))
    {
      return true;
    }

    foreach (INamedTypeSymbol implemented in type.AllInterfaces)
    {
      if (SymbolEquals(implemented.OriginalDefinition, "System.Collections.Generic.IEqualityComparer`1")
          || SymbolEquals(implemented.OriginalDefinition, "System.Collections.Generic.IComparer`1"))
      {
        return true;
      }
    }

    if (type is INamedTypeSymbol { TypeKind: TypeKind.Interface } named
        && (SymbolEquals(named.OriginalDefinition, "System.Collections.Generic.IEqualityComparer`1")
            || SymbolEquals(named.OriginalDefinition, "System.Collections.Generic.IComparer`1")))
    {
      return true;
    }

    return false;
  }

  private bool AssignableTo(ITypeSymbol type, string metadataName)
  {
    INamedTypeSymbol? target = Compilation.GetTypeByMetadataName(metadataName);
    if (target is null)
    {
      return false;
    }

    if (SymbolEqualityComparer.Default.Equals(type, target) || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, target))
    {
      return true;
    }

    if (type.AllInterfaces.Any(implemented =>
          SymbolEqualityComparer.Default.Equals(implemented, target)
          || SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, target)))
    {
      return true;
    }

    for (ITypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current, target) || SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, target))
      {
        return true;
      }
    }

    return false;
  }

  private static bool Named(ITypeSymbol type, string namespaceName, string name) =>
    type.Name == name && type.ContainingNamespace?.ToDisplayString() == namespaceName;

  private bool IsState(INamedTypeSymbol type)
  {
    if (StateType is null)
    {
      return false;
    }

    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, StateType))
      {
        return true;
      }
    }

    return false;
  }

  private static bool ImplementsICloneable(ITypeSymbol type) =>
    type.AllInterfaces.Any(implemented => implemented.Name == "ICloneable" && implemented.ContainingNamespace?.ToDisplayString() == "System");

  private static bool HasGenerateClone(ITypeSymbol type) =>
    type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is "GenerateClone" or "GenerateCloneAttribute");

  private static string CopyExpression(Slot slot, string sourceExpression)
  {
    if (slot.Kind == SlotKind.Share)
    {
      return sourceExpression;
    }

    if (slot.IsValueType)
    {
      return $"{slot.MethodName}({sourceExpression}, map)";
    }

    return $"{sourceExpression} is null ? null! : {slot.MethodName}({sourceExpression}, map)";
  }

  #endregion

  #region Diagnostics

  private static void Fail(Slot slot, string detail, Location? location, Slot? cause = null, bool member = false)
  {
    if (slot.PrimaryFailure)
    {
      return;
    }

    slot.Kind = SlotKind.Error;
    slot.Detail ??= detail;
    slot.DiagnosticLocation ??= location ?? Location.None;
    slot.PrimaryFailure = true;
    slot.Cause = cause;
    slot.MemberFailure = member;
  }

  private static Location? LocationOf(ITypeSymbol type) =>
    type.Locations.FirstOrDefault(location => location.IsInSource) ?? type.Locations.FirstOrDefault();

  private static Location? SourceLocation(ITypeSymbol type) =>
    type.OriginalDefinition.Locations.FirstOrDefault(location => location.IsInSource);

  private static Location? FieldLocation(IFieldSymbol field) =>
    field.Locations.FirstOrDefault(location => location.IsInSource)
    ?? field.AssociatedSymbol?.Locations.FirstOrDefault(location => location.IsInSource);

  private void PropagateErrors()
  {
    bool changed = true;
    while (changed)
    {
      changed = false;
      foreach (Slot slot in Slots.Values)
      {
        if (slot.Kind is SlotKind.Error or SlotKind.Share)
        {
          continue;
        }

        foreach (Slot dependency in slot.Dependencies)
        {
          if (dependency.Kind != SlotKind.Error)
          {
            continue;
          }

          slot.Kind = SlotKind.Error;
          slot.Detail ??= $"it depends on '{dependency.Type.ToDisplayString()}', which cannot be cloned";
          changed = true;
          break;
        }
      }
    }
  }

  // One diagnostic per primary failure that has a source location: the member (or type) the author can change.
  // Failures inside BCL or referenced types surface through the source member that reaches them. A failed root with
  // no reported cause still gets one diagnostic at its declaration.
  private void Report(SourceProductionContext sourceContext)
  {
    Dictionary<Slot, Slot> rootOf = [];
    foreach (Slot root in Roots)
    {
      Mark(root, root, rootOf);
    }

    HashSet<string> reported = new(StringComparer.Ordinal);
    HashSet<Slot> reportedRoots = [];
    foreach (KeyValuePair<Slot, Slot> pair in rootOf)
    {
      Slot slot = pair.Key;
      if (slot.Kind != SlotKind.Error || !slot.PrimaryFailure || slot.DiagnosticLocation is not { IsInSource: true } location)
      {
        continue;
      }

      // A type-level failure (no implementation, no constructor, ...) is reported through the member that reaches
      // it, unless the type is itself a root. A member failure caused by a deeper source member is left to that one.
      if (!slot.MemberFailure && !slot.IsRoot || HasDeeperMemberFailure(slot))
      {
        continue;
      }

      if (!reported.Add(slot.FullyQualified + "|" + slot.Detail + "|" + location.GetLineSpan()))
      {
        continue;
      }

      reportedRoots.Add(pair.Value);
      sourceContext.ReportDiagnostic(Diagnostic.Create(
        StateCloneSourceGenerator.UnsupportedRule,
        location,
        slot.Type.ToDisplayString(),
        slot.Detail,
        RootSuffix(slot, pair.Value)));
    }

    foreach (Slot root in Roots)
    {
      if (root.Kind != SlotKind.Error || reportedRoots.Contains(root) || Roots.Any(other => other != root && reportedRoots.Contains(other) && Reaches(other, root)))
      {
        continue;
      }

      if (!AnyReportedBelow(root, rootOf, reportedRoots))
      {
        sourceContext.ReportDiagnostic(Diagnostic.Create(
          StateCloneSourceGenerator.UnsupportedRule,
          SourceLocation(root.Type) ?? Location.None,
          root.Type.ToDisplayString(),
          root.Detail ?? "a member cannot be cloned",
          string.Empty));
      }
    }
  }

  private static bool HasDeeperMemberFailure(Slot slot)
  {
    HashSet<Slot> seen = [slot];
    for (Slot? cause = slot.Cause; cause is not null && seen.Add(cause); cause = cause.Cause)
    {
      if (cause.PrimaryFailure && cause.MemberFailure && cause.DiagnosticLocation is { IsInSource: true })
      {
        return true;
      }
    }

    return false;
  }

  private static bool AnyReportedBelow(Slot root, Dictionary<Slot, Slot> rootOf, HashSet<Slot> reportedRoots)
  {
    HashSet<Slot> seen = [];
    Stack<Slot> pending = new();
    pending.Push(root);
    while (pending.Count > 0)
    {
      Slot current = pending.Pop();
      if (!seen.Add(current))
      {
        continue;
      }

      if (current.Kind == SlotKind.Error && current.PrimaryFailure && current.DiagnosticLocation is { IsInSource: true }
          && rootOf.TryGetValue(current, out Slot? owner) && reportedRoots.Contains(owner))
      {
        return true;
      }

      foreach (Slot dependency in current.Dependencies)
      {
        pending.Push(dependency);
      }
    }

    return false;
  }

  private static bool Reaches(Slot from, Slot to)
  {
    HashSet<Slot> seen = [];
    Stack<Slot> pending = new();
    pending.Push(from);
    while (pending.Count > 0)
    {
      Slot current = pending.Pop();
      if (current == to)
      {
        return true;
      }

      if (!seen.Add(current))
      {
        continue;
      }

      foreach (Slot dependency in current.Dependencies)
      {
        pending.Push(dependency);
      }
    }

    return false;
  }

  private static string RootSuffix(Slot slot, Slot root) =>
    slot == root ? string.Empty : $" (reached from '{root.Type.ToDisplayString()}')";

  private static void Mark(Slot slot, Slot root, Dictionary<Slot, Slot> rootOf)
  {
    Stack<Slot> pending = new();
    pending.Push(slot);
    while (pending.Count > 0)
    {
      Slot current = pending.Pop();
      if (rootOf.ContainsKey(current))
      {
        continue;
      }

      rootOf.Add(current, root);
      foreach (Slot dependency in current.Dependencies)
      {
        pending.Push(dependency);
      }
    }
  }

  #endregion

  #region Emit

  private void MaterializeDispatches()
  {
    foreach (Slot slot in Slots.Values)
    {
      if (!slot.IsDispatch || slot.Kind != SlotKind.Clone)
      {
        continue;
      }

      string name = slot.FullyQualified;
      List<string> lines =
      [
        $"if (map.TryGet(source, out {name}? existing))",
        "{",
        "  return existing;",
        "}"
      ];

      if (slot.Exact is not null && !slot.ExactIsFallback)
      {
        lines.Add($"if (source.GetType() == typeof({name}))");
        lines.Add("{");
        lines.Add($"  return {slot.Exact.MethodName}(source, map);");
        lines.Add("}");
      }

      lines.Add("switch (source)");
      lines.Add("{");
      HashSet<string> seen = new(StringComparer.Ordinal);
      List<CaseRequest> ordered = [];
      int typedIndex = 0;
      bool cloneableCase = !slot.ExactIsFallback;
      foreach (CaseRequest caseRequest in slot.Cases)
      {
        ITypeSymbol type = Normalize(caseRequest.Type);
        if (!Slots.TryGetValue(type, out Slot? target) || target.Kind is not (SlotKind.Clone or SlotKind.Share))
        {
          continue;
        }

        // An ICloneable subtype goes to the ICloneable case, which casts its Clone() result to this slot's type.
        if (target.IsCloneable)
        {
          cloneableCase = true;
          continue;
        }

        if (!seen.Add(type.ToDisplayString(Format)))
        {
          continue;
        }

        string variable = caseRequest.Variable == "typed" ? $"typed{typedIndex++}" : caseRequest.Variable;
        int depth = type is INamedTypeSymbol named ? Depth(named) : 1;
        ordered.Add(new CaseRequest(type, variable, depth, caseRequest.Guard));
      }

      foreach (CaseRequest caseRequest in ordered
        .OrderByDescending(item => item.Depth)
        .ThenBy(item => item.Variable, StringComparer.Ordinal))
      {
        Slot target = Slots[Normalize(caseRequest.Type)];
        string display = caseRequest.Type.ToDisplayString(Format);
        string guard = caseRequest.Guard ? $" when {caseRequest.Variable}.GetType() == typeof({display})" : string.Empty;
        lines.Add($"  case {display} {caseRequest.Variable}{guard}:");
        lines.Add(target.Kind == SlotKind.Share
          ? "    return source;"
          : $"    return {target.MethodName}({caseRequest.Variable}, map);");
      }

      if (cloneableCase)
      {
        lines.Add("  case global::System.ICloneable cloneable:");
        lines.Add("  {");
        lines.Add("    object? cloned = cloneable.Clone();");
        lines.Add("    if (cloned is null)");
        lines.Add("    {");
        lines.Add("      return null!;");
        lines.Add("    }");
        lines.Add($"    {name} typedClone = ({name})cloned;");
        lines.Add("    map.Add(source, typedClone);");
        lines.Add("    return typedClone;");
        lines.Add("  }");
      }

      if (slot.Exact is not null && slot.ExactIsFallback)
      {
        lines.Add("  default:");
        lines.Add($"    return {slot.Exact.MethodName}(source, map);");
        lines.Add("}");
        slot.Statements = lines;
        continue;
      }

      lines.Add("  default:");
      if (slot.Fallback is not null)
      {
        lines.AddRange(slot.Fallback.Select(line => "  " + line));
      }
      else
      {
        string message = $"Cannot clone \" + source.GetType().FullName + \" as {slot.Type.ToDisplayString().Replace("\"", "\\\"")}: " +
          "the clone source generator did not see that type at build time (it is declared in an assembly this project " +
          "does not reference, or in a framework assembly). Implement ICloneable on it.";
        lines.Add($"    throw new global::System.InvalidOperationException(\"{message}\");");
      }

      lines.Add("}");
      slot.Statements = lines;
    }
  }

  private string? Emit()
  {
    List<Slot> methods = Slots.Values
      .Where(slot => slot.Kind == SlotKind.Clone && slot.Statements is not null)
      .SelectMany(slot => slot.Exact is null ? [slot] : new[] { slot, slot.Exact })
      .OrderBy(slot => slot.MethodName, StringComparer.Ordinal)
      .ToList();
    List<Slot> roots = Roots.Where(slot => slot.Kind == SlotKind.Clone).Distinct().ToList();
    if (methods.Count == 0 && roots.Count == 0)
    {
      return null;
    }

    const string Host = "global::TimeWarp.State.Cloning.Generated.TimeWarpStateClones";
    StringBuilder builder = new();
    builder.AppendLine("// <auto-generated/>");
    builder.AppendLine("#nullable enable");
    builder.AppendLine("#pragma warning disable CS1591");
    builder.AppendLine("#pragma warning disable CS8600");
    builder.AppendLine("#pragma warning disable CS8601");
    builder.AppendLine("#pragma warning disable CS8602");
    builder.AppendLine("#pragma warning disable CS8603");
    builder.AppendLine("#pragma warning disable CS8604");
    builder.AppendLine("#pragma warning disable CS8625");
    builder.AppendLine();
    builder.AppendLine("namespace TimeWarp.Features.Cloning");
    builder.AppendLine("{");
    // TimeWarp.Fixie discovers every public method. Clone has parameters, so each one shows up as a skipped test.
    // The attribute type is TimeWarp.Fixie.NotTest (no Attribute suffix).
    if (Compilation.GetTypeByMetadataName("TimeWarp.Fixie.NotTest") is not null
        || Compilation.GetTypeByMetadataName("TimeWarp.Fixie.NotTestAttribute") is not null)
    {
      builder.AppendLine("  [global::TimeWarp.Fixie.NotTest]");
    }

    builder.AppendLine("  public static class GeneratedCloneExtensions");
    builder.AppendLine("  {");
    foreach (Slot root in roots)
    {
      // A public extension cannot name an internal or nested non-public type (CS0050/CS0051).
      string visibility = IsPubliclyVisible(root.Type) ? "public" : "internal";
      if (root.IsValueType)
      {
        builder.AppendLine($"    {visibility} static {root.FullyQualified} Clone(this {root.FullyQualified} source) =>");
        builder.AppendLine($"      {Host}.{root.MethodName}(source, new global::TimeWarp.Features.Cloning.CloneMap());");
      }
      else
      {
        builder.AppendLine($"    {visibility} static {root.FullyQualified}? Clone(this {root.FullyQualified}? source) =>");
        builder.AppendLine($"      source is null ? null : {Host}.{root.MethodName}(source, new global::TimeWarp.Features.Cloning.CloneMap());");
      }

      builder.AppendLine();
    }

    builder.AppendLine("  }");
    builder.AppendLine("}");
    builder.AppendLine();
    builder.AppendLine("namespace TimeWarp.State.Cloning.Generated");
    builder.AppendLine("{");
    builder.AppendLine("  file static class TimeWarpStateClones");
    builder.AppendLine("  {");
    foreach (Slot slot in methods)
    {
      string parameters = $"{slot.FullyQualified} source, global::TimeWarp.Features.Cloning.CloneMap map";
      builder.AppendLine($"    public static {slot.FullyQualified} {slot.MethodName}({parameters})");
      builder.AppendLine("    {");
      foreach (string statement in slot.Statements!)
      {
        builder.Append("      ");
        builder.AppendLine(statement);
      }

      builder.AppendLine("    }");
      builder.AppendLine();
    }

    List<Slot> registrations = roots.Where(slot => slot.RegisterState && !slot.IsValueType).ToList();
    if (registrations.Count > 0)
    {
      builder.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
      builder.AppendLine("    internal static void Register()");
      builder.AppendLine("    {");
      foreach (Slot slot in registrations)
      {
        builder.AppendLine($"      global::TimeWarp.Features.Cloning.StateCloneRegistry.Register<{slot.FullyQualified}>(static state => {slot.MethodName}(state, new global::TimeWarp.Features.Cloning.CloneMap()));");
      }

      builder.AppendLine("    }");
    }

    builder.AppendLine("  }");
    foreach (Holder holder in Holders.Values
      .Where(holder => holder.Problem is null && holder.Members.Count > 0)
      .OrderBy(holder => holder.Name, StringComparer.Ordinal))
    {
      builder.AppendLine();
      builder.AppendLine($"  file static class {holder.Name}{holder.TypeParameters}");
      foreach (string constraint in holder.Constraints)
      {
        builder.AppendLine($"    {constraint}");
      }

      builder.AppendLine("  {");
      foreach (string member in holder.Members)
      {
        builder.AppendLine($"    {member}");
      }

      builder.AppendLine("  }");
    }

    builder.AppendLine("}");
    return builder.ToString();
  }

  private static bool IsPubliclyVisible(ITypeSymbol type)
  {
    for (ISymbol? symbol = type; symbol is not null and not INamespaceSymbol; symbol = symbol.ContainingSymbol)
    {
      if (symbol.DeclaredAccessibility != Accessibility.Public)
      {
        return false;
      }
    }

    return true;
  }

  private string UniqueName(string prefix, string identity)
  {
    string trimmed = prefix.Length > 80 ? prefix.Substring(0, 80) : prefix;
    string name = $"{trimmed}_{StableHash(identity)}";
    string candidate = name;
    int suffix = 1;
    while (!UsedNames.Add(candidate))
    {
      candidate = $"{name}_{suffix++}";
    }

    return candidate;
  }

  // FNV-1a over the fully qualified name: stable across builds and machines, unlike string.GetHashCode.
  private static string StableHash(string text)
  {
    uint hash = 2166136261;
    foreach (char character in text)
    {
      hash ^= character;
      hash *= 16777619;
    }

    return hash.ToString("x8");
  }

  private static string Sanitize(string name)
  {
    StringBuilder builder = new(name.Length);
    foreach (char character in name)
    {
      builder.Append(char.IsLetterOrDigit(character) ? character : '_');
    }

    return builder.ToString();
  }

  #endregion

  #region Types

  private enum SlotKind
  {
    Pending,
    Share,
    Clone,
    Error
  }

  private enum CollectionKind
  {
    None,
    List,
    Dictionary,
    HashSet,
    SortedSet,
    SortedDictionary,
    SortedList,
    LinkedList,
    Stack,
    Queue,
    ConcurrentDictionary,
    Collection,
    ObservableCollection,
    ReadOnlyCollection,
    ReadOnlyDictionary,
    ImmutableArray,
    ImmutableList,
    ImmutableDictionary,
    ImmutableSortedDictionary,
    ImmutableHashSet,
    ImmutableSortedSet,
    ImmutableQueue,
    ImmutableStack,
    FrozenDictionary,
    FrozenSet,
    EnumerableInterface,
    DictionaryInterface
  }

  private readonly struct Member
  {
    public Member(ITypeSymbol type, ITypeSymbol definitionType, string fieldName, INamedTypeSymbol declaringType, Location? location)
    {
      Type = type;
      DefinitionType = definitionType;
      FieldName = fieldName;
      DeclaringType = declaringType;
      Location = location;
    }

    public ITypeSymbol Type { get; }
    public ITypeSymbol DefinitionType { get; }
    public string FieldName { get; }
    public INamedTypeSymbol DeclaringType { get; }
    public Location? Location { get; }
  }

  private sealed class CaseRequest
  {
    public CaseRequest(ITypeSymbol type, string variable, int depth, bool guard)
    {
      Type = type;
      Variable = variable;
      Depth = depth;
      Guard = guard;
    }

    public ITypeSymbol Type { get; }
    public string Variable { get; }
    public int Depth { get; }
    public bool Guard { get; }
  }

  private sealed class Holder
  {
    public INamedTypeSymbol Definition { get; init; } = null!;
    public Dictionary<ITypeParameterSymbol, string> Map { get; init; } = null!;
    public string Name { get; init; } = "";
    public string TypeParameters { get; init; } = "";
    public string Self { get; init; } = "";
    public string? Problem { get; set; }
    public List<string> Constraints { get; } = [];
    public List<string> Members { get; } = [];
    public Dictionary<string, string> Methods { get; } = new(StringComparer.Ordinal);
  }

  private sealed class Slot
  {
    public ITypeSymbol Type { get; init; } = null!;
    public string FullyQualified { get; init; } = "";
    public string MethodName { get; init; } = "";
    public bool IsValueType { get; init; }
    public SlotKind Kind { get; set; }
    public bool IsRoot { get; set; }
    public bool RegisterState { get; set; }
    public bool IsDispatch { get; set; }
    public bool IsCloneable { get; set; }
    public bool PrimaryFailure { get; set; }
    public bool MemberFailure { get; set; }
    public Slot? Cause { get; set; }
    public string? Detail { get; set; }
    public Location? DiagnosticLocation { get; set; }
    public List<string>? Statements { get; set; }
    public List<string>? Fallback { get; set; }
    public Slot? Exact { get; set; }
    public bool ExactIsFallback { get; set; }
    public List<Slot> Dependencies { get; } = [];
    public List<CaseRequest> Cases { get; } = [];
  }

  #endregion
}
