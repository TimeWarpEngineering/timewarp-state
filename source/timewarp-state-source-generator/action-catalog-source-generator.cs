#region Purpose
// Emits the per-assembly action catalog for [CatalogAction] actions: descriptors, registry, executors.
#endregion

#region Design
// ForAttributeWithMetadataName on TimeWarp.State.CatalogActionAttribute keeps the scan opt-in.
// Parameters come from ActionSetConstructorParser, the same parse ActionSetMethodSourceGenerator uses,
// and each executor calls that generated State.Method(args, externalCancellationToken: ct) through
// store.GetState<TState>() — no reflection, AOT/trim safe. Trailing optional parameters may be omitted:
// one call per legal argument count so the generated method's own defaults apply.
// DisplayName is copied verbatim from the attribute literal (null when absent); TWS0008 rejects blank values.
// Misplaced attributes (not a nested *ActionSet.Action) are skipped here; TWS0004 reports them.
// The predicate is TypeDeclarationSyntax, so a record Action is cataloged too. An entry is also skipped when
// ActionSetShape says the generated State.Method cannot exist (not an IState parent, or a TWSG003 failure, which the
// ActionSet method generator reports); an executor would otherwise call a method that is not there (CS1061).
//
// Pipeline (named in TrackingNames, pinned by action-catalog-incrementality-tests): Entries (ForAttributeWithMetadataName,
// equatable CatalogEntryModel), AssemblyName (CompilationProvider.Select), Collected (Entries.Collect + AssemblyName).
//
// Analyzer contract:
// | Assumption                                                        | Guard                                        |
// |-------------------------------------------------------------------|----------------------------------------------|
// | [CatalogAction] is on a nested *ActionSet.Action, none generic,   | TWS0004 (analyzer); generator skips          |
// | all at least internal                                             |                                              |
// | Description is present and one plain sentence                    | TWS0005, TWS0007                             |
// | DisplayName, when given, is not blank                             | TWS0008                                      |
// | catalog names are unique in the assembly                          | TWS0006                                      |
// | the Action may be a record or have a primary constructor          | generator: TypeDeclarationSyntax + parser    |
// | the generated State.Method exists with the parser's parameters    | generator: shares ActionSetShape and parser  |
// | the parent implements IState (store.GetState<T> constraint)       | generator: skips (ActionSetShape)            |
// Output: one internal GeneratedActionCatalog (All) plus an internal assembly attribute deriving from
// ActionCatalogProviderAttribute so AddActionCatalog(assembly) can aggregate without scanning.
// Nothing is emitted for an assembly without cataloged actions.
// The namespace is the sanitized assembly name so assemblies with InternalsVisibleTo do not collide.
#endregion

namespace TimeWarp.State.SourceGenerator;

[Generator]
public class ActionCatalogSourceGenerator : IIncrementalGenerator
{
  public const string CatalogActionAttributeMetadataName = "TimeWarp.State.CatalogActionAttribute";
  public const string HintName = "TimeWarp.State.ActionCatalog.g.cs";

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    IncrementalValuesProvider<CatalogEntryModel> entries = context.SyntaxProvider
      .ForAttributeWithMetadataName
      (
        CatalogActionAttributeMetadataName,
        predicate: static (node, _) => node is TypeDeclarationSyntax and not InterfaceDeclarationSyntax,
        transform: static (ctx, cancellationToken) => GetEntry(ctx, cancellationToken)
      )
      .Where(static entry => entry is not null)
      .Select(static (entry, _) => entry!)
      .WithTrackingName(TrackingNames.CatalogEntries);

    IncrementalValueProvider<string?> assemblyName =
      context.CompilationProvider
        .Select(static (compilation, _) => compilation.AssemblyName)
        .WithTrackingName(TrackingNames.CatalogAssemblyName);

    IncrementalValueProvider<(ImmutableArray<CatalogEntryModel> Left, string? Right)> collected = entries
      .Collect()
      .Combine(assemblyName)
      .WithTrackingName(TrackingNames.CatalogCollected);

    context.RegisterSourceOutput
    (
      collected,
      static (spc, source) => Execute(spc, source.Left, source.Right)
    );
  }

  private static CatalogEntryModel? GetEntry(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
  {
    if (context.TargetSymbol is not INamedTypeSymbol actionSymbol) return null;
    if (actionSymbol.Name != "Action") return null;

    INamedTypeSymbol? actionSetSymbol = actionSymbol.ContainingType;
    if (actionSetSymbol is null || !actionSetSymbol.Name.EndsWith("ActionSet")) return null;

    INamedTypeSymbol? stateSymbol = actionSetSymbol.ContainingType;
    if (stateSymbol is null) return null;
    if (stateSymbol.IsGenericType || actionSetSymbol.IsGenericType || actionSymbol.IsGenericType) return null;
    if (!IsVisibleInAssembly(actionSymbol)) return null;

    ActionSetAnalysis? analysis = ActionSetShape.Analyze(actionSymbol, context.SemanticModel.Compilation, cancellationToken);
    if (analysis is null || analysis.Failure is not null) return null;

    AttributeData attribute = context.Attributes[0];
    string description = string.Empty;
    string? name = null;
    string? displayName = null;
    List<string> permissions = [];
    int visibility = 1;

    foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
    {
      switch (argument.Key)
      {
        case "Description":
          description = argument.Value.Value as string ?? string.Empty;
          break;
        case "Name":
          name = argument.Value.Value as string;
          break;
        case "DisplayName":
          displayName = argument.Value.Value as string;
          break;
        case "Permissions":
          if (!argument.Value.IsNull)
            permissions.AddRange(argument.Value.Values.Select(value => value.Value as string ?? string.Empty));
          break;
        case "Visibility":
          if (argument.Value.Value is int value) visibility = value;
          break;
      }
    }

    string methodName = analysis.MethodName;
    List<ActionParameterModel> parameters = analysis.Construction.Parameters;

    return new CatalogEntryModel
    (
      Name: string.IsNullOrWhiteSpace(name) ? GetDefaultName(stateSymbol.Name, actionSetSymbol.Name) : name!,
      Description: description,
      DisplayName: displayName,
      Permissions: new EquatableArray<string>(permissions),
      Visibility: visibility,
      StateType: stateSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      ActionType: actionSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      MethodName: methodName,
      Parameters: new EquatableArray<ActionParameterModel>(parameters)
    );
  }

  /// <summary>
  /// Default catalog name: &lt;StateWithoutSuffix&gt;.&lt;ActionSetWithoutSuffix&gt;.
  /// </summary>
  public static string GetDefaultName(string stateName, string actionSetName)
  {
    string state = stateName.EndsWith("State") && stateName.Length > "State".Length
      ? stateName.Substring(0, stateName.Length - "State".Length)
      : stateName;
    string actionSet = actionSetName.Substring(0, actionSetName.Length - "ActionSet".Length);
    return $"{state}.{actionSet}";
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

  private static void Execute
  (
    SourceProductionContext context,
    ImmutableArray<CatalogEntryModel> entries,
    string? assemblyName
  )
  {
    if (entries.IsDefaultOrEmpty) return;

    context.AddSource(HintName, GenerateCode(GetNamespace(assemblyName), entries));
  }

  internal static string GetNamespace(string? assemblyName)
  {
    if (string.IsNullOrWhiteSpace(assemblyName)) return "TimeWarp.State.Generated";

    IEnumerable<string> segments = assemblyName!
      .Split('.')
      .Where(segment => segment.Length > 0)
      .Select(segment =>
      {
        var builder = new StringBuilder(segment.Length + 1);
        foreach (char character in segment)
          builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');
        if (char.IsDigit(builder[0])) builder.Insert(0, '_');
        string identifier = builder.ToString();
        return SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None ? identifier : "@" + identifier;
      });

    return string.Join(".", segments);
  }

  private static string GenerateCode(string namespaceName, ImmutableArray<CatalogEntryModel> entries)
  {
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated/>");
    builder.AppendLine("#nullable enable");
    builder.AppendLine("#pragma warning disable CS1591");
    builder.AppendLine();
    builder.AppendLine($"[assembly: global::{namespaceName}.GeneratedActionCatalogProviderAttribute]");
    builder.AppendLine();
    builder.AppendLine($"namespace {namespaceName}");
    builder.AppendLine("{");
    builder.AppendLine("  /// <summary>Actions in this assembly marked with [CatalogAction].</summary>");
    builder.AppendLine("  internal static class GeneratedActionCatalog");
    builder.AppendLine("  {");
    builder.AppendLine("    public static global::System.Collections.Generic.IReadOnlyList<global::TimeWarp.State.ActionCatalogEntry> All { get; } =");
    builder.AppendLine("      new global::TimeWarp.State.ActionCatalogEntry[]");
    builder.AppendLine("      {");

    foreach (CatalogEntryModel entry in entries.OrderBy(entry => entry.Name, StringComparer.Ordinal))
      AppendEntry(builder, entry);

    builder.AppendLine("      };");
    builder.AppendLine("  }");
    builder.AppendLine();
    builder.AppendLine("  internal sealed class GeneratedActionCatalogProviderAttribute : global::TimeWarp.State.ActionCatalogProviderAttribute");
    builder.AppendLine("  {");
    builder.AppendLine("    public override global::System.Collections.Generic.IReadOnlyList<global::TimeWarp.State.ActionCatalogEntry> Entries => GeneratedActionCatalog.All;");
    builder.AppendLine("  }");
    builder.AppendLine("}");
    builder.AppendLine("#pragma warning restore CS1591");
    return builder.ToString();
  }

  private static void AppendEntry(StringBuilder builder, CatalogEntryModel entry)
  {
    string nameLiteral = Literal(entry.Name);
    int total = entry.Parameters.Count;
    int required = entry.Parameters.TakeWhile(parameter => parameter.DefaultValue is null).Count();
    string permissions = entry.Permissions.Count == 0
      ? "global::System.Array.Empty<string>()"
      : $"new string[] {{ {string.Join(", ", entry.Permissions.Select(Literal))} }}";

    builder.AppendLine("        new global::TimeWarp.State.ActionCatalogEntry");
    builder.AppendLine("        (");
    builder.AppendLine($"          name: {nameLiteral},");
    builder.AppendLine($"          description: {Literal(entry.Description)},");
    builder.AppendLine($"          permissions: {permissions},");
    builder.AppendLine($"          visibility: (global::TimeWarp.State.ActionVisibility){entry.Visibility},");
    builder.AppendLine($"          stateType: typeof({entry.StateType}),");
    builder.AppendLine($"          actionType: typeof({entry.ActionType}),");
    builder.AppendLine("          parameters: new global::TimeWarp.State.ActionCatalogParameter[]");
    builder.AppendLine("          {");
    foreach (ActionParameterModel parameter in entry.Parameters)
    {
      builder.AppendLine
      (
        $"            new global::TimeWarp.State.ActionCatalogParameter({Literal(parameter.CatalogName)}, typeof({parameter.TypeOfName}), " +
        $"{(parameter.DefaultValue is null ? "true" : "false")}, {NullableLiteral(parameter.DefaultValue)}, {NullableLiteral(parameter.JsonSchema)}),"
      );
    }
    builder.AppendLine("          },");
    builder.AppendLine($"          inputSchema: {Literal(GetInputSchema(entry))},");
    builder.AppendLine("          executor: static (store, arguments, cancellationToken) =>");
    builder.AppendLine("          {");
    builder.AppendLine($"            global::TimeWarp.State.ActionCatalogArguments.EnsureCount({nameLiteral}, arguments, {required}, {total});");
    builder.AppendLine($"            {entry.StateType} state = store.GetState<{entry.StateType}>();");

    for (int count = required; count <= total; count++)
    {
      IEnumerable<string> arguments = entry.Parameters
        .Take(count)
        .Select((parameter, index) =>
          $"global::TimeWarp.State.ActionCatalogArguments.Get<{parameter.Type}>({nameLiteral}, arguments, {index}, {Literal(parameter.CatalogName)})")
        .Concat(["externalCancellationToken: cancellationToken"]);
      string call = $"state.{entry.MethodName}({string.Join(", ", arguments)})";

      builder.AppendLine
      (
        count == total
          ? $"            return {call};"
          : $"            if (arguments.Length == {count}) return {call};"
      );
    }

    builder.AppendLine("          },");
    builder.AppendLine($"          displayName: {NullableLiteral(entry.DisplayName)}");
    builder.AppendLine("        ),");
  }

  private static string GetInputSchema(CatalogEntryModel entry)
  {
    IEnumerable<string> properties = entry.Parameters.Select(parameter =>
      $"\"{parameter.CatalogName}\":{parameter.JsonSchema ?? $"{{\"x-clr-type\":\"{parameter.TypeOfName.Replace("global::", "")}\"}}"}");
    IEnumerable<string> required = entry.Parameters
      .Where(parameter => parameter.DefaultValue is null)
      .Select(parameter => $"\"{parameter.CatalogName}\"");

    return $"{{\"type\":\"object\",\"properties\":{{{string.Join(",", properties)}}},\"required\":[{string.Join(",", required)}],\"additionalProperties\":false}}";
  }

  private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, quote: true);

  private static string NullableLiteral(string? value) => value is null ? "null" : Literal(value);

  private sealed record CatalogEntryModel
  (
    string Name,
    string Description,
    string? DisplayName,
    EquatableArray<string> Permissions,
    int Visibility,
    string StateType,
    string ActionType,
    string MethodName,
    EquatableArray<ActionParameterModel> Parameters
  );
}
