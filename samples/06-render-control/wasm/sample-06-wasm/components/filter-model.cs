#region Purpose
// A reference-type parameter so two instances can carry the same text.
#endregion

#region Design
// Sealed class with one init-only Text property. Because it is a reference type, a new instance with the
// same Text is still a changed parameter to Blazor.
#endregion

namespace Sample06Wasm.Components;

/// <summary>
/// Filter text passed by reference. A new instance is a new parameter value to Blazor.
/// </summary>
public sealed class FilterModel
{
  public string Text { get; init; } = "";
}
