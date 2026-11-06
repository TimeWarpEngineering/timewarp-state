#region Purpose
// TWS0013 (warning): a concrete TimeWarp.State state that persistence or telemetry snapshots serialize is not listed
// by any JsonSerializerContext in the project, so it has no source-generated JsonTypeInfo.
#endregion

#region Design
// System.Text.Json's generator only reads user source, so TimeWarp cannot generate the [JsonSerializable] line. This
// rule reports the gap and StateJsonSerializableCodeFixProvider (separate assembly) writes the line.
// Reasons:
// - Persistence: the state has [PersistentState]. Persistence falls back to the reflection resolver when the project
//   declares no context, so this reason fires only once at least one context exists.
// - Telemetry: the project references TimeWarp.State.Telemetry. Snapshots skip a state whose TypeInfoResolver returns
//   null and have no reflection fallback, so this reason fires with or without a context.
// Contexts are found by walking the source assembly's types once per compilation (lazy), so the diagnostic is a normal
// symbol diagnostic (editor squiggle and light bulb) rather than a compilation-end one. Only contexts declared in this
// project count; a context in another project cannot be seen and is out of scope.
// Abstract and open generic states are skipped: neither can be listed with typeof(X).
// The State<T> check goes through StateSymbolFacts, the same linked source the other analyzers use.
#endregion

namespace TimeWarp.State.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StateJsonSerializableAnalyzer : DiagnosticAnalyzer
{
  public const string DiagnosticId = "TWS0013";

  /// <summary>Diagnostic property: "true" when the project declares at least one JsonSerializerContext.</summary>
  public const string HasContextPropertyKey = "HasJsonSerializerContext";

  public const string JsonSerializerContextMetadataName = "System.Text.Json.Serialization.JsonSerializerContext";
  public const string JsonSerializableAttributeMetadataName = "System.Text.Json.Serialization.JsonSerializableAttribute";
  public const string PersistentStateAttributeMetadataName = "TimeWarp.Features.Persistence.PersistentStateAttribute";
  public const string TelemetryOptionsMetadataName = "TimeWarp.State.Telemetry.TimeWarpStateTelemetryOptions";

  private const string PersistenceReason = "persistence";
  private const string TelemetryReason = "telemetry snapshots";

  private static readonly LocalizableString Title = "State needs a [JsonSerializable] entry in a JsonSerializerContext";
  private static readonly LocalizableString MessageFormat = "The state '{0}' is serialized by {1} but no JsonSerializerContext in this project lists it; add [JsonSerializable(typeof({0}))]";
  private static readonly LocalizableString Description =
    "Persistence and telemetry snapshots read JsonTypeInfo from TimeWarpStateOptions.JsonSerializerOptions.TypeInfoResolver. " +
    "Telemetry silently skips a state with no type info, and persistence needs it under trimming and AOT. " +
    "List the state on a JsonSerializerContext and set that context as the TypeInfoResolver.";
  private const string Category = "Design";

  private static readonly DiagnosticDescriptor Rule = new
  (
    DiagnosticId,
    Title,
    MessageFormat,
    Category,
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: Description
  );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();

    context.RegisterCompilationStartAction(static compilationStartContext =>
    {
      Compilation compilation = compilationStartContext.Compilation;
      INamedTypeSymbol? timeWarpState = StateSymbolFacts.GetTimeWarpStateType(compilation);
      INamedTypeSymbol? jsonSerializerContext = compilation.GetTypeByMetadataName(JsonSerializerContextMetadataName);
      INamedTypeSymbol? jsonSerializableAttribute = compilation.GetTypeByMetadataName(JsonSerializableAttributeMetadataName);
      if (timeWarpState is null || jsonSerializerContext is null || jsonSerializableAttribute is null)
        return;

      INamedTypeSymbol? persistentStateAttribute = compilation.GetTypeByMetadataName(PersistentStateAttributeMetadataName);
      bool telemetryReferenced = compilation.GetTypeByMetadataName(TelemetryOptionsMetadataName) is not null;

      Lazy<ContextIndex> contextIndex = new
      (
        () => ContextIndex.Build(compilation.Assembly.GlobalNamespace, jsonSerializerContext, jsonSerializableAttribute),
        isThreadSafe: true
      );

      compilationStartContext.RegisterSymbolAction
      (
        symbolContext => AnalyzeSymbol(symbolContext, timeWarpState, persistentStateAttribute, telemetryReferenced, contextIndex),
        SymbolKind.NamedType
      );
    });
  }

  private static void AnalyzeSymbol
  (
    SymbolAnalysisContext context,
    INamedTypeSymbol timeWarpState,
    INamedTypeSymbol? persistentStateAttribute,
    bool telemetryReferenced,
    Lazy<ContextIndex> contextIndex
  )
  {
    INamedTypeSymbol state = (INamedTypeSymbol)context.Symbol;

    if (state.TypeKind != TypeKind.Class || state.IsAbstract || state.IsUnboundGenericType || state.TypeParameters.Length > 0)
      return;

    if (!StateSymbolFacts.InheritsFromTimeWarpState(state.BaseType, timeWarpState))
      return;

    bool persistent = persistentStateAttribute is not null && HasAttribute(state, persistentStateAttribute);
    if (!persistent && !telemetryReferenced)
      return;

    ContextIndex index = contextIndex.Value;
    if (index.ListedTypes.Contains(state))
      return;

    bool persistenceReason = persistent && index.HasContext;
    if (!persistenceReason && !telemetryReferenced)
      return;

    string reason = persistenceReason && telemetryReferenced
      ? PersistenceReason + " and " + TelemetryReason
      : persistenceReason ? PersistenceReason : TelemetryReason;

    ImmutableDictionary<string, string?> properties = ImmutableDictionary<string, string?>.Empty
      .Add(HasContextPropertyKey, index.HasContext ? "true" : "false");

    context.ReportDiagnostic(Diagnostic.Create(Rule, state.Locations[0], properties, state.Name, reason));
  }

  private static bool HasAttribute(INamedTypeSymbol type, INamedTypeSymbol attributeType) =>
    type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));

  /// <summary>
  /// The JsonSerializerContext types declared in the source assembly and the types they list.
  /// </summary>
  private sealed class ContextIndex
  {
    public bool HasContext { get; }
    public ImmutableHashSet<ITypeSymbol> ListedTypes { get; }

    private ContextIndex(bool hasContext, ImmutableHashSet<ITypeSymbol> listedTypes)
    {
      HasContext = hasContext;
      ListedTypes = listedTypes;
    }

    public static ContextIndex Build
    (
      INamespaceSymbol globalNamespace,
      INamedTypeSymbol jsonSerializerContext,
      INamedTypeSymbol jsonSerializableAttribute
    )
    {
      bool hasContext = false;
      ImmutableHashSet<ITypeSymbol>.Builder listedTypes = ImmutableHashSet.CreateBuilder<ITypeSymbol>(SymbolEqualityComparer.Default);

      foreach (INamedTypeSymbol type in GetAllTypes(globalNamespace))
      {
        if (!DerivesFrom(type, jsonSerializerContext))
          continue;

        hasContext = true;
        foreach (AttributeData attribute in type.GetAttributes())
        {
          if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, jsonSerializableAttribute))
            continue;

          if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is ITypeSymbol listedType)
            listedTypes.Add(listedType);
        }
      }

      return new ContextIndex(hasContext, listedTypes.ToImmutable());
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
      for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
      {
        if (SymbolEqualityComparer.Default.Equals(current, baseType))
          return true;
      }

      return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
      foreach (INamespaceOrTypeSymbol member in namespaceSymbol.GetMembers())
      {
        if (member is INamespaceSymbol childNamespace)
        {
          foreach (INamedTypeSymbol type in GetAllTypes(childNamespace))
            yield return type;
        }
        else if (member is INamedTypeSymbol type)
        {
          foreach (INamedTypeSymbol nested in GetSelfAndNestedTypes(type))
            yield return nested;
        }
      }
    }

    private static IEnumerable<INamedTypeSymbol> GetSelfAndNestedTypes(INamedTypeSymbol type)
    {
      yield return type;
      foreach (INamedTypeSymbol nestedType in type.GetTypeMembers())
      {
        foreach (INamedTypeSymbol nested in GetSelfAndNestedTypes(nestedType))
          yield return nested;
      }
    }
  }
}
