namespace TimeWarp.State.Analyzer;

using Microsoft.CodeAnalysis.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TimeWarpStateActionAnalyzer : DiagnosticAnalyzer
{
  public const string NestActionInStateDiagnosticId = "TW0001";
  public const string DebugDiagnosticId = "TWD001";
  public const string IActionDefinition = "TimeWarp.State.IAction";
  public const string IStateDefinition = "TimeWarp.State.IState";

  private static readonly LocalizableString Title = "TimeWarp.State Action should be a nested type of its State";
  private static readonly LocalizableString MessageFormat = "The Action '{0}' is not a nested type of its State";
  private static readonly LocalizableString Description = "TimeWarp.State Actions should be nested types of their corresponding States.";
  private const string Category = "TimeWarp.State";

  private static readonly DiagnosticDescriptor Rule =
    new
    (
      NestActionInStateDiagnosticId,
      Title,
      MessageFormat,
      Category,
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: Description
    );

  private static readonly DiagnosticDescriptor DebugRule =
    new
    (
      id: DebugDiagnosticId,
      title: "TimeWarpStateAnalyzerDebug",
      messageFormat: "{0}",
      category: "Debug",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, DebugRule);

  public override void Initialize(AnalysisContext context)
  {
    // LaunchDebugger();

    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration);
    context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.RecordDeclaration);
    context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.StructDeclaration);
  }

  private static void LaunchDebugger()
  {
    if (!System.Diagnostics.Debugger.IsAttached)
      System.Diagnostics.Debugger.Launch();
  }

  private static void ReportDebugInformation(SyntaxNodeAnalysisContext context, string message)
  {
    var debugDiagnostic = Diagnostic.Create(DebugRule, context.Node.GetLocation(), message);
    context.ReportDiagnostic(debugDiagnostic);
  }

  private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
  {
    // This analyzer only concerns itself with type declarations (classes, structs, records).
    if (context.Node is not TypeDeclarationSyntax typeDeclaration) return;

    if (!ImplementsIAction(context, typeDeclaration)) return;

    if (typeDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)) return;

    if (!IsNestedInIState(context, typeDeclaration))
    {
      ReportDiagnostic(context, typeDeclaration.Identifier);
    }
  }

  private static bool ImplementsIAction(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration)
  {
    INamedTypeSymbol? symbolInfo = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

    return ImplementsIAction(symbolInfo);
  }

  private static bool ImplementsIAction(ITypeSymbol? symbolInfo)
  {
    if (symbolInfo == null)
    {
      return false;
    }

    return Enumerable.Any(symbolInfo.Interfaces, namedTypeSymbol => namedTypeSymbol.OriginalDefinition.ToString() == IActionDefinition) ||
      // Recursively check if any base class implements IAction
      ImplementsIAction(symbolInfo.BaseType);
  }


  private static bool IsNestedInIState(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration)
  {
    // Check all ancestor classes.
    IEnumerable<TypeDeclarationSyntax> typeDeclarations = typeDeclaration.Ancestors().OfType<TypeDeclarationSyntax>();
    foreach (TypeDeclarationSyntax ancestorTypeDeclaration in typeDeclarations)
    {
      INamedTypeSymbol? classSymbol = context.SemanticModel.GetDeclaredSymbol(ancestorTypeDeclaration);
      if (classSymbol == null) continue;

      // Look at the interfaces the class implements. If it implements IState, return true.
      if (classSymbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.ToDisplayString() == IStateDefinition))
        return true;
    }

    return false;
  }


  private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxToken identifier)
  {
    var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Text);
    context.ReportDiagnostic(diagnostic);
  }
}
