#region Purpose
// TWS0004-TWS0007: [CatalogAction] placement, Description, and per-assembly name uniqueness.
#endregion

#region Design
// Mirrors what ActionCatalogSourceGenerator accepts so a misplaced attribute is an error instead of a
// silent omission: the target must be a class named Action nested in a *ActionSet nested in a type, none of them generic, all at least internal.
// Description is required (TWS0005) and should be one plain sentence (TWS0007: no line breaks, a single
// sentence terminator at most at the end). Names default to <StateWithoutSuffix>.<ActionSetWithoutSuffix>,
// the same rule the generator applies; duplicates in one compilation are reported at compilation end
// (TWS0006) on every declaration that shares the name. Cross-assembly duplicates are a runtime error in
// ActionCatalog.
#endregion

namespace TimeWarp.State.Analyzer;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CatalogActionAnalyzer : DiagnosticAnalyzer
{
  public const string PlacementDiagnosticId = "TWS0004";
  public const string DescriptionDiagnosticId = "TWS0005";
  public const string DuplicateNameDiagnosticId = "TWS0006";
  public const string PlainSentenceDiagnosticId = "TWS0007";

  private const string Category = "Design";
  private const string CatalogActionMetadataName = "TimeWarp.State.CatalogActionAttribute";

  private static readonly DiagnosticDescriptor PlacementRule =
    new
    (
      PlacementDiagnosticId,
      "CatalogAction must be on a nested ActionSet Action",
      "[CatalogAction] on '{0}' is not allowed. Apply it to the nested Action class of an *ActionSet.",
      Category,
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "The action catalog generator only catalogs <State>.<Name>ActionSet.Action classes."
    );

  private static readonly DiagnosticDescriptor DescriptionRule =
    new
    (
      DescriptionDiagnosticId,
      "CatalogAction requires a Description",
      "[CatalogAction] on '{0}' must set a non-empty Description",
      Category,
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Description is what a command palette or agent shows for the action; it is required."
    );

  private static readonly DiagnosticDescriptor DuplicateNameRule =
    new
    (
      DuplicateNameDiagnosticId,
      "Duplicate action catalog name",
      "Action catalog name '{0}' is used by more than one [CatalogAction] in this assembly",
      Category,
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "Catalog names identify actions for hosts and agents and must be unique per assembly. Set Name to disambiguate.",
      customTags: WellKnownDiagnosticTags.CompilationEnd
    );

  private static readonly DiagnosticDescriptor PlainSentenceRule =
    new
    (
      PlainSentenceDiagnosticId,
      "CatalogAction Description should be one plain sentence",
      "[CatalogAction] Description on '{0}' should be one plain sentence without line breaks",
      Category,
      DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: "Keep catalog descriptions to a single sentence so they fit a palette row and an agent tool description."
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    ImmutableArray.Create(PlacementRule, DescriptionRule, DuplicateNameRule, PlainSentenceRule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      INamedTypeSymbol? catalogAction = compilationStartContext.Compilation.GetTypeByMetadataName(CatalogActionMetadataName);
      if (catalogAction is null)
        return;

      ConcurrentBag<(string Name, Location Location)> names = [];

      compilationStartContext.RegisterSymbolAction
      (
        symbolContext => AnalyzeNamedType(symbolContext, catalogAction, names),
        SymbolKind.NamedType
      );

      compilationStartContext.RegisterCompilationEndAction(endContext =>
      {
        foreach (IGrouping<string, (string Name, Location Location)> group in names.GroupBy(entry => entry.Name))
        {
          if (group.Count() < 2)
            continue;

          foreach ((string name, Location location) in group.OrderBy(entry => entry.Location.SourceTree?.FilePath).ThenBy(entry => entry.Location.SourceSpan.Start))
            endContext.ReportDiagnostic(Diagnostic.Create(DuplicateNameRule, location, name));
        }
      });
    });
  }

  private static void AnalyzeNamedType
  (
    SymbolAnalysisContext context,
    INamedTypeSymbol catalogAction,
    ConcurrentBag<(string Name, Location Location)> names
  )
  {
    INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
    AttributeData? attribute = type.GetAttributes()
      .FirstOrDefault(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, catalogAction));
    if (attribute is null)
      return;

    Location location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
      ?? type.Locations.FirstOrDefault()
      ?? Location.None;

    INamedTypeSymbol? actionSet = type.ContainingType;
    INamedTypeSymbol? state = actionSet?.ContainingType;
    if (type.TypeKind != TypeKind.Class || type.Name != "Action" || actionSet is null || state is null
        || !actionSet.Name.EndsWith("ActionSet", StringComparison.Ordinal) || actionSet.Name == "ActionSet"
        || state.IsGenericType || actionSet.IsGenericType || type.IsGenericType
        || !IsVisibleInAssembly(type))
    {
      context.ReportDiagnostic(Diagnostic.Create(PlacementRule, location, type.ToDisplayString()));
      return;
    }

    string? description = GetNamedString(attribute, "Description");
    if (string.IsNullOrWhiteSpace(description))
    {
      context.ReportDiagnostic(Diagnostic.Create(DescriptionRule, location, type.ToDisplayString()));
    }
    else if (!IsPlainSentence(description!))
    {
      context.ReportDiagnostic(Diagnostic.Create(PlainSentenceRule, location, type.ToDisplayString()));
    }

    string? name = GetNamedString(attribute, "Name");
    names.Add((string.IsNullOrWhiteSpace(name) ? GetDefaultName(state.Name, actionSet.Name) : name!, location));
  }

  private static bool IsVisibleInAssembly(INamedTypeSymbol symbol)
  {
    for (INamedTypeSymbol? current = symbol; current is not null; current = current.ContainingType)
    {
      switch (current.DeclaredAccessibility)
      {
        case Accessibility.Public:
        case Accessibility.Internal:
        case Accessibility.ProtectedOrInternal:
          continue;
        default:
          return false;
      }
    }

    return true;
  }

  private static string? GetNamedString(AttributeData attribute, string argumentName) =>
    attribute.NamedArguments
      .Where(argument => argument.Key == argumentName)
      .Select(argument => argument.Value.Value as string)
      .FirstOrDefault();

  internal static bool IsPlainSentence(string description)
  {
    string trimmed = description.Trim();
    if (trimmed.IndexOf('\n') >= 0 || trimmed.IndexOf('\r') >= 0)
      return false;

    for (int index = 0; index < trimmed.Length - 1; index++)
    {
      if ((trimmed[index] == '.' || trimmed[index] == '!' || trimmed[index] == '?') && trimmed[index + 1] == ' ')
        return false;
    }

    return true;
  }

  internal static string GetDefaultName(string stateName, string actionSetName)
  {
    string state = stateName.EndsWith("State", StringComparison.Ordinal) && stateName.Length > "State".Length
      ? stateName.Substring(0, stateName.Length - "State".Length)
      : stateName;
    string actionSet = actionSetName.Substring(0, actionSetName.Length - "ActionSet".Length);
    return $"{state}.{actionSet}";
  }
}
