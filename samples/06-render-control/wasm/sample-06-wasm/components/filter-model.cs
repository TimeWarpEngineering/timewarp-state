#region Purpose
// A reference-type parameter so two instances can carry the same text.
#endregion

namespace Sample06Wasm.Components;

/// <summary>
/// Filter text passed by reference. A new instance is a new parameter value to Blazor.
/// </summary>
public sealed class FilterModel
{
  public string Text { get; init; } = "";
}
