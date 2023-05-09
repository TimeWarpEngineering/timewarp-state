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

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
  }

  private void AnalyzeNode(SyntaxNodeAnalysisContext context)
  {
    var classDeclaration = (ClassDeclarationSyntax)context.Node;

    // Check if the class implements IAction
    bool implementsIAction = false;
    foreach (var baseType in classDeclaration.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>())
    {
      var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
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

    // Check if the class is nested within a class implementing IState
    var parentClassDeclaration = classDeclaration.Parent as ClassDeclarationSyntax;
    if (parentClassDeclaration == null)
    {
      ReportDiagnostic(context, classDeclaration.Identifier);
      return;
    }

    bool parentImplementsIState = false;
    foreach (var baseType in parentClassDeclaration.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>())
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
      ReportDiagnostic(context, classDeclaration.Identifier);
    }
  }

  private void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxToken identifier)
  {
    var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Text);
    context.ReportDiagnostic(diagnostic);
  }
}
