#region Purpose
// TWS0012 (error): a public property on a State<T> has a writable setter.
#endregion

#region Design
// Symbol-based. init-only setters are allowed, including positional record properties, which the
// compiler generates and GeneratedCodeAnalysisFlags.None would otherwise hide. Private, internal,
// private protected, and protected internal setters are allowed. A protected setter is allowed only
// on an abstract state. A public setter is not.
// Resolves State<T> once per compilation and walks the full base-type chain. Members declared on a
// base are not reported again on the derived type.
#endregion

namespace TimeWarp.State.Analyzer;

using Microsoft.CodeAnalysis.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StateReadOnlyPublicPropertiesAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "TWS0012";

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
        SyntaxKind.ClassDeclaration,
        SyntaxKind.RecordDeclaration);
    });
  }

  private static void AnalyzeNode(SyntaxNodeAnalysisContext context, INamedTypeSymbol timeWarpState)
  {
    if (context.Node is not TypeDeclarationSyntax typeDeclaration)
      return;

    INamedTypeSymbol? type = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
    if (type is null || !StateSymbolHelpers.InheritsFromTimeWarpState(type, timeWarpState))
      return;

    foreach (ISymbol member in type.GetMembers())
    {
      if (member is not IPropertySymbol property)
        continue;

      if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
        continue;

      if (property.DeclaredAccessibility != Accessibility.Public)
        continue;

      IMethodSymbol? setter = property.SetMethod;
      if (setter is null || setter.IsInitOnly)
        continue;

      if (IsAllowedSetter(setter, type.IsAbstract))
        continue;

      Location? location = property.Locations.FirstOrDefault();
      if (location is null)
        continue;

      context.ReportDiagnostic(Diagnostic.Create(Rule, location, property.Name));
    }
  }

  private static bool IsAllowedSetter(IMethodSymbol setter, bool isAbstractType) =>
    setter.DeclaredAccessibility switch
    {
      Accessibility.Private => true,
      Accessibility.Internal => true,
      Accessibility.ProtectedAndInternal => true,
      Accessibility.ProtectedOrInternal => true,
      Accessibility.Protected => isAbstractType,
      _ => false
    };
}
