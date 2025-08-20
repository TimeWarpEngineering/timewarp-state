namespace TimeWarp.State.Analyzer;

using Microsoft.CodeAnalysis.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateInheritanceAnalyzer : DiagnosticAnalyzer
{
    public const string InheritanceDiagnosticId = "StateInheritanceTypeArgumentRule";
    public const string SealedDiagnosticId = "StateSealedClassRule";

    private static readonly LocalizableString InheritanceTitle = "Incorrect State<T> inheritance";
    private static readonly LocalizableString InheritanceMessageFormat = "The type argument for State<T> must be the derived class itself";
    private static readonly LocalizableString InheritanceDescription = "When inheriting from State<T>, T must be the name of the derived class.";
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor InheritanceRule = new(InheritanceDiagnosticId, InheritanceTitle, InheritanceMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InheritanceDescription);
    private static readonly LocalizableString SealedTitle = "Non-abstract State<T> class should be sealed";
    private static readonly LocalizableString SealedMessageFormat = "The class '{0}' inheriting from State<T> should be sealed";
    private static readonly LocalizableString SealedDescription = "Non-abstract classes inheriting from State<T> should be sealed to prevent further inheritance.";
    private static readonly DiagnosticDescriptor SealedRule = new(SealedDiagnosticId, SealedTitle, SealedMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: SealedDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InheritanceRule, SealedRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        BaseTypeSyntax? baseType = classDeclaration.BaseList?.Types.FirstOrDefault();
        
        if (baseType == null) return;

        var baseTypeSymbol = context.SemanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
        if (baseTypeSymbol == null || !baseTypeSymbol.Name.Equals("State")) return;

        ITypeSymbol? typeArg = baseTypeSymbol.TypeArguments.FirstOrDefault();
        if (typeArg == null) return;

        INamedTypeSymbol? derivedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (derivedTypeSymbol == null) return;

        if (!SymbolEqualityComparer.Default.Equals(typeArg, derivedTypeSymbol))
        {
            var diagnostic = Diagnostic.Create(InheritanceRule, classDeclaration.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        // Check if the class should be sealed
        if (derivedTypeSymbol is { IsAbstract: false, IsSealed: false })
        {
            var sealedDiagnostic = Diagnostic.Create(SealedRule, classDeclaration.Identifier.GetLocation(), derivedTypeSymbol.Name);
            context.ReportDiagnostic(sealedDiagnostic);
        }
    }
}
