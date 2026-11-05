#region Purpose
// Turns one `[PersistentState]` class declaration into an equatable PersistenceFact: the Load model, or TWSG001 or
// TWSG003, plus the persistence method the manifest reports.
#endregion

#region Design
// The rules are the pre-142 persistence generator's, moved here unchanged. The golden tests pin them.
// Discovery stays a syntax match on the attribute name (no semantic attribute check), so nothing that matched
// before stops matching. It accepts `PersistentState`, `PersistentStateAttribute`, and either one qualified.
// Nested [PersistentState] is unsupported. Policies nest actions in states, not states in types, so the extractor
// reports TWSG001 and emits nothing, instead of building a containing-type partial chain.
// The rest is semantic. Namespace and type parameters come from the symbol, so a global-namespace or generic state
// gets a partial that merges. Success facts carry no location. Failures carry an equatable LocationInfo.
// Method is the PersistentStateMethod member name from the bound attribute. It is null when the attribute does not
// bind, for example in a compilation without TimeWarp.State, where only the syntax match runs.
//
// Analyzer contract:
// | Assumption                                                     | Guard                                           |
// |----------------------------------------------------------------|-------------------------------------------------|
// | the [PersistentState] type is top level                        | generator: TWSG001                              |
// | the type is partial                                            | generator: TWSG003 (was CS0260)                 |
// | the type derives from State<T> (Sender, CancellationToken)     | generator: TWSG003 when State<T> is referenced  |
// | the attribute is TimeWarp's [PersistentState]                  | none: name match by design (non-breaking)       |
// | AttributeUsage(Class) keeps it off structs and records structs | the compiler (CS0592)                           |
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class PersistenceFactExtractor
{
  public static bool IsCandidateSyntax(SyntaxNode node)
  {
    if (node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclaration)
      return false;

    return classDeclaration.AttributeLists
      .SelectMany(attributeList => attributeList.Attributes)
      .Any(attribute => IsPersistentStateName(attribute.Name.ToString()));
  }

  internal static bool IsPersistentStateName(string name) =>
    name is "PersistentState" or "PersistentStateAttribute"
    || name.EndsWith(".PersistentState", StringComparison.Ordinal)
    || name.EndsWith(".PersistentStateAttribute", StringComparison.Ordinal)
    || name.EndsWith("::PersistentState", StringComparison.Ordinal)
    || name.EndsWith("::PersistentStateAttribute", StringComparison.Ordinal);

  public static PersistenceFact? Extract(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
    if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol state)
      return null;

    string stateKey = StateSymbolFacts.FullMetadataName(state);
    string? method = GetMethod(state);
    string className = classDeclaration.Identifier.Text;
    LocationInfo? location = LocationInfo.From(classDeclaration.Identifier.GetLocation());

    if (state.ContainingType is not null)
    {
      return new PersistenceFact
      (
        Model: null,
        new GeneratorDiagnosticInfo(StateGeneratorDiagnostics.NestedPersistentStateId, location, new EquatableArray<string>([className])),
        stateKey,
        method
      );
    }

    string? failure = null;
    if (!classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
      failure = $"'{state.ToDisplayString()}' is not partial; add the partial modifier so the generated Load can join it";

    INamedTypeSymbol? stateBase = StateSymbolFacts.GetTimeWarpStateType(context.SemanticModel.Compilation);
    if (failure is null && stateBase is not null && !StateSymbolFacts.DerivesFromTimeWarpState(state, stateBase))
      failure = $"'{state.ToDisplayString()}' does not derive from State<T>, which provides Sender and CancellationToken";

    if (failure is not null)
    {
      return new PersistenceFact
      (
        Model: null,
        new GeneratorDiagnosticInfo
        (
          StateGeneratorDiagnostics.CannotGenerateMemberId,
          location,
          new EquatableArray<string>([$"{state.Name}.Load", state.ToDisplayString(), failure])
        ),
        stateKey,
        method
      );
    }

    string? namespaceName = state.ContainingNamespace is { IsGlobalNamespace: false } containing
      ? containing.ToDisplayString()
      : null;
    string typeParameters = state.TypeParameters.Length == 0
      ? string.Empty
      : $"<{string.Join(", ", state.TypeParameters.Select(parameter => parameter.Name))}>";
    string hintName = $"{namespaceName ?? "Global"}.{state.MetadataName}_Persistence.g.cs";

    return new PersistenceFact(new PersistenceModel(namespaceName, className + typeParameters, hintName), Diagnostic: null, stateKey, method);
  }

  // The PersistentStateMethod member name (SessionStorage, LocalStorage) from the bound attribute's first argument.
  private static string? GetMethod(INamedTypeSymbol state)
  {
    foreach (AttributeData attribute in state.GetAttributes())
    {
      if (attribute.AttributeClass is not { Name: "PersistentStateAttribute" }) continue;
      if (attribute.ConstructorArguments.Length == 0) continue;

      TypedConstant argument = attribute.ConstructorArguments[0];
      if (argument.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType || argument.Value is null) return null;

      return enumType.GetMembers()
        .OfType<IFieldSymbol>()
        .FirstOrDefault(field => field.HasConstantValue && Equals(field.ConstantValue, argument.Value))
        ?.Name;
    }

    return null;
  }
}
