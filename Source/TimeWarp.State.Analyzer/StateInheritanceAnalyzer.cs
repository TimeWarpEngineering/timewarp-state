namespace TimeWarp.State.Analyzer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateInheritanceAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "StateInheritanceRule";

    private static readonly LocalizableString Title = "Incorrect State<T> inheritance";
    private static readonly LocalizableString MessageFormat = "The type argument for State<T> must be the derived class itself";
    private static readonly LocalizableString Description = "When inheriting from State<T>, T must be the name of the derived class.";
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
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
