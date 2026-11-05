#region Purpose
// Emits a Load() partial for top-level [PersistentState] types; rejects nested states.
#endregion

#region Design
// Nested [PersistentState] is unsupported: policies nest actions in states, not states in types.
// TWSG001 + skip emit rather than a containing-type partial chain (not a product feature).
// Hint names include containing types with CLR '+' so AddSource cannot collide if emit is added later.
// Load() sends the shared LoadPersistentStateRequest; PersistentStateMethod is read at runtime.
// Discovery stays a syntax match on the attribute name (no semantic attribute check), so nothing that matched
// before stops matching. It accepts `PersistentState`, `PersistentStateAttribute`, and either one qualified.
// The transform is semantic for the rest: namespace and type parameters come from the symbol, so a global-namespace
// or generic state gets a partial that merges (it used to get `namespace Global;` or a non-generic twin).
// Generated code names Task, CancellationToken and CancellationTokenSource with global:: so it does not depend on the
// consumer's usings, and the partial carries no accessibility modifier so an internal state no longer gets CS0262.
// Success models carry no location; failures carry an equatable LocationInfo re-joined to the current tree on report.
//
// Pipeline (named in TrackingNames, pinned by persistence-incrementality-tests): Candidates (CreateSyntaxProvider
// into an equatable PersistenceResult), Models, Diagnostics.
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

[Generator]
public class PersistenceStateSourceGenerator : IIncrementalGenerator
{
  public const string NestedPersistentStateDiagnosticId = "TWSG001";

  internal static readonly DiagnosticDescriptor NestedPersistentStateRule =
    new
    (
      NestedPersistentStateDiagnosticId,
      title: "[PersistentState] is not supported on nested classes",
      messageFormat: "[PersistentState] is not supported on nested class '{0}'. Move the state to a top-level type.",
      category: "Persistence",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Policies nest actions in states, not states in other types. Nested [PersistentState] would emit a top-level partial that does not merge with the nested type."
    );

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<PersistenceResult> results = context.SyntaxProvider
      .CreateSyntaxProvider
      (
        predicate: static (node, _) => IsCandidateClass(node),
        transform: static (syntaxContext, cancellationToken) => GetResult(syntaxContext, cancellationToken)
      )
      .Where(static result => result is not null)
      .Select(static (result, _) => result!)
      .WithTrackingName(TrackingNames.PersistenceCandidates);

    IncrementalValuesProvider<ClassModel> models = results
      .Where(static result => result.Model is not null)
      .Select(static (result, _) => result.Model!)
      .WithTrackingName(TrackingNames.PersistenceModels);

    IncrementalValuesProvider<GeneratorDiagnosticInfo> diagnostics = results
      .Where(static result => result.Diagnostic is not null)
      .Select(static (result, _) => result.Diagnostic!)
      .WithTrackingName(TrackingNames.PersistenceDiagnostics);

    context.RegisterSourceOutput(models, static (sourceContext, model) => Execute(model, sourceContext));
    context.RegisterSourceOutput
    (
      diagnostics.Combine(context.CompilationProvider),
      static (sourceContext, pair) => sourceContext.ReportDiagnostic(pair.Left.ToDiagnostic(pair.Right))
    );
  }

  private static bool IsCandidateClass(SyntaxNode node)
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

  private static PersistenceResult? GetResult(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
    if (context.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol state)
      return null;

    string className = classDeclaration.Identifier.Text;
    LocationInfo? location = LocationInfo.From(classDeclaration.Identifier.GetLocation());

    if (state.ContainingType is not null)
    {
      return new PersistenceResult
      (
        Model: null,
        new GeneratorDiagnosticInfo(NestedPersistentStateDiagnosticId, location, new EquatableArray<string>([className]))
      );
    }

    string? failure = null;
    if (!classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
      failure = $"'{state.ToDisplayString()}' is not partial; add the partial modifier so the generated Load can join it";

    INamedTypeSymbol? stateBase = context.SemanticModel.Compilation.GetTypeByMetadataName("TimeWarp.State.State`1");
    if (failure is null && stateBase is not null && !DerivesFrom(state, stateBase))
      failure = $"'{state.ToDisplayString()}' does not derive from State<T>, which provides Sender and CancellationToken";

    if (failure is not null)
    {
      return new PersistenceResult
      (
        Model: null,
        new GeneratorDiagnosticInfo
        (
          StateGeneratorDiagnostics.CannotGenerateMemberId,
          location,
          new EquatableArray<string>([$"{state.Name}.Load", state.ToDisplayString(), failure])
        )
      );
    }

    string? namespaceName = state.ContainingNamespace is { IsGlobalNamespace: false } containing
      ? containing.ToDisplayString()
      : null;
    string typeParameters = state.TypeParameters.Length == 0
      ? string.Empty
      : $"<{string.Join(", ", state.TypeParameters.Select(parameter => parameter.Name))}>";
    string hintName = $"{namespaceName ?? "Global"}.{state.MetadataName}_Persistence.g.cs";

    return new PersistenceResult(new ClassModel(namespaceName, className + typeParameters, hintName), Diagnostic: null);
  }

  private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseDefinition)
  {
    for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseDefinition))
        return true;
    }

    return false;
  }

  private static void Execute(ClassModel model, SourceProductionContext context)
  {
    string generatedCode = GenerateLoadClassCode(model.NamespaceName, model.TypeName);
    context.AddSource(model.HintName, SourceText.From(generatedCode, Encoding.UTF8));
  }

  internal static string GenerateLoadClassCode(string? namespaceName, string typeName)
  {
    string namespaceLine = namespaceName is null ? string.Empty : $"namespace {namespaceName};\n";
    return $$$"""
      #nullable enable

      #pragma warning disable CS1591
      {{{namespaceLine}}}
      partial class {{{typeName}}}
      {
        /// <summary>
        /// (Re)loads this [PersistentState] state from its configured persistence store.
        /// </summary>
        public async global::System.Threading.Tasks.Task Load(global::System.Threading.CancellationToken? externalCancellationToken = null)
        {
          using global::System.Threading.CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

          // Load is dispatched through a single hand-written Mediator request whose handler is
          // registered by Mediator's generator (a per-state generated handler would not be).
          await Sender.Send
          (
            new global::TimeWarp.State.Plus.PersistentState.LoadPersistentStateRequest(typeof({{{typeName}}})),
            linkedCts?.Token ?? CancellationToken
          );
        }
      }
      #pragma warning restore CS1591

      """;
  }

  private sealed record ClassModel
  (
    string? NamespaceName,
    string TypeName,
    string HintName
  );

  private sealed record PersistenceResult
  (
    ClassModel? Model,
    GeneratorDiagnosticInfo? Diagnostic
  );
}
