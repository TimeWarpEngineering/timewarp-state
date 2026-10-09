#region Purpose
// CloneableState: a state that implements ICloneable, to prove ICloneable wins over the reflection deep clone.
#endregion

#region Design
// Clone() deliberately returns a new instance with Count 42 regardless of the current value, so a test can tell which
// clone path ran. Initialize sets Count to 3.
#endregion

namespace Test.App.Client.Features.CloneTest;

public sealed partial class CloneableState : State<CloneableState>, ICloneable
{
  public int Count { get; private set; }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize() => Count = 3;
  
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>We are trying to prove ICloneable is used when available instead of the reflection deep clone.</remarks>
  /// <returns>New CloneableState object where Count is always 42</returns>
  public object Clone() => new CloneableState { Count = 42 };
}
