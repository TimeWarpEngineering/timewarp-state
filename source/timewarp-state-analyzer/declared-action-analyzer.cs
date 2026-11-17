#region Purpose
// TWS0015: a declared action (a request a Mediator GeneratedRequestPattern declares under a State type) is missing
// from the final compilation, so the generator that should emit it and the declaration have drifted.
#endregion

#region Design
// Mirror of Mediator's TWM013 (task 043) from State's side (task 160), so the consumer hears about drift from either
// package. Analyzers run on the compilation after every generator has run, so the Action another generator emits is
// visible here even though State's generator could not see it. DeclaredActionFacts (linked from the generator) reads
// and places the declarations the same way the generator does; a pattern Mediator cannot read is TWM013's to report,
// not this rule's. Reported on the attribute, once per missing action.
//
// Disposition of the other TWS rules for a declared action. The Action and its handler are generated code, and every
// State analyzer uses GeneratedCodeAnalysisFlags.None, so a rule that needs the Action's or the handler's syntax or
// symbol analysis does not run on them:
// | Rule          | Declared action                                                                              |
// |---------------|----------------------------------------------------------------------------------------------|
// | TWS0001       | Holds by construction: DeclaredActionFacts only places a request nested in an IState.         |
// | TWS0002, 0003 | Skipped: they read the handler's method bodies, which the other generator emits.              |
// | TWS0004       | Not applicable: no [CatalogAction]; the entry comes from the declaration's placement.         |
// | TWS0005, 0007 | Skipped: Description comes from the contract's attributes and may be empty by design.        |
// | TWS0006       | Skipped: the declared name is the default <State>.<ActionSet>; a clash with a source          |
// |               | [CatalogAction] Name still throws in ActionCatalog at startup.                                |
// | TWS0008       | Not applicable: a declared entry has no DisplayName.                                          |
// | TWS0009-0012  | Unchanged: they check the state, which is in source.                                          |
// | TWS0013       | Unchanged: it checks states, not actions.                                                     |
// | TWS0014       | Not applicable: the other generator emits the handler; [Reducer] is not involved.             |
#endregion

namespace TimeWarp.State.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeclaredActionAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "TWS0015";

  private static readonly DiagnosticDescriptor Rule =
    new
    (
      DiagnosticId,
      "Declared action is missing",
      "'{0}' declares action '{1}' under state '{2}', but no generator emitted it. Fix the declaration or the generator that emits it.",
      "Design",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "A GeneratedRequestPattern declares an action another source generator emits. TimeWarp.State generates the dispatch method, catalog entry and manifest row from the declaration, so the action must exist after every generator has run."
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      INamedTypeSymbol? patternAttribute = DeclaredActionFacts.GetPatternAttribute(compilationStartContext.Compilation);
      if (patternAttribute is null)
        return;

      compilationStartContext.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, patternAttribute), SymbolKind.NamedType);
    });
  }

  private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol patternAttribute)
  {
    INamedTypeSymbol host = (INamedTypeSymbol)context.Symbol;
    foreach (DeclaredAction declared in DeclaredActionFacts.Read(host, context.Compilation, patternAttribute, context.CancellationToken))
    {
      if (DeclaredActionFacts.ResolveDotted(context.Compilation, declared.RequestName) is not null)
        continue;

      Location location = declared.Attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
        ?? host.Locations.FirstOrDefault()
        ?? Location.None;

      context.ReportDiagnostic
      (
        Diagnostic.Create
        (
          Rule,
          location,
          host.ToDisplayString(),
          declared.RequestName,
          declared.State.ToDisplayString()
        )
      );
    }
  }
}
