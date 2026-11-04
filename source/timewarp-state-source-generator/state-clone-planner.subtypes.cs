#region Purpose
// Finds concrete subtypes a runtime-type dispatch can name.
#endregion

#region Design
// Closed non-generic targets cache both implementation and derived lists. A generic target is closed per construction, so it is not cached. Candidate assemblies that reference the declaring assembly are cached so each scan walks ReferencedAssemblySymbols once.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  // Concrete known types (classes and structs) that implement an interface or derive from an abstract class.
  private List<INamedTypeSymbol> FindImplementations(INamedTypeSymbol target) => FindSubtypes(target, implementations: true);

  private List<INamedTypeSymbol> FindDerived(INamedTypeSymbol target) => FindSubtypes(target, implementations: false);

  private List<INamedTypeSymbol> FindSubtypes(INamedTypeSymbol target, bool implementations)
  {
    if (target.IsGenericType)
    {
      return OrderSubtypes(target, implementations);
    }

    if (!ClosedSubtypeCache.TryGetValue(target, out (List<INamedTypeSymbol> Implementations, List<INamedTypeSymbol> Derived) pair))
    {
      pair = (
        OrderSubtypes(target, implementations: true),
        OrderSubtypes(target, implementations: false));
      ClosedSubtypeCache.Add(target, pair);
    }

    return implementations ? pair.Implementations : pair.Derived;
  }

  private List<INamedTypeSymbol> OrderSubtypes(INamedTypeSymbol target, bool implementations) =>
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
    foreach (IAssemblySymbol referenced in AssembliesThatReference(declaring))
    {
      candidates = candidates.Concat(TypesOf(referenced));
    }

    return candidates;
  }

  private List<IAssemblySymbol> AssembliesThatReference(IAssemblySymbol declaring)
  {
    if (AssembliesReferencing.TryGetValue(declaring, out List<IAssemblySymbol>? cached))
    {
      return cached;
    }

    List<IAssemblySymbol> matches = [];
    foreach (IAssemblySymbol referenced in Compilation.SourceModule.ReferencedAssemblySymbols)
    {
      if (SymbolEqualityComparer.Default.Equals(referenced, declaring) || IsFrameworkAssembly(referenced))
      {
        continue;
      }

      if (referenced.Modules.Any(module => module.ReferencedAssemblySymbols.Contains(declaring, SymbolEqualityComparer.Default)))
      {
        matches.Add(referenced);
      }
    }

    AssembliesReferencing.Add(declaring, matches);
    return matches;
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
}
