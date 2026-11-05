#region Purpose
// [CatalogAction] facts shared as linked source by the State generator (catalog entries, manifest) and the
// CatalogActionAnalyzer (TWS0004-TWS0008): placement, assembly visibility, the default catalog name, and the
// attribute's named arguments.
#endregion

#region Design
// Before task 142 the generator and the analyzer each had their own copy of the placement rule, the visibility walk
// and the default-name rule. They now share this one file. The analyzer project compiles it via
// <Compile Include ... Link>, so it must build in both assemblies: only Roslyn and BCL types, only the global usings
// both projects share, no generator-only types, internal visibility.
// Placement: the Action is named Action, nested in a type whose name ends in ActionSet and is longer than
// "ActionSet", nested in a type; none of the three is generic; every one is at least internal. The analyzer adds one
// rule of its own (the Action must be a class, TWS0004). The generator also asks ActionSetShape whether the
// State.Method can exist, so an entry is skipped where an executor would not compile.
// ReadAttribute mirrors the CatalogActionAttribute properties: Description, Name, DisplayName, Permissions,
// Visibility (default Human = 1).
#endregion

namespace TimeWarp.State.Facts;

internal sealed class CatalogActionAttributeFacts
{
  public string Description { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? DisplayName { get; set; }
  public bool HasDisplayName { get; set; }
  public List<string> Permissions { get; } = [];
  public int Visibility { get; set; } = 1;
}

internal static class CatalogActionFacts
{
  public const string AttributeMetadataName = "TimeWarp.State.CatalogActionAttribute";

  /// <summary>
  /// The catalog placement rule: <paramref name="action"/> is <c>State.XActionSet.Action</c>, none generic, all at
  /// least internal.
  /// </summary>
  public static bool TryGetPlacement(INamedTypeSymbol action, out INamedTypeSymbol actionSet, out INamedTypeSymbol state)
  {
    actionSet = null!;
    state = null!;
    if (action.Name != "Action") return false;
    if (action.ContainingType is not { } containingSet) return false;
    if (!containingSet.Name.EndsWith("ActionSet", StringComparison.Ordinal) || containingSet.Name == "ActionSet") return false;
    if (containingSet.ContainingType is not { } containingState) return false;
    if (containingState.IsGenericType || containingSet.IsGenericType || action.IsGenericType) return false;
    if (!IsVisibleInAssembly(action)) return false;

    actionSet = containingSet;
    state = containingState;
    return true;
  }

  public static bool IsVisibleInAssembly(INamedTypeSymbol symbol)
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

  /// <summary>Default catalog name: &lt;StateWithoutSuffix&gt;.&lt;ActionSetWithoutSuffix&gt;.</summary>
  public static string GetDefaultName(string stateName, string actionSetName)
  {
    string state = stateName.EndsWith("State", StringComparison.Ordinal) && stateName.Length > "State".Length
      ? stateName.Substring(0, stateName.Length - "State".Length)
      : stateName;
    string actionSet = actionSetName.Substring(0, actionSetName.Length - "ActionSet".Length);
    return $"{state}.{actionSet}";
  }

  public static AttributeData? FindAttribute(INamedTypeSymbol type, INamedTypeSymbol catalogAction) =>
    type.GetAttributes().FirstOrDefault(data => SymbolEqualityComparer.Default.Equals(data.AttributeClass, catalogAction));

  public static CatalogActionAttributeFacts ReadAttribute(AttributeData attribute)
  {
    CatalogActionAttributeFacts facts = new();
    foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
    {
      switch (argument.Key)
      {
        case "Description":
          facts.Description = argument.Value.Value as string ?? string.Empty;
          break;
        case "Name":
          facts.Name = argument.Value.Value as string;
          break;
        case "DisplayName":
          facts.HasDisplayName = argument.Value.Kind != TypedConstantKind.Error;
          facts.DisplayName = argument.Value.Value as string;
          break;
        case "Permissions":
          if (!argument.Value.IsNull)
            facts.Permissions.AddRange(argument.Value.Values.Select(value => value.Value as string ?? string.Empty));
          break;
        case "Visibility":
          if (argument.Value.Value is int value) facts.Visibility = value;
          break;
      }
    }

    return facts;
  }

  /// <summary>The catalog name: the attribute's Name when set, otherwise the default name.</summary>
  public static string GetName(CatalogActionAttributeFacts facts, INamedTypeSymbol state, INamedTypeSymbol actionSet) =>
    string.IsNullOrWhiteSpace(facts.Name) ? GetDefaultName(state.Name, actionSet.Name) : facts.Name!;
}
