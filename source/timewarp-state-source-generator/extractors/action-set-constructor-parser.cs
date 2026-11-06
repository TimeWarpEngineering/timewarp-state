#region Purpose
// Single parse of an *ActionSet.Action's construction shape, shared by the ActionSet method generator
// (State.Method(args, ct)) and the action catalog generator (descriptor + executor).
#endregion

#region Design
// Works on the Action symbol, so a partial Action, a record and a primary constructor all read the same way.
// Constructor choice, in order, for a record or struct Action:
//   1. the primary constructor (struct primary constructor, or a positional record);
//   2. the first explicit instance constructor in source order;
//   3. none, which means no constructor arguments.
// A plain (non-record) class Action swaps 1 and 2: the first explicit instance constructor, then the primary
// constructor. The pre-141 parser always took the first explicit constructor, so a class with both kept that
// signature and must keep it (review round 1, M2). A class with only a primary constructor used to be CS7036, so
// reading it there changes nothing that compiled.
// Static constructors and implicit ones (the default constructor, a record's copy constructor) are never chosen.
// `required` members of the Action and its bases become parameters too, unless the chosen constructor has
// [SetsRequiredMembers]. They are set with an object initializer. Before this change such an Action produced
// CS9035, so adding them breaks nothing. Plain `init` properties that are not `required` stay out: adding them would
// change the signature of a method that compiles today (for example the test app's IncrementCount()).
// Parameter order: constructor parameters without a default, then required members, then constructor parameters
// with a default. Required members have no default, so they must come before optional parameters. Constructor
// arguments are still passed in constructor order, so construction is unaffected by this order.
// DefaultValue is the author's source text and is what the catalog records (unchanged). DefaultExpression is what
// the generated method declares. It is the source text when that is a literal or `default`. Otherwise it is the
// constant the compiler bound, so a default that names a constant only visible inside the Action still compiles in
// the state's scope.
// JsonSchema covers primitives, string, Guid, date/time and enums. Complex types (for example a Command) return null
// and are recorded by CLR type name only (v1).
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed record ActionParameterModel
(
  string Type,
  string Name,
  string? DefaultValue,
  string TypeOfName,
  string? JsonSchema,
  string CatalogName,
  string? DefaultExpression,
  string? InitializerMember
);

internal sealed record ActionConstruction
(
  IMethodSymbol? Constructor,
  List<ActionParameterModel> Parameters,
  List<ITypeSymbol> ParameterTypes
);

internal static class ActionSetConstructorParser
{
  private const string SetsRequiredMembersAttribute = "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute";

  public static ActionConstruction Parse(INamedTypeSymbol action, CancellationToken cancellationToken)
  {
    IMethodSymbol? constructor = SelectConstructor(action, cancellationToken);
    List<(ActionParameterModel Model, ITypeSymbol Type)> constructorParameters = constructor is null
      ? []
      : constructor.Parameters.Select(parameter => (FromParameter(parameter, cancellationToken), parameter.Type)).ToList();

    bool setsRequiredMembers = constructor?.GetAttributes()
      .Any(attribute => attribute.AttributeClass?.ToDisplayString() == SetsRequiredMembersAttribute) == true;
    List<(ActionParameterModel Model, ITypeSymbol Type)> requiredMembers = setsRequiredMembers ? [] : GetRequiredMembers(action);

    int firstOptional = constructorParameters.FindIndex(parameter => parameter.Model.DefaultValue is not null);
    if (firstOptional < 0) firstOptional = constructorParameters.Count;

    List<(ActionParameterModel Model, ITypeSymbol Type)> parameters = [];
    parameters.AddRange(constructorParameters.Take(firstOptional));
    parameters.AddRange(requiredMembers);
    parameters.AddRange(constructorParameters.Skip(firstOptional));
    return new ActionConstruction
    (
      constructor,
      parameters.Select(parameter => parameter.Model).ToList(),
      parameters.Select(parameter => parameter.Type).ToList()
    );
  }

  /// <summary>Parameters only, in method order. Used by the catalog generator.</summary>
  public static List<ActionParameterModel> GetParameters(INamedTypeSymbol action, CancellationToken cancellationToken) =>
    Parse(action, cancellationToken).Parameters;

  private static IMethodSymbol? SelectConstructor(INamedTypeSymbol action, CancellationToken cancellationToken)
  {
    IMethodSymbol? primary = action.InstanceConstructors
      .FirstOrDefault(constructor => IsPrimaryConstructor(constructor, cancellationToken));
    bool explicitFirst = action is { TypeKind: TypeKind.Class, IsRecord: false };
    if (primary is not null && !explicitFirst)
      return primary;

    return FirstExplicitConstructor(action, cancellationToken) ?? primary;
  }

  private static IMethodSymbol? FirstExplicitConstructor(INamedTypeSymbol action, CancellationToken cancellationToken)
  {
    return action.InstanceConstructors
      .Where(constructor => !constructor.IsImplicitlyDeclared)
      .Select(constructor => (Constructor: constructor, Syntax: constructor.DeclaringSyntaxReferences.FirstOrDefault()))
      .Where(candidate => candidate.Syntax?.GetSyntax(cancellationToken) is ConstructorDeclarationSyntax)
      .OrderBy(candidate => candidate.Syntax!.SyntaxTree.FilePath, StringComparer.Ordinal)
      .ThenBy(candidate => candidate.Syntax!.Span.Start)
      .Select(candidate => candidate.Constructor)
      .FirstOrDefault();
  }

  // A primary constructor's declaring syntax is the type declaration that carries the parameter list. A record's
  // copy constructor is implicitly declared and has one parameter of the record's own type, so it is excluded.
  private static bool IsPrimaryConstructor(IMethodSymbol constructor, CancellationToken cancellationToken)
  {
    foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences)
    {
      if (reference.GetSyntax(cancellationToken) is not TypeDeclarationSyntax { ParameterList: { } parameterList })
        continue;

      if (parameterList.Parameters.Count != constructor.Parameters.Length)
        continue;

      bool isCopyConstructor = constructor.IsImplicitlyDeclared
        && constructor.Parameters.Length == 1
        && SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, constructor.ContainingType);
      if (!isCopyConstructor)
        return true;
    }

    return false;
  }

  private static ActionParameterModel FromParameter(IParameterSymbol parameter, CancellationToken cancellationToken)
  {
    ParameterSyntax? syntax = parameter.DeclaringSyntaxReferences
      .Select(reference => reference.GetSyntax(cancellationToken))
      .OfType<ParameterSyntax>()
      .FirstOrDefault();

    string fullTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    string typeOfName = parameter.Type
      .WithNullableAnnotation(NullableAnnotation.NotAnnotated)
      .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    ExpressionSyntax? defaultSyntax = syntax?.Default?.Value;
    string? defaultValue = defaultSyntax?.ToString();
    string? defaultExpression = defaultSyntax is null ? null : GetDefaultExpression(parameter, defaultSyntax);
    string name = syntax?.Identifier.Text ?? Escape(parameter.Name);

    return new ActionParameterModel
    (
      fullTypeName,
      name,
      defaultValue,
      typeOfName,
      GetJsonSchema(parameter.Type),
      parameter.Name,
      defaultExpression,
      InitializerMember: null
    );
  }

  private static List<(ActionParameterModel Model, ITypeSymbol Type)> GetRequiredMembers(INamedTypeSymbol action)
  {
    List<INamedTypeSymbol> chain = [];
    for (INamedTypeSymbol? current = action; current is not null; current = current.BaseType)
      chain.Insert(0, current);

    HashSet<string> seen = new(StringComparer.Ordinal);
    List<(ActionParameterModel Model, ITypeSymbol Type)> parameters = [];
    foreach (INamedTypeSymbol type in chain)
    {
      foreach (ISymbol member in type.GetMembers())
      {
        ITypeSymbol? memberType = member switch
        {
          IPropertySymbol { IsRequired: true } property => property.Type,
          IFieldSymbol { IsRequired: true } field => field.Type,
          _ => null
        };
        if (memberType is null || !seen.Add(member.Name)) continue;

        string catalogName = CamelCase(member.Name);
        parameters.Add
        ((
          new ActionParameterModel
          (
            memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Escape(catalogName),
            DefaultValue: null,
            memberType.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            GetJsonSchema(memberType),
            catalogName,
            DefaultExpression: null,
            InitializerMember: Escape(member.Name)
          ),
          memberType
        ));
      }
    }

    return parameters;
  }

  private static string? GetDefaultExpression(IParameterSymbol parameter, ExpressionSyntax syntax)
  {
    if (IsSelfContained(syntax) || !parameter.HasExplicitDefaultValue)
      return syntax.ToString();

    object? value = parameter.ExplicitDefaultValue;
    ITypeSymbol type = parameter.Type;
    if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
      type = nullable.TypeArguments[0];

    if (value is null)
      return parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T
        ? "default"
        : "null";

    if (type.TypeKind == TypeKind.Enum)
      return $"({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})({SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false)})";

    switch (value)
    {
      case double number when double.IsNaN(number) || double.IsInfinity(number):
      case float single when float.IsNaN(single) || float.IsInfinity(single):
        return syntax.ToString();
    }

    string? literal = SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false);
    if (literal is null) return syntax.ToString();
    return value switch
    {
      float => literal + "F",
      double => literal + "D",
      decimal => literal + "M",
      long => literal + "L",
      ulong => literal + "UL",
      uint => literal + "U",
      _ => literal
    };
  }

  // Source text that binds the same way in any scope: a literal, `default`, `default(T)` of a predefined type, or a
  // signed numeric literal.
  private static bool IsSelfContained(ExpressionSyntax syntax) => syntax switch
  {
    LiteralExpressionSyntax => true,
    DefaultExpressionSyntax { Type: PredefinedTypeSyntax } => true,
    PrefixUnaryExpressionSyntax { Operand: LiteralExpressionSyntax } => true,
    ParenthesizedExpressionSyntax parenthesized => IsSelfContained(parenthesized.Expression),
    _ => false
  };

  private static string CamelCase(string name) =>
    name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);

  internal static string Escape(string identifier) =>
    SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None ? identifier : "@" + identifier;

  private static string? GetJsonSchema(ITypeSymbol type)
  {
    if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
      type = nullable.TypeArguments[0];

    if (type.TypeKind == TypeKind.Enum)
    {
      IEnumerable<string> members = type.GetMembers()
        .OfType<IFieldSymbol>()
        .Where(field => field.HasConstantValue)
        .Select(field => $"\"{field.Name}\"");
      return $"{{\"type\":\"string\",\"enum\":[{string.Join(",", members)}]}}";
    }

    switch (type.SpecialType)
    {
      case SpecialType.System_Boolean:
        return "{\"type\":\"boolean\"}";
      case SpecialType.System_SByte:
      case SpecialType.System_Byte:
      case SpecialType.System_Int16:
      case SpecialType.System_UInt16:
      case SpecialType.System_Int32:
      case SpecialType.System_UInt32:
      case SpecialType.System_Int64:
      case SpecialType.System_UInt64:
        return "{\"type\":\"integer\"}";
      case SpecialType.System_Single:
      case SpecialType.System_Double:
      case SpecialType.System_Decimal:
        return "{\"type\":\"number\"}";
      case SpecialType.System_String:
      case SpecialType.System_Char:
        return "{\"type\":\"string\"}";
      case SpecialType.System_DateTime:
        return "{\"type\":\"string\",\"format\":\"date-time\"}";
    }

    return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) switch
    {
      "global::System.Guid" => "{\"type\":\"string\",\"format\":\"uuid\"}",
      "global::System.DateTimeOffset" => "{\"type\":\"string\",\"format\":\"date-time\"}",
      "global::System.DateOnly" => "{\"type\":\"string\",\"format\":\"date\"}",
      "global::System.TimeSpan" => "{\"type\":\"string\",\"format\":\"duration\"}",
      _ => null
    };
  }
}
