#region Purpose
// One planned clone for one normalized type.
#endregion

#region Design
// Dependencies drive error propagation and diagnostic reachability. Cases become the runtime-type switch. Exact is the pre-dispatch body of a non-sealed class.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed class Slot
{
  public ITypeSymbol Type { get; init; } = null!;
  public string FullyQualified { get; init; } = "";
  public string MethodName { get; init; } = "";
  public bool IsValueType { get; init; }
  public SlotKind Kind { get; set; }
  public bool IsRoot { get; set; }
  public bool RegisterState { get; set; }
  public bool IsDispatch { get; set; }
  public bool IsCloneable { get; set; }
  public bool PrimaryFailure { get; set; }
  public bool MemberFailure { get; set; }
  public Slot? Cause { get; set; }
  public string? Detail { get; set; }
  public Location? DiagnosticLocation { get; set; }
  public List<string>? Statements { get; set; }
  public List<string>? Fallback { get; set; }
  public Slot? Exact { get; set; }
  public bool ExactIsFallback { get; set; }
  public List<Slot> Dependencies { get; } = [];
  public List<CaseRequest> Cases { get; } = [];
}

