#region Purpose
// Describes one parameter of a cataloged action (name, CLR type, required/default, JSON schema).
#endregion

#region Design
// Immutable sealed record filled by the generator from the action's first explicit constructor. JsonSchema is null
// for complex types, which are recorded only by ClrType.
#endregion

namespace TimeWarp.State;

/// <summary>
/// One parameter of a cataloged action, taken from the first explicit constructor of the action.
/// </summary>
/// <param name="Name">Constructor parameter name.</param>
/// <param name="ClrType">CLR type of the parameter.</param>
/// <param name="IsRequired">False when the constructor declares a default value.</param>
/// <param name="DefaultValue">C# source text of the default value, or null when required.</param>
/// <param name="JsonSchema">
/// JSON schema for primitives, strings, Guid, DateTime and enums; null for complex types, which v1 records by
/// <paramref name="ClrType"/> only.
/// </param>
public sealed record ActionCatalogParameter
(
  string Name,
  Type ClrType,
  bool IsRequired,
  string? DefaultValue,
  string? JsonSchema
);
