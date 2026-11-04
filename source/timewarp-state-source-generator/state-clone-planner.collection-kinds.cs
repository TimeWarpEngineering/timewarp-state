#region Purpose
// Matches a type to a BCL collection kind and reads its element types.
#endregion

#region Design
// Interface and concrete tables live on StateCloneTypeCatalog, so matching compares symbols instead of calling GetTypeByMetadataName. Element and dictionary type arguments share one interface lookup.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private bool TryElementType(INamedTypeSymbol type, out ITypeSymbol? element)
  {
    if (Implemented(type, Catalog.Enumerable) is { } implemented)
    {
      element = implemented.TypeArguments[0];
      return true;
    }

    element = null;
    return false;
  }

  private bool TryDictionaryArguments(INamedTypeSymbol type, out ITypeSymbol? key, out ITypeSymbol? value)
  {
    INamedTypeSymbol? implemented = Implemented(type, Catalog.Dictionary) ?? Implemented(type, Catalog.ReadOnlyDictionary);
    if (implemented is not null)
    {
      key = implemented.TypeArguments[0];
      value = implemented.TypeArguments[1];
      return true;
    }

    key = null;
    value = null;
    return false;
  }

  private static INamedTypeSymbol? Implemented(INamedTypeSymbol type, INamedTypeSymbol? definition)
  {
    if (definition is null)
    {
      return null;
    }

    if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition))
    {
      return type;
    }

    foreach (INamedTypeSymbol candidate in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, definition))
      {
        return candidate;
      }
    }

    return null;
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
    foreach ((INamedTypeSymbol? definition, CollectionKind kind) in Catalog.InterfaceKinds)
    {
      if (definition is not null && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition))
      {
        return kind;
      }
    }

    return CollectionKind.None;
  }

  private CollectionKind MatchConcrete(INamedTypeSymbol type)
  {
    foreach ((INamedTypeSymbol? definition, CollectionKind kind, string _) in Catalog.ConcreteKinds)
    {
      if (definition is not null && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, definition))
      {
        return kind;
      }
    }

    return CollectionKind.None;
  }

  private string DefinitionName(CollectionKind kind)
  {
    foreach ((INamedTypeSymbol? _, CollectionKind entryKind, string metadataName) in Catalog.ConcreteKinds)
    {
      if (entryKind == kind)
      {
        return metadataName;
      }
    }

    return string.Empty;
  }

  private bool SymbolEquals(INamedTypeSymbol type, string metadataName) =>
    Catalog.SymbolEquals(type, metadataName);
}
