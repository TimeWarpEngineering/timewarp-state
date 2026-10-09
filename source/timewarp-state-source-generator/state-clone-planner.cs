#region Purpose
// Plans per-type clone methods for StateCloneSourceGenerator.
#endregion

#region Design
// Share immutable static types. Clone everything else with constructors plus UnsafeAccessor field writes.
// Collections are rebuilt through their public API. Cycles use a method name assigned before the body exists.
// Diagnostics are reported only for types reachable from a state or [GenerateClone] root.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class StateClonePlanner
{
  private static readonly SymbolDisplayFormat Format = SymbolDisplayFormat.FullyQualifiedFormat;

  private readonly Compilation Compilation;
  private readonly Dictionary<ITypeSymbol, Slot> Slots = new(SymbolEqualityComparer.Default);
  private readonly List<Slot> Roots = [];
  private readonly INamedTypeSymbol? StateType;

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

    Walk(Compilation.Assembly.GlobalNamespace);
    CollectClosedGenerateCloneTypes();
    PropagateErrors();
    Report(sourceContext);
    MaterializeDispatches();
    return Emit();
  }

  private void Walk(INamespaceSymbol namespaceSymbol)
  {
    foreach (INamespaceSymbol child in namespaceSymbol.GetNamespaceMembers())
    {
      Walk(child);
    }

    foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
    {
      WalkType(type);
    }
  }

  private void WalkType(INamedTypeSymbol type)
  {
    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      WalkType(nested);
    }

    Consider(type);
  }

  private void CollectClosedGenerateCloneTypes()
  {
    foreach (SyntaxTree tree in Compilation.SyntaxTrees)
    {
      SemanticModel model = Compilation.GetSemanticModel(tree);
      foreach (GenericNameSyntax genericName in tree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>())
      {
        if (model.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol symbol)
        {
          continue;
        }

        if (symbol.IsUnboundGenericType || symbol.TypeArguments.Any(argument => argument.TypeKind == TypeKind.TypeParameter))
        {
          continue;
        }

        if (HasGenerateClone(symbol) || HasGenerateClone(symbol.OriginalDefinition))
        {
          Consider(symbol);
        }
      }
    }
  }

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

  private Slot Build(ITypeSymbol type)
  {
    type = Normalize(type);
    if (Slots.TryGetValue(type, out Slot? existing))
    {
      return existing;
    }

    Slot slot = GetOrCreate(type);
    if (slot.Kind != SlotKind.Pending)
    {
      return slot;
    }

    Classify(slot, type);
    return slot;
  }

  private static ITypeSymbol Normalize(ITypeSymbol type) =>
    type.WithNullableAnnotation(NullableAnnotation.None);

  private Slot GetOrCreate(ITypeSymbol type)
  {
    type = Normalize(type);
    if (Slots.TryGetValue(type, out Slot? existing))
    {
      return existing;
    }

    Slot slot = new()
    {
      Type = type,
      FullyQualified = type.ToDisplayString(Format),
      MethodName = "Clone_" + Sanitize(type.ToDisplayString()),
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

    if (ImplementsICloneable(named))
    {
      ClassifyCloneable(slot, named);
      return;
    }

    if (!CanName(named))
    {
      Fail(slot, "the type is not accessible to generated code", LocationOf(named));
      return;
    }

    if (named.SpecialType == SpecialType.System_Object)
    {
      Fail(slot, "System.Object can hold any runtime value", LocationOf(named));
      return;
    }

    CollectionKind collectionKind = CollectionKindOf(named);
    if (collectionKind != CollectionKind.None)
    {
      ClassifyCollection(slot, named, collectionKind);
      return;
    }

    if (named.TypeKind == TypeKind.Interface)
    {
      ClassifyInterface(slot, named);
      return;
    }

    if (named.IsAbstract)
    {
      ClassifyInterface(slot, named);
      return;
    }

    if (named.IsRefLikeType)
    {
      Fail(slot, "ref structs are not supported", LocationOf(named));
      return;
    }

    ClassifyObject(slot, named);
  }

  private void ClassifyArray(Slot slot, IArrayTypeSymbol array)
  {
    Slot element = Build(array.ElementType);
    slot.Dependencies.Add(element);
    if (element.Kind == SlotKind.Error)
    {
      Fail(slot, $"array element '{array.ElementType.ToDisplayString()}' cannot be cloned", LocationOf(array));
      return;
    }

    slot.Kind = SlotKind.Clone;
    string elementName = array.ElementType.ToDisplayString(Format);
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
    lines.Add($"{arrayName} clone = new {elementName}[{lengths}];");
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

  private void ClassifyCloneable(Slot slot, INamedTypeSymbol type)
  {
    slot.Kind = SlotKind.Clone;
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

  private void ClassifyCollection(Slot slot, INamedTypeSymbol type, CollectionKind kind)
  {
    if (kind is CollectionKind.Dictionary or CollectionKind.ImmutableDictionary or CollectionKind.FrozenDictionary)
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
        Fail(slot, "a dictionary key or value cannot be cloned", LocationOf(type));
        return;
      }

      if (kind == CollectionKind.FrozenDictionary)
      {
        if (keySlot.Kind == SlotKind.Share && valueSlot.Kind == SlotKind.Share)
        {
          slot.Kind = SlotKind.Share;
          return;
        }

        Fail(slot, "FrozenDictionary of mutable elements is not supported", LocationOf(type));
        return;
      }

      if (kind == CollectionKind.ImmutableDictionary && keySlot.Kind == SlotKind.Share && valueSlot.Kind == SlotKind.Share)
      {
        slot.Kind = SlotKind.Share;
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
      Fail(slot, $"element '{elementType!.ToDisplayString()}' cannot be cloned", LocationOf(type));
      return;
    }

    if (kind is CollectionKind.ImmutableArray or CollectionKind.ImmutableList or CollectionKind.ImmutableHashSet
        or CollectionKind.ImmutableQueue or CollectionKind.ImmutableStack or CollectionKind.FrozenSet)
    {
      if (element.Kind == SlotKind.Share)
      {
        slot.Kind = SlotKind.Share;
        return;
      }

      if (kind == CollectionKind.FrozenSet)
      {
        Fail(slot, "FrozenSet of mutable elements is not supported", LocationOf(type));
        return;
      }
    }

    ClassifyEnumerableCollection(slot, type, kind, element);
  }

  private void ClassifyDictionary(
    Slot slot,
    INamedTypeSymbol type,
    CollectionKind kind,
    Slot keySlot,
    Slot valueSlot)
  {
    slot.Kind = SlotKind.Clone;
    string name = type.ToDisplayString(Format);
    string keyCopy = CopyExpression(keySlot, "pair.Key");
    string valueCopy = CopyExpression(valueSlot, "pair.Value");
    List<string> lines = [];
    if (!type.IsValueType)
    {
      lines.Add($"if (map.TryGet(source, out {name}? existing))");
      lines.Add("{");
      lines.Add("  return existing;");
      lines.Add("}");
    }

    if (kind == CollectionKind.ImmutableDictionary)
    {
      lines.Add("var builder = global::System.Collections.Immutable.ImmutableDictionary.CreateBuilder<" +
                $"{keySlot.FullyQualified}, {valueSlot.FullyQualified}>(source.KeyComparer, source.ValueComparer);");
      lines.Add("foreach (var pair in source)");
      lines.Add("{");
      lines.Add($"  builder.Add({keyCopy}, {valueCopy});");
      lines.Add("}");
      lines.Add("return builder.ToImmutable();");
      slot.Statements = lines;
      return;
    }

    bool exact = SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.Dictionary`2");
    if (exact)
    {
      lines.Add($"{name} clone = new(source.Count, source.Comparer);");
    }
    else if (!TryEmitConstruction(type, lines, out string? problem))
    {
      Fail(slot, problem!, LocationOf(type));
      return;
    }

    lines.Add("map.Add(source, clone);");
    AppendExtraFields(slot, type, lines, StopBeforeDictionary(type));
    lines.Add("foreach (var pair in source)");
    lines.Add("{");
    lines.Add($"  clone.Add({keyCopy}, {valueCopy});");
    lines.Add("}");
    lines.Add("return clone;");
    slot.Statements = lines;
  }

  private void ClassifyEnumerableCollection(Slot slot, INamedTypeSymbol type, CollectionKind kind, Slot element)
  {
    if (type.TypeKind == TypeKind.Interface)
    {
      ClassifyCollectionInterface(slot, type, kind, element);
      return;
    }

    slot.Kind = SlotKind.Clone;
    string name = type.ToDisplayString(Format);
    string copy = CopyExpression(element, "item");
    List<string> lines = [];
    if (type.IsValueType)
    {
      AppendImmutableStruct(kind, lines, element, copy);
      slot.Statements = lines;
      return;
    }

    lines.Add($"if (map.TryGet(source, out {name}? existing))");
    lines.Add("{");
    lines.Add("  return existing;");
    lines.Add("}");

    bool exactList = SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.List`1");
    bool exactHash = SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.HashSet`1");
    bool exactStack = SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.Stack`1");
    bool exactQueue = SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.Queue`1");
    bool exactObservable = SymbolEquals(type.OriginalDefinition, "System.Collections.ObjectModel.ObservableCollection`1");
    bool exactCollection = SymbolEquals(type.OriginalDefinition, "System.Collections.ObjectModel.Collection`1");

    if (exactList)
    {
      lines.Add($"{name} clone = new(source.Count);");
    }
    else if (exactHash)
    {
      lines.Add($"{name} clone = new(source.Comparer);");
    }
    else if (exactStack || exactQueue)
    {
      lines.Add($"{name} clone = new(source.Count);");
    }
    else if (exactObservable || exactCollection)
    {
      lines.Add($"{name} clone = new();");
    }
    else if (kind is CollectionKind.ImmutableList or CollectionKind.ImmutableHashSet or CollectionKind.ImmutableQueue or CollectionKind.ImmutableStack)
    {
      AppendImmutableClass(kind, lines, element, copy, name);
      slot.Statements = lines;
      return;
    }
    else if (!TryEmitConstruction(type, lines, out string? problem))
    {
      Fail(slot, problem!, LocationOf(type));
      return;
    }

    lines.Add("map.Add(source, clone);");
    INamedTypeSymbol? stop = CollectionDefinition(type);
    AppendExtraFields(slot, type, lines, stop);

    if (exactStack || kind == CollectionKind.Stack)
    {
      lines.Add($"{element.FullyQualified}[] buffer = new {element.FullyQualified}[source.Count];");
      lines.Add("source.CopyTo(buffer, 0);");
      lines.Add("for (int index = buffer.Length - 1; index >= 0; index--)");
      lines.Add("{");
      lines.Add($"  clone.Push({CopyExpression(element, "buffer[index]")});");
      lines.Add("}");
    }
    else if (exactQueue || kind == CollectionKind.Queue)
    {
      lines.Add("foreach (var item in source)");
      lines.Add("{");
      lines.Add($"  clone.Enqueue({copy});");
      lines.Add("}");
    }
    else if (exactList && element.Kind == SlotKind.Share)
    {
      lines.Add("clone.AddRange(source);");
    }
    else if (exactHash || kind == CollectionKind.HashSet)
    {
      lines.Add("foreach (var item in source)");
      lines.Add("{");
      lines.Add($"  clone.Add({copy});");
      lines.Add("}");
    }
    else
    {
      lines.Add("foreach (var item in source)");
      lines.Add("{");
      lines.Add($"  clone.Add({copy});");
      lines.Add("}");
    }

    lines.Add("return clone;");
    slot.Statements = lines;
  }

  private static void AppendImmutableStruct(CollectionKind kind, List<string> lines, Slot element, string copy)
  {
    if (kind != CollectionKind.ImmutableArray)
    {
      lines.Add($"return global::System.Collections.Immutable.ImmutableArray.CreateRange(source, item => {copy});");
      return;
    }

    lines.Add("if (source.IsDefault)");
    lines.Add("{");
    lines.Add("  return default;");
    lines.Add("}");
    lines.Add($"return global::System.Collections.Immutable.ImmutableArray.CreateRange(source, item => {copy});");
  }

  private static void AppendImmutableClass(
    CollectionKind kind,
    List<string> lines,
    Slot element,
    string copy,
    string name)
  {
    string create = kind switch
    {
      CollectionKind.ImmutableHashSet =>
        $"global::System.Collections.Immutable.ImmutableHashSet.CreateRange(source.KeyComparer, source.Select(item => {copy}))",
      CollectionKind.ImmutableQueue =>
        $"global::System.Collections.Immutable.ImmutableQueue.CreateRange(source.Select(item => {copy}))",
      CollectionKind.ImmutableStack =>
        $"global::System.Collections.Immutable.ImmutableStack.CreateRange(source.Select(item => {copy}))",
      _ =>
        $"global::System.Collections.Immutable.ImmutableList.CreateRange(source.Select(item => {copy}))"
    };
    lines.Add($"{name} clone = {create};");
    lines.Add("map.Add(source, clone);");
    lines.Add("return clone;");
  }

  private void ClassifyCollectionInterface(Slot slot, INamedTypeSymbol type, CollectionKind kind, Slot element)
  {
    slot.Kind = SlotKind.Clone;
    slot.IsDispatch = true;
    if (kind == CollectionKind.DictionaryInterface)
    {
      AppendDictionaryInterfaceCases(type, slot);
    }
    else
    {
      AppendEnumerableInterfaceCases(element, slot);
    }
  }

  private void AppendEnumerableInterfaceCases(Slot element, Slot parent)
  {
    AddCase(parent, CreateArray(element.Type), "array");
    AddConstructedCase(parent, "System.Collections.Generic.List`1", [element.Type], "list");
    AddConstructedCase(parent, "System.Collections.Generic.HashSet`1", [element.Type], "set");
    AddConstructedCase(parent, "System.Collections.ObjectModel.ObservableCollection`1", [element.Type], "observable");
    AddConstructedCase(parent, "System.Collections.ObjectModel.Collection`1", [element.Type], "collection");
    foreach (INamedTypeSymbol implementation in FindImplementations(parent.Type))
    {
      AddCase(parent, implementation, "typed");
    }
  }

  private void AppendDictionaryInterfaceCases(INamedTypeSymbol iface, Slot parent)
  {
    if (!TryDictionaryArguments(iface, out ITypeSymbol? key, out ITypeSymbol? value))
    {
      return;
    }

    AddConstructedCase(parent, "System.Collections.Generic.Dictionary`2", [key!, value!], "dictionary");
    foreach (INamedTypeSymbol implementation in FindImplementations(iface))
    {
      AddCase(parent, implementation, "typed");
    }
  }

  private void AddConstructedCase(Slot parent, string metadataName, ITypeSymbol[] arguments, string variable)
  {
    INamedTypeSymbol? definition = Compilation.GetTypeByMetadataName(metadataName);
    if (definition is null)
    {
      return;
    }

    AddCase(parent, definition.Construct(arguments), variable);
  }

  private void AddCase(Slot parent, ITypeSymbol type, string variable)
  {
    if (!CanName(type))
    {
      return;
    }

    Slot slot = Build(type);
    if (slot.Kind == SlotKind.Error)
    {
      return;
    }

    parent.Dependencies.Add(slot);
    parent.Cases.Add(new CaseRequest(type, variable));
  }

  private void ClassifyInterface(Slot slot, INamedTypeSymbol type)
  {
    List<INamedTypeSymbol> implementations = FindImplementations(type);
    if (implementations.Count == 0 && !AssignableTo(type, "System.ICloneable"))
    {
      Fail(slot, $"interface or abstract type '{type.ToDisplayString()}' has no cloneable implementation in this compilation", LocationOf(type));
      return;
    }

    slot.Kind = SlotKind.Clone;
    slot.IsDispatch = true;
    foreach (INamedTypeSymbol implementation in implementations)
    {
      AddCase(slot, implementation, "typed");
    }
  }

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
        "}",
        "switch (source)",
        "{"
      ];

      HashSet<string> seen = [];
      List<CaseRequest> ordered = [];
      int typedIndex = 0;
      foreach (CaseRequest caseRequest in slot.Cases)
      {
        ITypeSymbol type = Normalize(caseRequest.Type);
        if (!Slots.TryGetValue(type, out Slot? target))
        {
          continue;
        }

        if (target.Kind is not (SlotKind.Clone or SlotKind.Share))
        {
          continue;
        }

        string display = type.ToDisplayString(Format);
        if (!seen.Add(display))
        {
          continue;
        }

        string variable = caseRequest.Variable == "typed" ? $"typed{typedIndex++}" : caseRequest.Variable;
        int depth = type is INamedTypeSymbol named ? Depth(named) : 1;
        ordered.Add(new CaseRequest(type, variable, depth));
      }

      foreach (CaseRequest caseRequest in ordered
        .OrderByDescending(item => item.Depth)
        .ThenBy(item => item.Variable, StringComparer.Ordinal))
      {
        Slot target = Slots[Normalize(caseRequest.Type)];
        string display = caseRequest.Type.ToDisplayString(Format);
        lines.Add($"  case {display} {caseRequest.Variable}:");
        if (target.Kind == SlotKind.Share)
        {
          lines.Add($"    {name} shared = {caseRequest.Variable};");
          lines.Add("    map.Add(source, shared);");
          lines.Add("    return shared;");
        }
        else
        {
          lines.Add($"    return {target.MethodName}({caseRequest.Variable}, map);");
        }
      }

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
      lines.Add("  default:");
      lines.Add("    throw new global::System.InvalidOperationException(\"Cannot clone \" + source.GetType().FullName + \". Implement ICloneable on that type.\");");
      lines.Add("}");
      slot.Statements = lines;
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

    bool deepField = AppendExtraFields(slot, type, lines, stopBefore: null);
    if (slot.Kind == SlotKind.Error)
    {
      return;
    }

    if (type.IsValueType && !deepField && slot.Accessors.Count == 0)
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
    bool deep = false;
    int index = slot.Accessors.Count;
    HashSet<ISymbol> copiedMembers = new(SymbolEqualityComparer.Default);
    foreach (IFieldSymbol field in EnumerateFields(type, stopBefore))
    {
      if (field.AssociatedSymbol is not null)
      {
        copiedMembers.Add(field.AssociatedSymbol);
      }

      if (IsIgnored(field))
      {
        continue;
      }

      if (!TryAppendMember(slot, type, lines, field.Type, field.Name, field.ContainingType, FieldLocation(field), ref index, ref deep))
      {
        return deep;
      }
    }

    // Referenced assemblies do not expose <Prop>k__BackingField through GetMembers. The getter is still
    // CompilerGenerated, and the field name is fixed, so copy those properties the same way.
    foreach ((IPropertySymbol property, INamedTypeSymbol declaringType) in EnumerateAutoProperties(type, stopBefore))
    {
      if (!copiedMembers.Add(property) || HasIgnoreAttribute(property))
      {
        continue;
      }

      string fieldName = $"<{property.Name}>k__BackingField";
      Location? location = property.Locations.FirstOrDefault(candidate => candidate.IsInSource) ?? LocationOf(declaringType);
      if (!TryAppendMember(slot, type, lines, property.Type, fieldName, declaringType, location, ref index, ref deep))
      {
        return deep;
      }
    }

    return deep;
  }

  private bool TryAppendMember(
    Slot slot,
    INamedTypeSymbol type,
    List<string> lines,
    ITypeSymbol memberType,
    string fieldName,
    INamedTypeSymbol declaringType,
    Location? location,
    ref int index,
    ref bool deep)
  {
    Slot fieldSlot = Build(memberType);
    slot.Dependencies.Add(fieldSlot);
    if (fieldSlot.Kind == SlotKind.Error)
    {
      Fail(slot, $"member '{fieldName}' of type '{memberType.ToDisplayString()}' cannot be cloned ({fieldSlot.Detail})", location);
      return false;
    }

    if (type.IsValueType && fieldSlot.Kind == SlotKind.Share)
    {
      return true;
    }

    if (fieldSlot.Kind != SlotKind.Share)
    {
      deep = true;
    }

    string accessorName = $"{slot.MethodName}_F{index}";
    string read = type.IsValueType ? $"{accessorName}(ref source)" : $"{accessorName}(source)";
    string write = type.IsValueType ? $"{accessorName}(ref clone)" : $"{accessorName}(clone)";
    lines.Add($"{write} = {CopyExpression(fieldSlot, read)};");
    slot.Accessors.Add(EmitAccessor(fieldName, memberType, declaringType, accessorName));
    index++;
    return true;
  }

  private bool TryEmitConstruction(INamedTypeSymbol type, List<string> lines, out string? problem)
  {
    IMethodSymbol? constructor = PickConstructor(type);
    if (constructor is null)
    {
      problem = "it has no constructor accessible to generated code";
      return false;
    }

    string arguments = string.Join(", ", constructor.Parameters.Select(parameter => parameter.Type.IsReferenceType ? "default!" : "default"));
    List<string>? required = RequiredInitializer(type, constructor);
    if (required is null)
    {
      problem = "a required member cannot be set by the constructor or an object initializer";
      return false;
    }

    string name = type.ToDisplayString(Format);
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

  private List<string>? RequiredInitializer(INamedTypeSymbol type, IMethodSymbol constructor)
  {
    if (constructor.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "SetsRequiredMembersAttribute"))
    {
      return [];
    }

    List<string> names = [];
    foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>())
    {
      if (!property.IsRequired)
      {
        continue;
      }

      if (property.SetMethod is null)
      {
        return null;
      }

      names.Add(property.Name);
    }

    return names;
  }

  private IMethodSymbol? PickConstructor(INamedTypeSymbol type)
  {
    List<IMethodSymbol> constructors = type.InstanceConstructors
      .Where(constructor => constructor.Parameters.All(parameter => parameter.RefKind == RefKind.None) && IsAccessible(constructor))
      .ToList();
    IMethodSymbol? parameterless = constructors.FirstOrDefault(constructor => constructor.Parameters.Length == 0);
    if (parameterless is not null)
    {
      return parameterless;
    }

    return constructors.OrderBy(constructor => constructor.Parameters.Length).FirstOrDefault();
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

  private bool CanName(ISymbol symbol)
  {
    for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
    {
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
    for (INamedTypeSymbol? current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
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

  private List<INamedTypeSymbol> FindImplementations(ITypeSymbol target)
  {
    List<INamedTypeSymbol> implementations = [];
    FindImplementations(Compilation.Assembly.GlobalNamespace, target, implementations);
    return implementations
      .OrderByDescending(Depth)
      .ToList();
  }

  private void FindImplementations(INamespaceSymbol namespaceSymbol, ITypeSymbol target, List<INamedTypeSymbol> implementations)
  {
    foreach (INamespaceSymbol child in namespaceSymbol.GetNamespaceMembers())
    {
      FindImplementations(child, target, implementations);
    }

    foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
    {
      FindImplementations(type, target, implementations);
    }
  }

  private void FindImplementations(INamedTypeSymbol type, ITypeSymbol target, List<INamedTypeSymbol> implementations)
  {
    foreach (INamedTypeSymbol nested in type.GetTypeMembers())
    {
      FindImplementations(nested, target, implementations);
    }

    if (type.IsAbstract || type.TypeKind != TypeKind.Class || type.IsGenericType && type.IsDefinition)
    {
      return;
    }

    if (SymbolEqualityComparer.Default.Equals(type, target))
    {
      return;
    }

    if (!CanName(type))
    {
      return;
    }

    bool implements = type.AllInterfaces.Any(implemented => SymbolEqualityComparer.Default.Equals(implemented, target))
      || Inherits(type, target);
    if (implements)
    {
      implementations.Add(type);
    }
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

  private IArrayTypeSymbol CreateArray(ITypeSymbol element) => Compilation.CreateArrayTypeSymbol(element);

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

      INamedTypeSymbol? implemented = type.AllInterfaces.FirstOrDefault(candidate =>
        SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, definition));
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

      if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition))
      {
        key = type.TypeArguments[0];
        value = type.TypeArguments[1];
        return true;
      }

      INamedTypeSymbol? implemented = type.AllInterfaces.FirstOrDefault(candidate =>
        SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, definition));
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

  private INamedTypeSymbol? StopBeforeDictionary(INamedTypeSymbol type) => CollectionDefinition(type);

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
      return CollectionKind.List;
    }

    return CollectionKind.None;
  }

  private CollectionKind MatchConcrete(INamedTypeSymbol type)
  {
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.List`1")) return CollectionKind.List;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.Dictionary`2")) return CollectionKind.Dictionary;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.HashSet`1")) return CollectionKind.HashSet;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.Stack`1")) return CollectionKind.Stack;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Generic.Queue`1")) return CollectionKind.Queue;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.ObjectModel.ObservableCollection`1")) return CollectionKind.ObservableCollection;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.ObjectModel.Collection`1")) return CollectionKind.Collection;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Immutable.ImmutableArray`1")) return CollectionKind.ImmutableArray;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Immutable.ImmutableList`1")) return CollectionKind.ImmutableList;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Immutable.ImmutableDictionary`2")) return CollectionKind.ImmutableDictionary;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Immutable.ImmutableHashSet`1")) return CollectionKind.ImmutableHashSet;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Immutable.ImmutableQueue`1")) return CollectionKind.ImmutableQueue;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Immutable.ImmutableStack`1")) return CollectionKind.ImmutableStack;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Frozen.FrozenDictionary`2")) return CollectionKind.FrozenDictionary;
    if (SymbolEquals(type.OriginalDefinition, "System.Collections.Frozen.FrozenSet`1")) return CollectionKind.FrozenSet;
    return CollectionKind.None;
  }

  private bool SymbolEquals(INamedTypeSymbol type, string metadataName)
  {
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
        || Named(type, "System", "DBNull"))
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

    string? namespaceName = type.ContainingNamespace?.ToDisplayString();
    if (!type.IsValueType && namespaceName is "System.Threading" or "System.Threading.Tasks")
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

  private static string EmitAccessor(IFieldSymbol field, string accessorName) =>
    EmitAccessor(field.Name, field.Type, field.ContainingType, accessorName);

  private static string EmitAccessor(string fieldName, ITypeSymbol fieldType, INamedTypeSymbol target, string accessorName)
  {
    string reference = target.IsReferenceType ? string.Empty : "ref ";
    string fieldTypeName = fieldType.ToDisplayString(Format);
    string targetName = target.ToDisplayString(Format);
    string literal = fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"");
    return
      $"[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = \"{literal}\")] private static extern ref {fieldTypeName} {accessorName}({reference}{targetName} target);";
  }

  private static void Fail(Slot slot, string detail, Location? location)
  {
    slot.Kind = SlotKind.Error;
    slot.Detail ??= detail;
    slot.DiagnosticLocation ??= location ?? Location.None;
    slot.PrimaryFailure = true;
  }

  private static Location? LocationOf(ITypeSymbol type) =>
    type.Locations.FirstOrDefault(location => location.IsInSource) ?? type.Locations.FirstOrDefault();

  private static Location? FieldLocation(IFieldSymbol field) =>
    field.Locations.FirstOrDefault(location => location.IsInSource)
    ?? field.AssociatedSymbol?.Locations.FirstOrDefault(location => location.IsInSource)
    ?? LocationOf(field.ContainingType);

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
          slot.Detail ??= $"it depends on '{dependency.FullyQualified}', which cannot be cloned";
          slot.DiagnosticLocation ??= dependency.DiagnosticLocation ?? Location.None;
          changed = true;
          break;
        }
      }
    }
  }

  private void Report(SourceProductionContext sourceContext)
  {
    HashSet<Slot> reachable = [];
    foreach (Slot root in Roots)
    {
      Mark(root, reachable);
    }

    HashSet<string> reported = [];
    foreach (Slot slot in reachable)
    {
      if (slot.Kind != SlotKind.Error || slot.Detail is null)
      {
        continue;
      }

      string key = slot.FullyQualified + "|" + slot.Detail;
      if (!reported.Add(key))
      {
        continue;
      }

      Location location = slot.DiagnosticLocation ?? Location.None;
      sourceContext.ReportDiagnostic(
        Diagnostic.Create(
          StateCloneSourceGenerator.UnsupportedRule,
          location,
          slot.Type.Name,
          slot.Detail));
    }
  }

  private static void Mark(Slot slot, HashSet<Slot> reachable)
  {
    if (!reachable.Add(slot))
    {
      return;
    }

    foreach (Slot dependency in slot.Dependencies)
    {
      Mark(dependency, reachable);
    }
  }

  private string? Emit()
  {
    List<Slot> methods = Slots.Values
      .Where(slot => slot.Kind == SlotKind.Clone && slot.Statements is not null)
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
      foreach (string accessor in slot.Accessors)
      {
        builder.Append("    ");
        builder.AppendLine(accessor);
        builder.AppendLine();
      }
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

  private static string Sanitize(string name)
  {
    StringBuilder builder = new(name.Length);
    foreach (char character in name)
    {
      builder.Append(char.IsLetterOrDigit(character) ? character : '_');
    }

    return builder.ToString();
  }

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
    Stack,
    Queue,
    Collection,
    ObservableCollection,
    ImmutableArray,
    ImmutableList,
    ImmutableDictionary,
    ImmutableHashSet,
    ImmutableQueue,
    ImmutableStack,
    FrozenDictionary,
    FrozenSet,
    DictionaryInterface
  }

  private sealed class CaseRequest
  {
    public CaseRequest(ITypeSymbol type, string variable, int depth = 0)
    {
      Type = type;
      Variable = variable;
      Depth = depth;
    }

    public ITypeSymbol Type { get; }
    public string Variable { get; }
    public int Depth { get; }
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
    public bool PrimaryFailure { get; set; }
    public string? Detail { get; set; }
    public Location? DiagnosticLocation { get; set; }
    public List<string>? Statements { get; set; }
    public List<string> Accessors { get; } = [];
    public List<Slot> Dependencies { get; } = [];
    public List<CaseRequest> Cases { get; } = [];
  }
}
