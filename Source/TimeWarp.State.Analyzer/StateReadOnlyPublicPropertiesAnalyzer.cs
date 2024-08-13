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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        
        if (!InheritsFromState(classDeclaration, context.SemanticModel))
            return;

        foreach (MemberDeclarationSyntax member in classDeclaration.Members)
        {
            if (member is PropertyDeclarationSyntax propertyDeclaration)
            {
                AnalyzeProperty(propertyDeclaration, context);
            }
        }
    }

    private static bool InheritsFromState(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        INamedTypeSymbol? classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null)
            return false;

        INamedTypeSymbol? baseType = classSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "State" && baseType.TypeArguments.Length == 1)
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static void AnalyzeProperty(PropertyDeclarationSyntax propertyDeclaration, SyntaxNodeAnalysisContext context)
    {
        if (!propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)) return;
        
        AccessorDeclarationSyntax? setter = 
          propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        
        if (setter != null && !setter.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            var diagnostic = Diagnostic.Create(Rule, propertyDeclaration.Identifier.GetLocation(), propertyDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
