#region Purpose
// One arm of a runtime-type switch the planner has not emitted yet.
#endregion

#region Design
// Depth is filled when the dispatch is materialized, from the named type's base-chain length. Guard limits a non-sealed BCL case to the exact runtime type.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class CaseRequest
{
  public CaseRequest(ITypeSymbol type, string variable, int depth, bool guard)
  {
    Type = type;
    Variable = variable;
    Depth = depth;
    Guard = guard;
  }

  public ITypeSymbol Type { get; }
  public string Variable { get; }
  public int Depth { get; }
  public bool Guard { get; }
}

