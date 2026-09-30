#region Purpose
// Single parse of an *ActionSet.Action's first explicit constructor, shared by the ActionSet method
// generator (State.Method(args, ct)) and the action catalog generator (descriptor + executor).
#endregion

#region Design
// The first ConstructorDeclarationSyntax in the Action class defines the parameter list; no explicit
// constructor means no parameters. Default values are the constructor's source text so the generated
// State.Method keeps the author's defaults. JsonSchema covers primitives, string, Guid, date/time and
// enums; complex types (for example a Command) return null and are recorded by CLR type name only (v1).
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed record ActionParameterModel
(
  string Type,
  string Name,
  string? DefaultValue,
  string TypeOfName,
  string? JsonSchema,
  string CatalogName
);

internal static class ActionSetConstructorParser
{
  public static List<ActionParameterModel> GetParameters
  (
    ClassDeclarationSyntax actionClass,
    SemanticModel semanticModel
  )
  {
    ConstructorDeclarationSyntax? constructor = actionClass.DescendantNodes()
      .OfType<ConstructorDeclarationSyntax>()
      .FirstOrDefault();

    if (constructor == null)
      return new List<ActionParameterModel>();

    return constructor.ParameterList.Parameters.Select(p =>
    {
      var parameterSymbol = semanticModel.GetDeclaredSymbol(p) as IParameterSymbol;

      string fullTypeName = parameterSymbol?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            ?? "System.Object";

      string typeOfName = parameterSymbol?.Type
                            .WithNullableAnnotation(NullableAnnotation.NotAnnotated)
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                          ?? "global::System.Object";

      string? defaultValue = p.Default?.Value?.ToString();
      string? jsonSchema = parameterSymbol is null ? null : GetJsonSchema(parameterSymbol.Type);
      return new ActionParameterModel(fullTypeName, p.Identifier.Text, defaultValue, typeOfName, jsonSchema, p.Identifier.ValueText);
    }).ToList();
  }

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
