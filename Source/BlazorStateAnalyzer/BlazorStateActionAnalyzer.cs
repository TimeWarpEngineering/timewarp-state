namespace BlazorStateAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BlazorStateActionAnalyzer : DiagnosticAnalyzer
{
  public const string NestActionInStateDiagnosticId = "TW0001";
  public const string DebugDiagnosticId = "TWD002";

  private static readonly LocalizableString Title = "Blazor State Action should be a nested type of its State";
  private static readonly LocalizableString MessageFormat = "The Action '{0}' is not a nested type of its State";
  private static readonly LocalizableString Description = "Blazor State Actions should be nested types of their corresponding States.";
  private const string Category = "BlazorState";
  private const string IActionDefinition = "BlazorState.IAction";
  private const string IStateDefinition = "BlazorState.IState";
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
        title: "BlazorStateAnalyzerDebug",
        messageFormat: "{0}",
        category: "Debug",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, DebugRule);

  public override void Initialize(AnalysisContext context)
  {
    if (!System.Diagnostics.Debugger.IsAttached)
      System.Diagnostics.Debugger.Launch();

    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.RecordDeclaration);
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.StructDeclaration);
  }

  private static void ReportDebugInformation(SyntaxNodeAnalysisContext context, string message)
  {
    var debugDiagnostic = Diagnostic.Create(DebugRule, context.Node.GetLocation(), message);
    context.ReportDiagnostic(debugDiagnostic);
  }

  private void AnalyzeNode(SyntaxNodeAnalysisContext context)
  {
    if (!System.Diagnostics.Debugger.IsAttached)
      System.Diagnostics.Debugger.Launch();

    ReportDebugInformation(context, $"Starting AnalyzeNode for node {context.Node}");

    var typeDeclaration = (TypeDeclarationSyntax)context.Node;

    if (!ImplementsIAction(context, typeDeclaration))
    {
      // Log the early return
      ReportDebugInformation(context, $"Early return from AnalyzeNode for node {context.Node}");
      return;
    }

    CheckAndReportIfNotNestedInIState(context, typeDeclaration);

    // Log the end of the method
    ReportDebugInformation(context, $"Finished AnalyzeNode for node {context.Node}");
  }

  private static bool ImplementsIAction(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration)
  {
    foreach (BaseTypeSyntax baseType in typeDeclaration.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>())
    {
      var symbolInfo = context.SemanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
      string? originalDefintion = symbolInfo?.OriginalDefinition.ToString();
      ReportDebugInformation(context, originalDefintion ?? "null");

      if (originalDefintion == IActionDefinition)
      {
        return true;
      }
    }

    return false;
  }

  private static void CheckAndReportIfNotNestedInIState(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration)
  {
    // Check all ancestor classes.
    IEnumerable<ClassDeclarationSyntax> classDeclarations = typeDeclaration.Ancestors().OfType<ClassDeclarationSyntax>();
    foreach (ClassDeclarationSyntax classDeclaration in classDeclarations)
    {
      INamedTypeSymbol? classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
      if (classSymbol == null)
      {
        // This shouldn't happen in normal circumstances, but it's good to be safe.
        continue;
      }

      // Look at the interfaces the class implements. If it implements IState, return true.
      foreach (INamedTypeSymbol interfaceSymbol in classSymbol.AllInterfaces)
      {
        if (interfaceSymbol.ToDisplayString() == "BlazorState.IState")
        {
          return;
        }
      }
    }

    ReportDiagnostic(context, typeDeclaration.Identifier);
  }

  private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxToken identifier)
  {
    var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Text);
    context.ReportDiagnostic(diagnostic);
  }
}
