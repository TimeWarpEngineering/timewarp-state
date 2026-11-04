#region Purpose
// One generated UnsafeAccessor class for a declaring type definition.
#endregion

#region Design
// Map renames the definition's type parameters to T0, T1, ... so the holder can be generic. Members are the accessor signatures emitted into that class.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class Holder
{
  public INamedTypeSymbol Definition { get; init; } = null!;
  public Dictionary<ITypeParameterSymbol, string> Map { get; init; } = null!;
  public string Name { get; init; } = "";
  public string TypeParameters { get; init; } = "";
  public string Self { get; init; } = "";
  public string? Problem { get; set; }
  public List<string> Constraints { get; } = [];
  public List<string> Members { get; } = [];
  public Dictionary<string, string> Methods { get; } = new(StringComparer.Ordinal);
}

