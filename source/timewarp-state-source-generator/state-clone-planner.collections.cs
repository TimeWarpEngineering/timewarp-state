#region Purpose
// Rebuilds BCL collections and materializes collection interfaces.
#endregion

#region Design
// Immutable collections with shared elements stay shared. A collection subclass passes source.Comparer when a constructor accepts it. Collection interfaces switch on known implementations and fall back to List, HashSet, or Dictionary.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

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
        else if (!TryEmitSubclassConstruction(type, kind, lines, out string? problem))
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
    else if (!TryEmitSubclassConstruction(type, kind, lines, out string? problem))
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
    INamedTypeSymbol? fallback = Catalog.Resolve(definition)?.Construct(elementType);
    if (fallback is null || !Compilation.HasImplicitConversion(fallback, type))
    {
      return;
    }

    slot.Fallback = MaterializeFallback(
      fallback,
      "foreach (var item in source)",
      $"materialized.Add({CopyExpression(element, "item")});");
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

    INamedTypeSymbol? fallback = Catalog.Resolve("System.Collections.Generic.Dictionary`2")?.Construct(arguments);
    if (fallback is null || !Compilation.HasImplicitConversion(fallback, type))
    {
      return;
    }

    slot.Fallback = MaterializeFallback(
      fallback,
      "foreach (var pair in source)",
      $"materialized.Add({CopyExpression(keySlot, "pair.Key")}, {CopyExpression(valueSlot, "pair.Value")});");
  }

  private static List<string> MaterializeFallback(INamedTypeSymbol fallback, string loop, string add) =>
  [
    "{",
    $"  {fallback.ToDisplayString(Format)} materialized = new();",
    "  map.Add(source, materialized);",
    $"  {loop}",
    "  {",
    $"    {add}",
    "  }",
    "  return materialized;",
    "}"
  ];

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
    INamedTypeSymbol? definition = Catalog.Resolve(metadataName);
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

}
