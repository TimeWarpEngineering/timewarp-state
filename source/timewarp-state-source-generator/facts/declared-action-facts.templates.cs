#region Purpose
// Template reading, placeholder expansion and dotted-name resolution for DeclaredActionFacts (Mediator 043's rules).
#endregion

#region Design
// Mirrors TimeWarp.Mediator's request-pattern.templates.cs and request-pattern.type-names.cs (15.0.0-beta.10). A
// dotted name resolves by trying each split between namespace and nested types, longest namespace first.
// RuntimeName turns a dotted name into the runtime Type.FullName shape the manifest uses ('+' between nested types):
// the longest prefix that resolves to a type is that type's FullMetadataName, the rest is appended with '+'. A name
// with no resolvable prefix stays dotted.
// ResponseName is for the manifest only. It reads the response from the request interface text: the argument of
// TimeWarp.Mediator.IRequest<T>, or the IRequest<T> a non-generic interface extends. A void request (IRequest,
// IAction) and an interface it cannot resolve give null. Mediator, not State, decides whether the interface is valid.
#endregion

namespace TimeWarp.State.Facts;

internal static partial class DeclaredActionFacts
{
  private const string RequestOfTMetadataName = "TimeWarp.Mediator.IRequest`1";

  // Namespace, containing types, and name, dotted. Null when any part is generic.
  internal static string? DottedName(INamedTypeSymbol type)
  {
    List<string> parts = [];
    for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
    {
      if (current.Arity > 0 || current.IsUnboundGenericType) return null;
      parts.Insert(0, current.Name);
    }

    string typePart = string.Join(".", parts);
    return type.ContainingNamespace is { IsGlobalNamespace: false } containing
      ? containing.ToDisplayString() + "." + typePart
      : typePart;
  }

  /// <summary>Expands {Host}, {Contract} and {ContractType}; false for anything else in braces or a generic host.</summary>
  internal static bool TryExpand(string template, string? host, string contract, string contractType, out string expanded)
  {
    System.Text.StringBuilder builder = new();
    expanded = string.Empty;
    int index = 0;
    while (index < template.Length)
    {
      char character = template[index];
      if (character == '}') return false;

      if (character != '{')
      {
        builder.Append(character);
        index++;
        continue;
      }

      int close = template.IndexOf('}', index + 1);
      if (close < 0) return false;

      switch (template.Substring(index + 1, close - index - 1))
      {
        case "Host":
          if (host is null) return false;
          builder.Append(host);
          break;
        case "Contract":
          builder.Append(contract);
          break;
        case "ContractType":
          builder.Append(contractType);
          break;
        default:
          return false;
      }

      index = close + 1;
    }

    expanded = builder.ToString();
    return true;
  }

  private static bool TryReadTemplate(AttributeData attribute, string parameterName, string fieldName, out string value)
  {
    value = string.Empty;
    if (attribute.AttributeConstructor is { } constructor)
    {
      ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
      ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;
      for (int index = 0; index < Math.Min(parameters.Length, arguments.Length); index++)
      {
        if (!string.Equals(parameters[index].Name, parameterName, StringComparison.Ordinal)) continue;

        TypedConstant argument = arguments[index];
        if (argument.Kind != TypedConstantKind.Primitive || argument.Value is not string text || text.Trim().Length == 0) return false;

        value = text;
        return true;
      }
    }

    for (INamedTypeSymbol? current = attribute.AttributeClass; current is not null; current = current.BaseType)
    {
      foreach (ISymbol member in current.GetMembers(fieldName))
      {
        if (member is IFieldSymbol { IsConst: true, ConstantValue: string text } && text.Trim().Length > 0)
        {
          value = text;
          return true;
        }
      }
    }

    return false;
  }

  private static bool TryReadContracts(AttributeData attribute, out ImmutableArray<TypedConstant> contracts)
  {
    contracts = ImmutableArray<TypedConstant>.Empty;
    if (attribute.AttributeConstructor is not { } constructor) return false;

    ImmutableArray<IParameterSymbol> parameters = constructor.Parameters;
    ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;
    for (int index = 0; index < Math.Min(parameters.Length, arguments.Length); index++)
    {
      if (!string.Equals(parameters[index].Name, "contracts", StringComparison.Ordinal)) continue;

      TypedConstant argument = arguments[index];
      if (argument.Kind != TypedConstantKind.Array || argument.IsNull) return false;

      contracts = argument.Values;
      return true;
    }

    return false;
  }

  internal static bool IsDottedTypeName(string name)
  {
    string stripped = StripGlobal(name);
    return stripped.Length > 0 && stripped.Split('.').All(SyntaxFacts.IsValidIdentifier);
  }

  internal static string StripGlobal(string name)
  {
    string trimmed = name.Trim();
    return trimmed.StartsWith("global::", StringComparison.Ordinal) ? trimmed.Substring("global::".Length) : trimmed;
  }

  /// <summary>Resolves a dotted type name (optionally <c>global::</c>-prefixed) to a type in the compilation.</summary>
  public static INamedTypeSymbol? ResolveDotted(Compilation compilation, string dottedName)
  {
    string name = StripGlobal(dottedName);
    if (name.Length == 0) return null;

    string[] segments = name.Split('.');
    for (int split = segments.Length - 1; split >= 0; split--)
    {
      string namespacePart = string.Join(".", segments, 0, split);
      string typePart = string.Join("+", segments, split, segments.Length - split);
      string metadataName = namespacePart.Length == 0 ? typePart : namespacePart + "." + typePart;
      if (compilation.GetTypeByMetadataName(metadataName) is { } found) return found;
    }

    return null;
  }

  /// <summary>The runtime <c>Type.FullName</c> shape of a dotted name, resolving the longest existing prefix.</summary>
  public static string RuntimeName(Compilation compilation, string dottedName)
  {
    string[] segments = StripGlobal(dottedName).Split('.');
    for (int length = segments.Length; length > 0; length--)
    {
      if (ResolveDotted(compilation, string.Join(".", segments, 0, length)) is not { } prefix) continue;

      string rest = string.Join("+", segments, length, segments.Length - length);
      string head = StateSymbolFacts.FullMetadataName(prefix);
      return rest.Length == 0 ? head : head + "+" + rest;
    }

    return string.Join(".", segments);
  }

  /// <summary>The response type the request interface names, for the manifest; null for a void or unresolved interface.</summary>
  public static string? ResponseName(Compilation compilation, string requestInterfaceName)
  {
    TypeSyntax syntax = SyntaxFactory.ParseTypeName(requestInterfaceName);
    if (syntax.ContainsDiagnostics) return null;
    if (syntax is AliasQualifiedNameSyntax { Alias.Identifier.ValueText: "global" } alias) syntax = alias.Name;

    (string? left, SimpleNameSyntax? right) = syntax switch
    {
      QualifiedNameSyntax qualified => (qualified.Left.ToString(), qualified.Right),
      SimpleNameSyntax simple => (null, simple),
      _ => (null, null)
    };
    if (right is null) return null;

    if (right is GenericNameSyntax generic)
    {
      string definitionName = (left is null ? "" : left + ".") + generic.Identifier.ValueText + "`" + generic.Arity;
      INamedTypeSymbol? definition = ResolveDotted(compilation, definitionName);
      if (definition is null || generic.Arity != 1) return null;
      return StateSymbolFacts.FullMetadataName(definition) == RequestOfTMetadataName
        ? StripGlobal(generic.TypeArgumentList.Arguments[0].ToString())
        : null;
    }

    INamedTypeSymbol? requestInterface = ResolveDotted(compilation, (left is null ? "" : left + ".") + right.Identifier.ValueText);
    INamedTypeSymbol? requestOfT = compilation.GetTypeByMetadataName(RequestOfTMetadataName);
    if (requestInterface is null || requestOfT is null) return null;

    INamedTypeSymbol? response = requestInterface.AllInterfaces
      .FirstOrDefault(candidate => SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, requestOfT));
    return response?.TypeArguments[0].ToDisplayString();
  }
}
