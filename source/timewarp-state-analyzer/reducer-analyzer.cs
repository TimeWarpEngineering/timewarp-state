#region Purpose
// TWS0014: a [Reducer] Action the State generator will not emit a Handler for, and why.
#endregion

#region Design
// Task 144, Option 3b. ReducerAttribute derives from TimeWarp.Mediator's GeneratedHandlerAttribute, so the generated
// mediator names <ActionSet>.Handler whether or not the State generator emitted it. When it did not, the user would
// see TWM012 ("does not exist after generation") and CS0234 or CS0246 inside Mediator.g.cs, none of which says what to
// change. This analyzer runs the generator's own rule (ReducerFacts, linked from the source generator project) and
// reports the reason on the attribute.
// It runs on the final compilation, so it sees the Handler the generator emitted. ReducerFacts ignores a member that
// carries the generator's [GeneratedCode] marker, so an emitted Handler is not reported as hand-written.
// Error, because the build already fails in Mediator.g.cs when the handler is missing. A library that references
// TimeWarp.State without a Mediator host gets the same Error: the shape is wrong wherever the host is.
#endregion

namespace TimeWarp.State.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReducerAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "TWS0014";

  private static readonly DiagnosticDescriptor Rule =
    new
    (
      DiagnosticId,
      "[Reducer] handler cannot be generated",
      "[Reducer] on '{0}' gets no generated handler because {1}",
      "Design",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "TimeWarp.State emits the Handler for a [Reducer] Action only when the Action is the nested Action class of a partial *ActionSet in a partial state and the action set declares static void Reduce(TState state, Action action). Otherwise TimeWarp.Mediator dispatches to a type that does not exist."
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      INamedTypeSymbol? reducer = compilationStartContext.Compilation.GetTypeByMetadataName(ReducerFacts.AttributeMetadataName);
      if (reducer is null)
        return;

      compilationStartContext.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, reducer), SymbolKind.NamedType);
    });
  }

  private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol reducer)
  {
    INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
    AttributeData? attribute = ReducerFacts.FindAttribute(type, reducer);
    if (attribute is null)
      return;

    if (ReducerFacts.Analyze(type, attribute, context.Compilation, out string? failure) is not null)
      return;

    Location location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
      ?? type.Locations.FirstOrDefault()
      ?? Location.None;
    context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.ToDisplayString(), failure));
  }
}
