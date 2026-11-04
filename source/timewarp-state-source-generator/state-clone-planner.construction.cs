#region Purpose
// Chooses the constructor a generated clone calls.
#endregion

#region Design
// A collection subclass prefers a comparer constructor, then the ordinary path. The ordinary path prefers a parameterless constructor, then the accessible constructor with the fewest parameters, then any constructor through an UnsafeAccessor. Required members are initialized to default unless SetsRequiredMembers is present.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  // A subclass does not inherit the base collection constructors. When it declares one that accepts the comparer,
  // pass source.Comparer. Pass source.Count only for an int whose name contains "capacity". A constructor whose
  // int means something else (maxEntries, version) is skipped, so a comparer-only constructor wins. Otherwise the
  // ordinary constructor path runs, which is what a subclass with only the implicit parameterless constructor can call.
  private bool TryEmitSubclassConstruction(INamedTypeSymbol type, CollectionKind kind, List<string> lines, out string? problem)
  {
    if (TryEmitComparerConstruction(type, kind, lines, out problem))
    {
      return true;
    }

    if (problem is not null)
    {
      return false;
    }

    return TryEmitConstruction(type, lines, out problem);
  }

  private bool TryEmitComparerConstruction(INamedTypeSymbol type, CollectionKind kind, List<string> lines, out string? problem)
  {
    problem = null;
    if (!HasSourceComparer(kind))
    {
      return false;
    }

    ITypeSymbol? comparerType = ComparerPropertyType(type);
    if (comparerType is null)
    {
      return false;
    }

    IMethodSymbol? chosen = null;
    int chosenScore = int.MinValue;
    foreach (IMethodSymbol constructor in type.InstanceConstructors)
    {
      if (!IsComparerConstructor(constructor, comparerType, kind))
      {
        continue;
      }

      int score = (IsAccessible(constructor) ? 100 : 0)
        + constructor.Parameters.Count(IsCapacityParameter) * 10
        - constructor.Parameters.Length;
      if (score > chosenScore)
      {
        chosen = constructor;
        chosenScore = score;
      }
    }

    if (chosen is null)
    {
      return false;
    }

    List<string> arguments = [];
    foreach (IParameterSymbol parameter in chosen.Parameters)
    {
      arguments.Add(IsCapacityParameter(parameter) ? "source.Count" : "source.Comparer");
    }

    return EmitConstructedClone(type, chosen, arguments, lines, out problem);
  }

  private bool IsComparerConstructor(IMethodSymbol constructor, ITypeSymbol comparerType, CollectionKind kind)
  {
    if (constructor.Parameters.Length == 0
        || constructor.Parameters.Any(parameter => parameter.RefKind != RefKind.None))
    {
      return false;
    }

    int ints = 0;
    bool sawComparer = false;
    foreach (IParameterSymbol parameter in constructor.Parameters)
    {
      if (parameter.Type.SpecialType == SpecialType.System_Int32)
      {
        // ConcurrentDictionary has no count-and-comparer constructor. Any other int is not a capacity.
        if (kind == CollectionKind.ConcurrentDictionary || !IsCapacityParameter(parameter))
        {
          return false;
        }

        ints++;
        if (ints > 1)
        {
          return false;
        }

        continue;
      }

      if (IsComparer(parameter.Type) && Compilation.HasImplicitConversion(comparerType, parameter.Type))
      {
        sawComparer = true;
        continue;
      }

      return false;
    }

    return sawComparer;
  }

  // BCL collection constructors name this parameter capacity. initialCapacity counts. maxEntries and version do not.
  private static bool IsCapacityParameter(IParameterSymbol parameter) =>
    parameter.Type.SpecialType == SpecialType.System_Int32
    && parameter.Name.Contains("capacity", StringComparison.OrdinalIgnoreCase);

  private static bool HasSourceComparer(CollectionKind kind) =>
    kind is CollectionKind.Dictionary
      or CollectionKind.HashSet
      or CollectionKind.SortedSet
      or CollectionKind.SortedDictionary
      or CollectionKind.SortedList
      or CollectionKind.ConcurrentDictionary;

  private static ITypeSymbol? ComparerPropertyType(INamedTypeSymbol type)
  {
    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      foreach (IPropertySymbol property in current.GetMembers("Comparer").OfType<IPropertySymbol>())
      {
        if (property.GetMethod is not null && property.Parameters.Length == 0 && !property.IsStatic)
        {
          return property.Type;
        }
      }
    }

    return null;
  }

  private bool EmitConstructedClone(
    INamedTypeSymbol type,
    IMethodSymbol constructor,
    IReadOnlyList<string> arguments,
    List<string> lines,
    out string? problem)
  {
    string name = type.ToDisplayString(Format);
    string argumentList = string.Join(", ", arguments);
    if (IsAccessible(constructor))
    {
      List<string>? required = RequiredInitializer(type, constructor);
      if (required is null)
      {
        problem = null;
        return false;
      }

      if (required.Count == 0)
      {
        lines.Add($"{name} clone = new({argumentList});");
      }
      else
      {
        lines.Add($"{name} clone = new({argumentList})");
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

    string? call = ConstructorAccessor(type, constructor, out problem);
    if (call is null)
    {
      return false;
    }

    lines.Add($"{name} clone = {call}({argumentList});");
    return true;
  }

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

}
