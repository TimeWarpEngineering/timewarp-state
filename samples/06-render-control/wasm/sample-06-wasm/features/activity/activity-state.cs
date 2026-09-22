#region Purpose
// One state with a value the UI cares about and a noisy counter beside it.
#endregion

#region Design
// Count is the field a card can register. Beat changes often and is the field a trigger ignores.
// Both live on one state so a subscription without a trigger repaints on either action.
// The store still clones the whole state on every action. A render trigger does not skip that clone.
#endregion

namespace Sample06Wasm.Features.Activity;

/// <summary>
/// A count the UI can pin, plus a beat that moves on its own.
/// </summary>
public sealed partial class ActivityState : State<ActivityState>
{
  public int Count { get; private set; }

  public int Beat { get; private set; }

  public override void Initialize()
  {
    Count = 0;
    Beat = 0;
  }
}
