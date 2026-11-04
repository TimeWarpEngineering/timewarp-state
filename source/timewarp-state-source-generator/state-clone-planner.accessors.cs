#region Purpose
// Emits one UnsafeAccessor holder class per declaring type definition.
#endregion

#region Design
// A generic declaring type gets a generic holder whose type parameters mirror the definition, which is the .NET 9 generic UnsafeAccessor rule. Field and constructor accessors are reused by name inside the holder.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

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

}
