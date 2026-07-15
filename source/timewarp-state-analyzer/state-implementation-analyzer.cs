namespace TimeWarp.State.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateImplementationAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "TWS001";

  private static readonly LocalizableString Title = "State implementation must implement ICloneable or have a parameterless constructor";
  private static readonly LocalizableString MessageFormat = "The state implementation '{0}' must implement ICloneable or have a parameterless constructor";
  private static readonly LocalizableString Description = "State implementations should either implement ICloneable for proper cloning or have a parameterless constructor for serialization purposes.";
  private const string Category = "Design";

  private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      INamedTypeSymbol? timeWarpState = StateSymbolHelpers.GetTimeWarpStateType(compilationStartContext.Compilation);
      if (timeWarpState is null)
        return;

      compilationStartContext.RegisterSymbolAction(
        symbolContext => AnalyzeSymbol(symbolContext, timeWarpState),
        SymbolKind.NamedType);
    });
  }

  private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol timeWarpState)
  {
    INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

    if (!StateSymbolHelpers.IsTimeWarpState(namedTypeSymbol.BaseType, timeWarpState))
      return;

    if (!ImplementsICloneable(namedTypeSymbol) && !HasParameterlessConstructor(namedTypeSymbol))
    {
      Diagnostic diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
      context.ReportDiagnostic(diagnostic);
    }
  }

  private static bool ImplementsICloneable(INamedTypeSymbol symbol)
  {
    return symbol.AllInterfaces.Any(i => i.Name == "ICloneable");
  }

  private static bool HasParameterlessConstructor(INamedTypeSymbol symbol)
  {
    return symbol.Constructors.Any(c => c.Parameters.Length == 0);
  }
}
