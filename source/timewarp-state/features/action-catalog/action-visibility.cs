#region Purpose
// Says who a cataloged action is offered to: people, agents, or both.
#endregion

#region Design
// [Flags] enum so Both is Human | Agent and callers can test either bit.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Who a cataloged action is offered to.
/// </summary>
[Flags]
public enum ActionVisibility
{
  /// <summary>Shown to people, for example in a command palette.</summary>
  Human = 1,

  /// <summary>Offered to agents, for example as a tool.</summary>
  Agent = 2,

  /// <summary>Offered to people and agents.</summary>
  Both = Human | Agent
}
