#region Purpose
// TWS0009 (error): a concrete type deriving directly from State<T> must implement ICloneable or have a constructor
// the clone source generator can call.
#endregion

#region Design
// Resolves State<T> once per compilation and does nothing when it is absent. Abstract intermediates are exempt.
// An accessible constructor is public, or internal in this assembly. A parameterless constructor is not required.
// ICloneable is matched by interface name only. Unsupported members are TWSG002, not this rule.
#endregion

namespace TimeWarp.State.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateImplementationAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "TWS0009";

  private static readonly LocalizableString Title = "State implementation must implement ICloneable or have an accessible constructor";
  private static readonly LocalizableString MessageFormat = "The state implementation '{0}' must implement ICloneable or have a constructor the clone source generator can call";
  private static readonly LocalizableString Description = "Concrete states need ICloneable or a public or same-assembly internal constructor so the clone source generator can construct a clone. Abstract states are exempt.";
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

    if (namedTypeSymbol.IsAbstract)
      return;

    if (!StateSymbolHelpers.IsTimeWarpState(namedTypeSymbol.BaseType, timeWarpState))
      return;

    if (!ImplementsICloneable(namedTypeSymbol) && !HasAccessibleConstructor(namedTypeSymbol))
    {
      Diagnostic diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
      context.ReportDiagnostic(diagnostic);
    }
  }

  private static bool ImplementsICloneable(INamedTypeSymbol symbol)
  {
    return symbol.AllInterfaces.Any(i => i.Name == "ICloneable");
  }

  private static bool HasAccessibleConstructor(INamedTypeSymbol symbol)
  {
    foreach (IMethodSymbol constructor in symbol.InstanceConstructors)
    {
      if (constructor.Parameters.Any(parameter => parameter.RefKind != RefKind.None))
      {
        continue;
      }

      if (constructor.DeclaredAccessibility == Accessibility.Public)
      {
        return true;
      }

      if (constructor.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal)
      {
        return true;
      }
    }

    return false;
  }
}
