#region Purpose
// Turns the GeneratedRequestPattern declarations on one source type into ActionFacts for the actions another
// generator emits under a State type: the dispatch method, the catalog entry, and the manifest row (origin
// "declared"). Task 160, the State half of Mediator task 043.
#endregion

#region Design
// Candidates (IsCandidateSyntax): any type declaration with an attribute list. The pattern attribute may be derived
// (architecture's [ContractState]) and live in a reference, so the name cannot be matched syntactically; Mediator's
// own extractor uses the same predicate. Only the first declaration of a partial host produces facts.
// DeclaredActionFacts (linked into the analyzer too) reads and places the declarations. A declared request that
// already resolves in this compilation was written in source, so ActionFactExtractor serves it and this skips it.
// What a declared action gets, compared with a source action:
// - Dispatch method: State cannot see the Action, so it does not know its constructor. The method takes the Action
//   and sends it: `internal Task Create(CreateActionSet.Action action, CancellationToken? externalCancellationToken)`.
//   Internal because the Action's accessibility is unknown and public could be CS0051; the emitting generator can
//   add a public, typed overload beside it. Skipped when the state already has a member with that name (the user's
//   or another generator's method wins, as ActionSetShape's MethodDeclaredByUser) or when the state chain is not
//   partial (the sibling generator could not nest the Action either, so the compiler already reports it).
// - Catalog entry: always, unless the method is skipped. Name is the default <State>.<ActionSet> rule. Description
//   comes from the contract's System.ComponentModel.DescriptionAttribute or, failing that, the first attribute on the
//   contract with a string named argument Description; Permissions from the first attribute on the contract with a
//   string[] named argument Permissions. Otherwise both are empty. No new State attribute (task 160 scope): the
//   declaring generator owns the contract and its attributes. The one parameter is the Action, so a host passes an
//   Action instance (ActionCatalogArguments.Get returns an argument that already has the type).
// - Manifest row: Type is the runtime name, Method the dispatch method when it exists, the Action parameter, the
//   catalog name, Tracked false (the Action's attributes are invisible), Origin "declared", and the declaration's
//   handler and response.
// - No [Reducer] handler: the emitting generator owns the handler. No diagnostic: Mediator's TWM013 reports a bad
//   pattern, and DeclaredActionAnalyzer (TWS0015) reports a declared action the final compilation lacks.
// Known limit, shared with ActionFactExtractor: the transform reruns when the host's tree changes, so a state that
// becomes partial in another file shows up on the host's next edit.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class DeclaredActionExtractor
{
  private const string ActionParameterName = "action";
  private const string DescriptionAttributeMetadataName = "System.ComponentModel.DescriptionAttribute";

  public static bool IsCandidateSyntax(SyntaxNode node) =>
    node is TypeDeclarationSyntax { AttributeLists.Count: > 0 } and not InterfaceDeclarationSyntax;

  public static EquatableArray<ActionFact>? Extract(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol host) return null;
    if (host.DeclaringSyntaxReferences.FirstOrDefault() is not { } first
        || first.SyntaxTree != context.Node.SyntaxTree
        || first.Span != context.Node.Span)
      return null;

    Compilation compilation = context.SemanticModel.Compilation;
    if (DeclaredActionFacts.GetPatternAttribute(compilation) is not { } patternAttribute) return null;

    List<ActionFact> facts = [];
    HashSet<string> seen = new(StringComparer.Ordinal);
    foreach (DeclaredAction declared in DeclaredActionFacts.Read(host, compilation, patternAttribute, cancellationToken))
    {
      if (!seen.Add(declared.RequestName)) continue;
      if (DeclaredActionFacts.ResolveDotted(compilation, declared.RequestName) is not null) continue;

      facts.Add(CreateFact(declared, compilation));
    }

    return facts.Count == 0 ? null : new EquatableArray<ActionFact>(facts);
  }

  private static ActionFact CreateFact(DeclaredAction declared, Compilation compilation)
  {
    string actionType = "global::" + declared.RequestName;
    ActionParameterModel parameter = new
    (
      Type: actionType,
      Name: ActionParameterName,
      DefaultValue: null,
      TypeOfName: actionType,
      JsonSchema: null,
      CatalogName: ActionParameterName,
      DefaultExpression: null,
      InitializerMember: null
    );

    bool memberExists = declared.State.GetMembers(declared.MethodName).Any();
    bool userMethod = declared.State.GetMembers(declared.MethodName).OfType<IMethodSymbol>().Any();
    ActionSetMethodModel? method = memberExists || !IsPartialChain(declared.State)
      ? null
      : CreateMethodModel(declared, parameter);
    CatalogEntryModel? catalog = method is null ? null : CreateCatalogEntry(declared, parameter, compilation);

    return new ActionFact
    (
      StateSymbolFacts.FullMetadataName(declared.State),
      method,
      Diagnostic: null,
      catalog,
      new ManifestActionModel
      (
        Type: DeclaredActionFacts.RuntimeName(compilation, declared.RequestName),
        Name: declared.ActionSetName + ".Action",
        Method: method is not null || userMethod ? declared.MethodName : null,
        Parameters: new EquatableArray<ManifestParameterModel>([new ManifestParameterModel(ActionParameterName, declared.RequestName, null, Required: true)]),
        Catalog: catalog?.Name,
        Tracked: false,
        Origin: ManifestActionModel.DeclaredOrigin,
        Handler: DeclaredActionFacts.RuntimeName(compilation, declared.HandlerName),
        Response: DeclaredActionFacts.ResponseName(compilation, declared.RequestInterfaceName)
      ),
      Handler: null
    );
  }

  private static bool IsPartialChain(INamedTypeSymbol state)
  {
    for (INamedTypeSymbol? current = state; current is not null; current = current.ContainingType)
    {
      if (!ActionSetShape.IsPartial(current)) return false;
    }

    return true;
  }

  private static ActionSetMethodModel CreateMethodModel(DeclaredAction declared, ActionParameterModel parameter)
  {
    List<string> typeHeaders = [];
    List<string> hintPath = [];
    for (INamedTypeSymbol? current = declared.State; current is not null; current = current.ContainingType)
    {
      typeHeaders.Insert(0, $"partial {ActionFactExtractor.Keyword(current)} {ActionFactExtractor.Declaration(current)}");
      hintPath.Insert(0, current.MetadataName);
    }

    string? namespaceName = declared.State.ContainingNamespace is { IsGlobalNamespace: false } containing
      ? containing.ToDisplayString()
      : null;

    return new ActionSetMethodModel
    (
      namespaceName,
      new EquatableArray<string>(typeHeaders),
      declared.ActionSetName,
      declared.MethodName,
      Accessibility: "internal",
      new EquatableArray<ActionParameterModel>([parameter]),
      HintName: $"{namespaceName ?? "Global"}.{string.Join("+", hintPath)}.{declared.ActionSetName}_Method.g.cs",
      SendsActionParameter: true
    );
  }

  private static CatalogEntryModel CreateCatalogEntry(DeclaredAction declared, ActionParameterModel parameter, Compilation compilation)
  {
    ImmutableArray<AttributeData> attributes = declared.Contract.GetAttributes();
    INamedTypeSymbol? descriptionAttribute = compilation.GetTypeByMetadataName(DescriptionAttributeMetadataName);

    string? description = attributes
      .Where(attribute => descriptionAttribute is not null && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, descriptionAttribute))
      .Select(attribute => attribute.ConstructorArguments.FirstOrDefault().Value as string)
      .FirstOrDefault(value => value is not null);
    description ??= attributes
      .SelectMany(attribute => attribute.NamedArguments)
      .Where(argument => argument.Key == "Description" && argument.Value.Value is string)
      .Select(argument => (string)argument.Value.Value!)
      .FirstOrDefault();

    List<string> permissions = attributes
      .SelectMany(attribute => attribute.NamedArguments)
      .Where(argument => argument.Key == "Permissions" && argument.Value.Kind == TypedConstantKind.Array && !argument.Value.IsNull)
      .Select(argument => argument.Value.Values.Select(value => value.Value as string ?? string.Empty).ToList())
      .FirstOrDefault() ?? [];

    return new CatalogEntryModel
    (
      Name: CatalogActionFacts.GetDefaultName(declared.State.Name, declared.ActionSetName),
      Description: description ?? string.Empty,
      DisplayName: null,
      Permissions: new EquatableArray<string>(permissions),
      Visibility: 1,
      StateType: declared.State.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      ActionType: parameter.Type,
      MethodName: declared.MethodName,
      Parameters: new EquatableArray<ActionParameterModel>([parameter])
    );
  }
}
