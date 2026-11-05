#region Purpose
// Incremental generator that adds a method to each state for every nested *ActionSet, for example
// CounterState.IncrementCount(amount), which builds the Action and sends it.
#endregion

#region Design
// Pipeline (every step named in TrackingNames, pinned by action-set-method-incrementality-tests):
//   Candidates  syntax: a class, record or struct named Action directly inside a class named *ActionSet directly
//               inside a type. Semantic: ActionSetShape.Analyze (the parent implements IState). The transform
//               returns an equatable ActionSetMethodResult that holds no symbols and no syntax.
//   Models      the successful results. A model carries no location, so an edit elsewhere in the file leaves it
//               equal and the output Cached.
//   Diagnostics the failures, re-joined to the current compilation's tree when reported.
// Only the first declaration of a partial Action is a candidate, so a partial Action yields one method.
// Parameters come from ActionSetConstructorParser: for a record or struct Action the primary constructor or
// positional record first, then the first explicit constructor; for a plain class Action the first explicit
// constructor first, then the primary one (its old signature). Then `required` members (set by object initializer). The method takes an optional
// CancellationToken linked with the state's own token. The containing-type chain, type parameters and namespace come
// from symbols, so nested, generic and global-namespace states get a partial that merges.
// SG002 (an Info diagnostic per hint name, never release-tracked) is gone. TWSG003 replaces emitting broken code, but
// only for shapes served before (ActionSetShape.WasServedBefore: a plain class Action in a class state). A failing
// record or struct Action, or an ActionSet in a record or struct state, got no code before and compiled, so it is
// skipped silently.
//
// Analyzer contract: every assumption this generator makes about its input, and what guards it.
// | Assumption                                                        | Guard                                       |
// |-------------------------------------------------------------------|---------------------------------------------|
// | the parent of *ActionSet implements IState                        | generator: skip silently (not an action set)|
// | an IAction is nested in an IState                                  | TWS0001 (analyzer)                          |
// | (every TWSG003 row: only for shapes served before; else skipped)  | ActionSetShape.WasServedBefore              |
// | the state and every containing type are partial                   | generator: TWSG003                          |
// | *ActionSet has a prefix (not just "ActionSet")                    | generator: TWSG003                          |
// | Action and ActionSet are not generic                              | generator: TWSG003                          |
// | Action is not abstract                                            | generator: TWSG003                          |
// | Action and its chosen constructor are reachable from the state    | generator: TWSG003                          |
// | constructor shape: primary / positional / explicit / none         | generator: reads all four (was CS7036)      |
// | `required` members are set                                        | generator: become parameters (was CS9035)   |
// | parameter names do not collide (externalCancellationToken, dups)  | generator: TWSG003                          |
// | the state has no non-method member with the method's name         | generator: TWSG003                          |
// | the state does not already declare the same method by hand        | generator: emits nothing, the user's wins   |
// | parameter types are at least as visible as the method             | generator: internal method, else TWSG003    |
// | a default value binds in the state's scope                        | generator: emits the bound constant         |
// | the Action is named exactly Action                                | none, by design: other names get no method  |
// | the Action implements IAction (Sender.Send accepts it)            | none: Send's own constraint reports it      |
// | the state has Sender and CancellationToken (State<T> does)        | none: an IState-only state behaves as before|
#endregion

namespace TimeWarp.State.SourceGenerator;

[Generator]
public class ActionSetMethodSourceGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<ActionSetMethodResult> results = context.SyntaxProvider
      .CreateSyntaxProvider
      (
        predicate: static (node, _) => ActionSetShape.IsCandidateSyntax(node),
        transform: static (syntaxContext, cancellationToken) => GetResult(syntaxContext, cancellationToken)
      )
      .Where(static result => result is not null)
      .Select(static (result, _) => result!)
      .WithTrackingName(TrackingNames.ActionSetCandidates);

    IncrementalValuesProvider<ActionSetMethodModel> models = results
      .Where(static result => result.Model is not null)
      .Select(static (result, _) => result.Model!)
      .WithTrackingName(TrackingNames.ActionSetModels);

    IncrementalValuesProvider<GeneratorDiagnosticInfo> diagnostics = results
      .Where(static result => result.Diagnostic is not null)
      .Select(static (result, _) => result.Diagnostic!)
      .WithTrackingName(TrackingNames.ActionSetDiagnostics);

    context.RegisterSourceOutput(models, static (sourceContext, model) => Execute(model, sourceContext));
    context.RegisterSourceOutput
    (
      diagnostics.Combine(context.CompilationProvider),
      static (sourceContext, pair) => sourceContext.ReportDiagnostic(pair.Left.ToDiagnostic(pair.Right))
    );
  }

  private static ActionSetMethodResult? GetResult(GeneratorSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol action)
      return null;

    // A partial Action has several declarations; only the first one produces the method.
    if (action.DeclaringSyntaxReferences.FirstOrDefault() is not { } first
        || first.SyntaxTree != context.Node.SyntaxTree
        || first.Span != context.Node.Span)
      return null;

    ActionSetAnalysis? analysis = ActionSetShape.Analyze(action, context.SemanticModel.Compilation, cancellationToken);
    if (analysis is null || analysis.MethodDeclaredByUser) return null;

    string memberName = $"{analysis.State.Name}.{analysis.MethodName}";
    if (analysis.Failure is not null)
    {
      // A shape the pre-141 generator ignored compiled before; an Error would break that build (review M1).
      if (!analysis.ServedBefore) return null;

      return new ActionSetMethodResult
      (
        Model: null,
        Diagnostic: new GeneratorDiagnosticInfo
        (
          StateGeneratorDiagnostics.CannotGenerateMemberId,
          LocationInfo.From(((TypeDeclarationSyntax)context.Node).Identifier.GetLocation()),
          new EquatableArray<string>([memberName, analysis.Action.ToDisplayString(), analysis.Failure])
        )
      );
    }

    return new ActionSetMethodResult(CreateModel(analysis), Diagnostic: null);
  }

  private static ActionSetMethodModel CreateModel(ActionSetAnalysis analysis)
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

  private static void Execute(ActionSetMethodModel model, SourceProductionContext context) =>
    context.AddSource(model.HintName, GenerateMethodCode(model));

  internal static string GenerateMethodCode(ActionSetMethodModel model)
  {
    string parameterList = string.Join
    (
      separator: ", ",
      model.Parameters.Select(parameter =>
        $"{parameter.Type} {parameter.Name}{(parameter.DefaultExpression is null ? "" : $" = {parameter.DefaultExpression}")}")
    );

    string argumentList = string.Join
    (
      separator: ", ",
      model.Parameters.Where(parameter => parameter.InitializerMember is null).Select(parameter => parameter.Name)
    );

    string initializer = string.Join
    (
      separator: ", ",
      model.Parameters.Where(parameter => parameter.InitializerMember is not null)
        .Select(parameter => $"{parameter.InitializerMember} = {parameter.Name}")
    );

    string construction = initializer.Length == 0
      ? $"new {model.ActionSetName}.Action({argumentList})"
      : $"new {model.ActionSetName}.Action({argumentList}) {{ {initializer} }}";

    string separator = model.Parameters.Count > 0 ? ", " : "";

    var builder = new StringBuilder();
    builder.AppendLine("#nullable enable");
    builder.AppendLine();
    builder.AppendLine("#pragma warning disable CS1591");
    if (model.NamespaceName is not null)
    {
      builder.AppendLine($"namespace {model.NamespaceName};");
      builder.AppendLine();
    }

    builder.AppendLine("using System.Threading;");
    builder.AppendLine("using System.Threading.Tasks;");
    builder.AppendLine();

    string indent = string.Empty;
    foreach (string header in model.TypeHeaders)
    {
      builder.AppendLine($"{indent}{header}");
      builder.AppendLine($"{indent}{{");
      indent += "    ";
    }

    builder.AppendLine($"{indent}{model.Accessibility} async Task {model.MethodName}({parameterList}{separator}CancellationToken? externalCancellationToken = null)");
    builder.AppendLine($"{indent}{{");
    builder.AppendLine($"{indent}    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue");
    builder.AppendLine($"{indent}        ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)");
    builder.AppendLine($"{indent}        : null;");
    builder.AppendLine();
    builder.AppendLine($"{indent}    await Sender.Send");
    builder.AppendLine($"{indent}    (");
    builder.AppendLine($"{indent}        {construction},");
    builder.AppendLine($"{indent}        linkedCts?.Token ?? CancellationToken");
    builder.AppendLine($"{indent}    );");
    builder.AppendLine($"{indent}}}");

    for (int depth = model.TypeHeaders.Count - 1; depth >= 0; depth--)
    {
      indent = indent.Substring(0, depth * 4);
      builder.AppendLine($"{indent}}}");
    }

    builder.AppendLine("#pragma warning restore CS1591");
    return builder.ToString();
  }
}

internal sealed record ActionSetMethodModel
(
  string? NamespaceName,
  EquatableArray<string> TypeHeaders,
  string ActionSetName,
  string MethodName,
  string Accessibility,
  EquatableArray<ActionParameterModel> Parameters,
  string HintName
);

internal sealed record ActionSetMethodResult
(
  ActionSetMethodModel? Model,
  GeneratorDiagnosticInfo? Diagnostic
);
