#region Purpose
// Turns one nested `*ActionSet.Action` declaration into an equatable ActionFact: the dispatch method model or its
// TWSG003 failure, the [CatalogAction] entry, and the manifest entry. ActionSetShape is analyzed once per action.
#endregion

#region Design
// Before task 142 the ActionSet method generator and the catalog generator each discovered actions and each called
// ActionSetShape.Analyze. Now one transform does it once and fills every part of the fact.
// Candidates (IsCandidateSyntax): a non-interface type named Action directly inside a type whose name ends in
// ActionSet, directly inside a type. That is the union of the two old discoveries. The method generator required the
// ActionSet to be a class declaration (ActionSetShape.IsCandidateSyntax), and the catalog generator went by the
// attribute on any type declaration. Only the first declaration of a partial Action produces a fact, so a partial
// Action yields one method, one entry and one manifest row.
// Parity rules, kept exactly:
// - Method: only when the node also matches ActionSetShape.IsCandidateSyntax. A user-declared method suppresses it.
//   A failure becomes TWSG003 only for shapes served before (ActionSetShape.WasServedBefore). Otherwise it is
//   skipped silently (task 141, review M1).
// - Catalog: only with [CatalogAction] (CatalogActionFacts.FindAttribute), the shared placement rule, an analysis
//   without failure, and a user-declared method only when the executor's call binds (UserMethodCallable).
// The manifest row exists for every action set whose parent implements IState. Method is the method name when the
// method exists, generated or hand-written. Parameters are the generated method's parameters. Catalog is the catalog
// name when there is an entry. Tracked means [TrackAction] is on the Action.
// Known limit, shared with the old generators: the transform reruns only when the Action's own tree changes, so
// [CatalogAction] added in another partial declaration's file shows up on that tree's next edit.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class ActionFactExtractor
{
  public const string TrackActionAttributeMetadataName = "TimeWarp.Features.ActionTracking.TrackActionAttribute";

  public static bool IsCandidateSyntax(SyntaxNode node) =>
    node is TypeDeclarationSyntax { Identifier.ValueText: "Action", Parent: TypeDeclarationSyntax actionSet }
    && node is not InterfaceDeclarationSyntax
    && actionSet.Identifier.ValueText.EndsWith("ActionSet", StringComparison.Ordinal)
    && actionSet.Parent is TypeDeclarationSyntax;

  public static ActionFact? Extract(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol action)
      return null;

    // A partial Action has several declarations; only the first one produces the fact.
    if (action.DeclaringSyntaxReferences.FirstOrDefault() is not { } first
        || first.SyntaxTree != context.Node.SyntaxTree
        || first.Span != context.Node.Span)
      return null;

    Compilation compilation = context.SemanticModel.Compilation;
    ActionSetAnalysis? analysis = ActionSetShape.Analyze(action, compilation, cancellationToken);
    if (analysis is null) return null;

    (ActionSetMethodModel? method, GeneratorDiagnosticInfo? diagnostic) = ActionSetShape.IsCandidateSyntax(context.Node)
      ? GetMethod(analysis, (TypeDeclarationSyntax)context.Node)
      : (null, null);
    CatalogEntryModel? catalog = GetCatalogEntry(action, analysis, compilation);

    return new ActionFact
    (
      StateSymbolFacts.FullMetadataName(analysis.State),
      method,
      diagnostic,
      catalog,
      GetManifest(analysis, catalog, compilation)
    );
  }

  private static (ActionSetMethodModel? Method, GeneratorDiagnosticInfo? Diagnostic) GetMethod
  (
    ActionSetAnalysis analysis,
    TypeDeclarationSyntax declaration
  )
  {
    if (analysis.MethodDeclaredByUser) return (null, null);

    if (analysis.Failure is null) return (CreateMethodModel(analysis), null);

    // A shape the pre-141 generator ignored compiled before; an Error would break that build (task 141 review M1).
    if (!analysis.ServedBefore) return (null, null);

    return
    (
      null,
      new GeneratorDiagnosticInfo
      (
        StateGeneratorDiagnostics.CannotGenerateMemberId,
        LocationInfo.From(declaration.Identifier.GetLocation()),
        new EquatableArray<string>([$"{analysis.State.Name}.{analysis.MethodName}", analysis.Action.ToDisplayString(), analysis.Failure])
      )
    );
  }

  private static ActionSetMethodModel CreateMethodModel(ActionSetAnalysis analysis)
  {
    List<string> typeHeaders = [];
    List<string> hintPath = [];
    for (INamedTypeSymbol? current = analysis.State; current is not null; current = current.ContainingType)
    {
      typeHeaders.Insert(0, $"partial {Keyword(current)} {Declaration(current)}");
      hintPath.Insert(0, current.MetadataName);
    }

    string? namespaceName = analysis.State.ContainingNamespace is { IsGlobalNamespace: false } containing
      ? containing.ToDisplayString()
      : null;

    string hintName = $"{namespaceName ?? "Global"}.{string.Join("+", hintPath)}.{analysis.ActionSet.Name}_Method.g.cs";

    return new ActionSetMethodModel
    (
      namespaceName,
      new EquatableArray<string>(typeHeaders),
      analysis.ActionSet.Name,
      analysis.MethodName,
      analysis.MethodAccessibility,
      new EquatableArray<ActionParameterModel>(analysis.Construction.Parameters),
      hintName
    );
  }

  private static CatalogEntryModel? GetCatalogEntry(INamedTypeSymbol action, ActionSetAnalysis analysis, Compilation compilation)
  {
    if (compilation.GetTypeByMetadataName(CatalogActionFacts.AttributeMetadataName) is not { } catalogAction) return null;
    if (CatalogActionFacts.FindAttribute(action, catalogAction) is not { } attribute) return null;
    if (!CatalogActionFacts.TryGetPlacement(action, out INamedTypeSymbol actionSet, out INamedTypeSymbol state)) return null;
    if (analysis.Failure is not null) return null;
    if (analysis.MethodDeclaredByUser && !analysis.UserMethodCallable) return null;

    CatalogActionAttributeFacts facts = CatalogActionFacts.ReadAttribute(attribute);
    return new CatalogEntryModel
    (
      Name: CatalogActionFacts.GetName(facts, state, actionSet),
      Description: facts.Description,
      DisplayName: facts.DisplayName,
      Permissions: new EquatableArray<string>(facts.Permissions),
      Visibility: facts.Visibility,
      StateType: state.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      ActionType: action.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      MethodName: analysis.MethodName,
      Parameters: new EquatableArray<ActionParameterModel>(analysis.Construction.Parameters)
    );
  }

  private static ManifestActionModel GetManifest(ActionSetAnalysis analysis, CatalogEntryModel? catalog, Compilation compilation)
  {
    bool hasMethod = analysis.MethodName.Length > 0 && (analysis.MethodDeclaredByUser || analysis.Failure is null);
    INamedTypeSymbol? trackAction = compilation.GetTypeByMetadataName(TrackActionAttributeMetadataName);
    bool tracked = trackAction is not null
      && analysis.Action.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, trackAction));

    return new ManifestActionModel
    (
      Type: StateSymbolFacts.FullMetadataName(analysis.Action),
      Name: StateSymbolFacts.NestedName(analysis.Action, analysis.State),
      Method: hasMethod ? analysis.MethodName : null,
      Parameters: new EquatableArray<ManifestParameterModel>
      (
        analysis.Construction.Parameters.Select(parameter => new ManifestParameterModel
        (
          parameter.CatalogName,
          parameter.TypeOfName.Replace("global::", ""),
          parameter.DefaultValue,
          Required: parameter.DefaultValue is null
        ))
      ),
      Catalog: catalog?.Name,
      Tracked: tracked
    );
  }

  private static string Keyword(INamedTypeSymbol type) => type switch
  {
    { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
    { IsRecord: true } => "record",
    { TypeKind: TypeKind.Struct } => "struct",
    { TypeKind: TypeKind.Interface } => "interface",
    _ => "class"
  };

  private static string Declaration(INamedTypeSymbol type) =>
    type.TypeParameters.Length == 0
      ? ActionSetConstructorParser.Escape(type.Name)
      : $"{ActionSetConstructorParser.Escape(type.Name)}<{string.Join(", ", type.TypeParameters.Select(parameter => parameter.Name))}>";
}
