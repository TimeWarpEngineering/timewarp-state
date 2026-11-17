#region Purpose
// [ContractState(typeof(...))]: declares, per contract, the ActionSet the sample's sibling generator emits.
#endregion

#region Design
// Derives from TimeWarp.Mediator.GeneratedRequestPatternAttribute with const templates, the shape Mediator 043
// documents for a consumer attribute. The sibling generator (sample-09-generator) reads this attribute and emits
// <Contract>ActionSet.Action and .Handler inside the state. Mediator dispatches them and TimeWarp.State emits the
// dispatch method, catalog entry and manifest row for them, all from this declaration. The templates must match what
// the sibling emits; TWS0015 and TWM013 report it when they drift.
#endregion

namespace Sample09Console;

/// <summary>Declares the ActionSets the sample's sibling generator emits in the attributed state, one per contract.</summary>
/// <param name="contracts">The contracts to emit an ActionSet for.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractStateAttribute(params Type[] contracts) : GeneratedRequestPatternAttribute(contracts)
{
  /// <summary>The emitted Action, per contract.</summary>
  public const string RequestTypeName = "{Host}.{Contract}ActionSet.Action";
  /// <summary>The emitted Handler, per contract.</summary>
  public const string HandlerTypeName = "{Host}.{Contract}ActionSet.Handler";
  /// <summary>The Action is a void action.</summary>
  public const string RequestInterfaceName = "TimeWarp.Mediator.IAction";
}
