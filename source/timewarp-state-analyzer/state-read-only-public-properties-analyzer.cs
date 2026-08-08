namespace TimeWarp.State.Analyzer;

using Microsoft.CodeAnalysis.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateReadOnlyPublicPropertiesAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "StateReadOnlyPublicPropertiesRule";

  private static readonly LocalizableString Title = "Public property in State class should be read-only";
  private static readonly LocalizableString MessageFormat = "The public property '{0}' in State-derived class should be read-only";
  private static readonly LocalizableString Description = "Public properties in classes inheriting from State should be read-only to enforce immutability.";
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

      compilationStartContext.RegisterSyntaxNodeAction(
        syntaxContext => AnalyzeNode(syntaxContext, timeWarpState),
        SyntaxKind.ClassDeclaration);
    });
  }

  private static void AnalyzeNode(SyntaxNodeAnalysisContext context, INamedTypeSymbol timeWarpState)
  {
    ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;

    if (!StateSymbolHelpers.InheritsFromTimeWarpState(
      context.SemanticModel.GetDeclaredSymbol(classDeclaration),
      timeWarpState))
    {
      return;
    }

    bool isAbstract = classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword);

    foreach (MemberDeclarationSyntax member in classDeclaration.Members)
    {
      if (member is PropertyDeclarationSyntax propertyDeclaration)
      {
        AnalyzeProperty(propertyDeclaration, context, isAbstract);
      }
    }
  }

  private static void AnalyzeProperty(PropertyDeclarationSyntax propertyDeclaration, SyntaxNodeAnalysisContext context, bool isAbstractClass)
  {
    if (!propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
      return;

    AccessorDeclarationSyntax? setter =
      propertyDeclaration.AccessorList?.Accessors
        .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

    if (setter is null)
      return;

    bool isSetterPrivate = setter.Modifiers.Any(SyntaxKind.PrivateKeyword);
    bool isSetterProtected = setter.Modifiers.Any(SyntaxKind.ProtectedKeyword);

    if (!isSetterPrivate && !(isAbstractClass && isSetterProtected))
    {
      Diagnostic diagnostic = Diagnostic.Create(Rule, propertyDeclaration.Identifier.GetLocation(), propertyDeclaration.Identifier.Text);
      context.ReportDiagnostic(diagnostic);
    }
  }
}
