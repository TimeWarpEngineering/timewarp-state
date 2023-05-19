namespace BlazorStateAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BlazorStateActionAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "BlazorStateAction";

  private static readonly LocalizableString Title = "Blazor State Action should be a nested type of its State";
  private static readonly LocalizableString MessageFormat = "The Action '{0}' is not a nested type of its State";
  private static readonly LocalizableString Description = "Blazor State Actions should be nested types of their corresponding States.";
  private const string Category = "BlazorState";

  private static readonly DiagnosticDescriptor Rule = 
    new DiagnosticDescriptor
    (
      DiagnosticId, 
      Title, 
      MessageFormat, 
      Category, 
      DiagnosticSeverity.Error, 
      isEnabledByDefault: true, 
      description: Description
    );

  private static readonly DiagnosticDescriptor DebugRule = 
    new DiagnosticDescriptor
    (
        id: "TW0001",
        title: "BlazorStateAnalyzerDebug",
        messageFormat: "BlazorStateAnalyzerDebug: {0}",
        category: "Debug",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
  { 
      get 
      {
          return ImmutableArray.Create(Rule, DebugRule); 
      } 
  }

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.RecordDeclaration);
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.StructDeclaration);
  }

  private void ReportDebugInformation(SyntaxNodeAnalysisContext context, string message)
  {
      var debugDiagnostic = Diagnostic.Create(DebugRule, context.Node.GetLocation(), message);
      context.ReportDiagnostic(debugDiagnostic);
  }

  private void AnalyzeNode(SyntaxNodeAnalysisContext context)
  {
    var typeDeclaration = (TypeDeclarationSyntax)context.Node;

    // Check if the class implements IAction
    bool implementsIAction = false;
    foreach (var baseType in typeDeclaration.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>())
    {
      var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
      ReportDebugInformation(context, "BlazorStateAnalyzerDebug: " + (typeSymbol?.ToDisplayString() ?? "null"));

      if (typeSymbol?.ToDisplayString() == "BlazorState.IAction")
      {
        implementsIAction = true;
        break;
      }
    }

    if (!implementsIAction)
    {
      return;
    }

    // Check if the type is nested within a type implementing IState
    var parentTypeDeclaration = typeDeclaration.Parent as TypeDeclarationSyntax;
    if (parentTypeDeclaration == null)
    {
      ReportDiagnostic(context, typeDeclaration.Identifier);
      return;
    }

    bool parentImplementsIState = false;
    foreach (var baseType in parentTypeDeclaration.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>())
    {
      var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
      if (typeSymbol?.ToDisplayString() == "BlazorState.IState")
      {
        parentImplementsIState = true;
        break;
      }
    }

    if (!parentImplementsIState)
    {
      ReportDiagnostic(context, typeDeclaration.Identifier);
    }
  }

  private void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxToken identifier)
  {
    var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Text);
    context.ReportDiagnostic(diagnostic);
  }
}
