#region Purpose
// Finds State<T> and [GenerateClone] roots, including closed constructions of open generic definitions.
#endregion

#region Design
// Both closed-construction scans walk GenericNameSyntax with a name set and a predicate. Open generic states that do not implement ICloneable fail at the definition. ICloneable states are remembered and not cloned field by field.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

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
  private void CollectClosedGenerateCloneTypes() =>
    CollectClosedGenericNames(GenericCloneNames, static (planner, symbol) =>
    {
      if (HasGenerateClone(symbol.OriginalDefinition))
      {
        planner.Consider(symbol);
      }
    });

  private static bool ContainsTypeParameter(ITypeSymbol type) =>
    type switch
    {
      ITypeParameterSymbol => true,
      IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
      INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter),
      _ => false
    };

  // Closed constructions of an ICloneable open generic state are nameable services. The open definition is not.
  private void CollectClosedICloneableStates()
  {
    if (OpenGenericICloneableStates.Count == 0)
    {
      return;
    }

    HashSet<string> names = new(StringComparer.Ordinal);
    foreach (INamedTypeSymbol open in OpenGenericICloneableStates)
    {
      names.Add(open.Name);
    }

    CollectClosedGenericNames(names, static (planner, symbol) =>
    {
      if (!planner.IsOpenGenericICloneableState(symbol.OriginalDefinition) || !planner.ImplementsICloneable(symbol))
      {
        return;
      }

      planner.RememberICloneableState(symbol);
    });
  }

  private void CollectClosedGenericNames(HashSet<string> names, Action<StateClonePlanner, INamedTypeSymbol> accept)
  {
    if (names.Count == 0)
    {
      return;
    }

    foreach (SyntaxTree tree in Compilation.SyntaxTrees)
    {
      PlanningCancellation.ThrowIfCancellationRequested();
      SemanticModel? model = null;
      foreach (GenericNameSyntax genericName in tree.GetRoot(PlanningCancellation).DescendantNodes().OfType<GenericNameSyntax>())
      {
        if (!names.Contains(genericName.Identifier.ValueText))
        {
          continue;
        }

        PlanningCancellation.ThrowIfCancellationRequested();
        model ??= Compilation.GetSemanticModel(tree);
        if (model.GetSymbolInfo(genericName, PlanningCancellation).Symbol is not INamedTypeSymbol symbol)
        {
          continue;
        }

        if (symbol.IsUnboundGenericType || symbol.TypeArguments.Any(ContainsTypeParameter))
        {
          continue;
        }

        accept(this, symbol);
      }
    }
  }

  private bool IsOpenGenericICloneableState(INamedTypeSymbol definition)
  {
    foreach (INamedTypeSymbol open in OpenGenericICloneableStates)
    {
      if (SymbolEqualityComparer.Default.Equals(open, definition))
      {
        return true;
      }
    }

    return false;
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
      if (HasGenerateClone(type))
      {
        GenericCloneNames.Add(type.Name);
      }

      if (isState && ImplementsICloneable(type))
      {
        OpenGenericICloneableStates.Add(type);
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
      RememberICloneableState(type);
      return;
    }

    if (!isState && !HasGenerateClone(type) && !HasGenerateClone(type.OriginalDefinition))
    {
      return;
    }

    Slot slot = Build(type);
    // A value with nothing to copy is Share. A [GenerateClone] or state root still needs a Clone method.
    if (slot.IsValueType && slot.Kind == SlotKind.Share)
    {
      slot.Kind = SlotKind.Clone;
      slot.Statements = ["return source;"];
    }

    if (slot.IsRoot)
    {
      slot.RegisterState |= isState;
      return;
    }

    slot.IsRoot = true;
    slot.RegisterState = isState;
    Roots.Add(slot);
  }

}
