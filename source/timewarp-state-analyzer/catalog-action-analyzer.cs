#region Purpose
// TWS0004-TWS0008: [CatalogAction] placement, Description, per-assembly name uniqueness, and DisplayName.
#endregion

#region Design
// Uses the generator's own placement rule (CatalogActionFacts, linked from the source generator project) so a
// misplaced attribute is an error instead of a silent omission: the target must be a class named Action nested in a
// *ActionSet nested in a type, none of them generic, all at least internal. The class requirement is this
// analyzer's own addition.
// Description is required (TWS0005) and should be one plain sentence (TWS0007: no line breaks, a single
// sentence terminator at most at the end). Names default to <StateWithoutSuffix>.<ActionSetWithoutSuffix>
// (CatalogActionFacts.GetName, the rule the generator applies); duplicates in one compilation are reported at compilation end
// (TWS0006) on every declaration that shares the name. Cross-assembly duplicates are a runtime error in
// ActionCatalog.
// DisplayName is optional; when given it must not be empty or whitespace (TWS0008). An explicit null is the
// same as not setting it. Compile-time constancy needs no rule here: C# only accepts constant attribute
// arguments (CS0182), so a non-constant value never reaches the analyzer as a string.
#endregion

namespace TimeWarp.State.Analyzer;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CatalogActionAnalyzer : DiagnosticAnalyzer
{
  public const string PlacementDiagnosticId = "TWS0004";
  public const string DescriptionDiagnosticId = "TWS0005";
  public const string DuplicateNameDiagnosticId = "TWS0006";
  public const string PlainSentenceDiagnosticId = "TWS0007";
  public const string DisplayNameDiagnosticId = "TWS0008";

  private const string Category = "Design";

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

  private static readonly DiagnosticDescriptor DisplayNameRule =
    new
    (
      DisplayNameDiagnosticId,
      "CatalogAction DisplayName must not be empty",
      "[CatalogAction] DisplayName on '{0}' must not be empty or whitespace. Omit it or set a label.",
      Category,
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "DisplayName is the human-facing label a command palette or menu shows; when given it must be non-empty authored copy."
    );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    ImmutableArray.Create(PlacementRule, DescriptionRule, DuplicateNameRule, PlainSentenceRule, DisplayNameRule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      INamedTypeSymbol? catalogAction = compilationStartContext.Compilation.GetTypeByMetadataName(CatalogActionFacts.AttributeMetadataName);
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
    AttributeData? attribute = CatalogActionFacts.FindAttribute(type, catalogAction);
    if (attribute is null)
      return;

    Location location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
      ?? type.Locations.FirstOrDefault()
      ?? Location.None;

    if (type.TypeKind != TypeKind.Class
        || !CatalogActionFacts.TryGetPlacement(type, out INamedTypeSymbol actionSet, out INamedTypeSymbol state))
    {
      context.ReportDiagnostic(Diagnostic.Create(PlacementRule, location, type.ToDisplayString()));
      return;
    }

    CatalogActionAttributeFacts facts = CatalogActionFacts.ReadAttribute(attribute);
    if (string.IsNullOrWhiteSpace(facts.Description))
    {
      context.ReportDiagnostic(Diagnostic.Create(DescriptionRule, location, type.ToDisplayString()));
    }
    else if (!IsPlainSentence(facts.Description))
    {
      context.ReportDiagnostic(Diagnostic.Create(PlainSentenceRule, location, type.ToDisplayString()));
    }

    if (facts.HasDisplayName && facts.DisplayName is string displayName && string.IsNullOrWhiteSpace(displayName))
    {
      context.ReportDiagnostic(Diagnostic.Create(DisplayNameRule, location, type.ToDisplayString()));
    }

    names.Add((CatalogActionFacts.GetName(facts, state, actionSet), location));
  }

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
}
