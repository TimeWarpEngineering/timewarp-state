#region Purpose
// Shared helpers for the analyzers to find TimeWarp.State.State<T> and test whether a type is or inherits from it.
#endregion

#region Design
// Compares symbols by OriginalDefinition and SymbolEqualityComparer instead of names, so look-alike types in other
// namespaces do not match.
#endregion

namespace TimeWarp.State.Analyzer;

internal static class StateSymbolHelpers
{
  public const string StateMetadataName = "TimeWarp.State.State`1";

  public static INamedTypeSymbol? GetTimeWarpStateType(Compilation compilation) =>
    compilation.GetTypeByMetadataName(StateMetadataName);

  public static bool IsTimeWarpState(
    INamedTypeSymbol? type,
    INamedTypeSymbol timeWarpStateType) =>
    type is not null
    && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, timeWarpStateType);

  public static bool InheritsFromTimeWarpState(
    INamedTypeSymbol? type,
    INamedTypeSymbol timeWarpStateType)
  {
    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      if (IsTimeWarpState(current, timeWarpStateType))
        return true;
    }

    return false;
  }
}
