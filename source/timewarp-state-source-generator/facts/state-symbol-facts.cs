#region Purpose
// Symbol facts about TimeWarp.State states, shared as linked source by the State generator and the State analyzers.
#endregion

#region Design
// One copy of the rules both sides need, so the analyzer and the generator cannot drift (task 140 warned against
// Mediator's two graph builders). The analyzer project compiles this file through <Compile Include ... Link>, so it
// must build in both assemblies: only the global usings both projects share, no generator-only types, internal visibility.
// Symbols are compared by OriginalDefinition with SymbolEqualityComparer, not by name, so look-alike types in other
// namespaces do not match.
// FullMetadataName is the runtime Type.FullName shape (namespace, containing types joined with '+', arity suffix),
// which is how the State manifest names types so DevTools can match them to live types and spans.
#endregion

namespace TimeWarp.State.Facts;

internal static class StateSymbolFacts
{
  public const string StateMetadataName = "TimeWarp.State.State`1";
  public const string StateInterfaceMetadataName = "TimeWarp.State.IState";

  public static INamedTypeSymbol? GetTimeWarpStateType(Compilation compilation) =>
    compilation.GetTypeByMetadataName(StateMetadataName);

  public static bool IsTimeWarpState(INamedTypeSymbol? type, INamedTypeSymbol timeWarpStateType) =>
    type is not null
    && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, timeWarpStateType);

  /// <summary>True when <paramref name="type"/> is <c>State&lt;T&gt;</c> or inherits from it.</summary>
  public static bool InheritsFromTimeWarpState(INamedTypeSymbol? type, INamedTypeSymbol timeWarpStateType)
  {
    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      if (IsTimeWarpState(current, timeWarpStateType))
        return true;
    }

    return false;
  }

  /// <summary>True when a base type of <paramref name="type"/> (not the type itself) is <c>State&lt;T&gt;</c>.</summary>
  public static bool DerivesFromTimeWarpState(INamedTypeSymbol type, INamedTypeSymbol timeWarpStateType) =>
    InheritsFromTimeWarpState(type.BaseType, timeWarpStateType);

  public static bool ImplementsState(INamedTypeSymbol type, INamedTypeSymbol stateInterface)
  {
    foreach (INamedTypeSymbol candidate in type.AllInterfaces)
    {
      if (SymbolEqualityComparer.Default.Equals(candidate, stateInterface))
        return true;
    }

    return false;
  }

  /// <summary>Namespace, containing types joined with '+', then the metadata name: the runtime Type.FullName shape.</summary>
  public static string FullMetadataName(INamedTypeSymbol type)
  {
    List<string> names = [];
    for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
      names.Insert(0, current.MetadataName);

    string typeName = string.Join("+", names);
    return type.ContainingNamespace is { IsGlobalNamespace: false } containing
      ? containing.ToDisplayString() + "." + typeName
      : typeName;
  }

  /// <summary>Type names from <paramref name="type"/> up to, not including, <paramref name="stopAt"/>, joined with '.'.</summary>
  /// <remarks>The same name TimeWarp.State.Telemetry puts in the timewarp.state.action span tag.</remarks>
  public static string NestedName(INamedTypeSymbol type, INamedTypeSymbol? stopAt)
  {
    List<string> names = [];
    for (INamedTypeSymbol? current = type; current is not null && !SymbolEqualityComparer.Default.Equals(current, stopAt); current = current.ContainingType)
      names.Insert(0, current.MetadataName);

    return string.Join(".", names);
  }
}
