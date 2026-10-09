#region Purpose
// Base class for ActionSet handlers that need the IStore.
#endregion

#region Design
// Derives from TimeWarp.Mediator.ActionHandler<TAction> so the Mediator generator discovers it. It is named
// StateActionHandler, not ActionHandler, to avoid a CS0104 ambiguity when both namespaces are imported.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Base handler for a nested ActionSet <c>Handler</c> that needs the <see cref="IStore"/>.
/// </summary>
/// <remarks>
/// Derives from <see cref="TimeWarp.Mediator.ActionHandler{TAction}"/> (TimeWarp.Mediator.Contracts) so the
/// TimeWarp.Mediator source generator discovers it as the <see cref="IRequestHandler{TRequest}"/> for
/// <typeparamref name="TAction"/>. The name differs from the contracts base type on purpose: both
/// <c>TimeWarp.State</c> and <c>TimeWarp.Mediator</c> are normally imported together, and a second
/// <c>ActionHandler&lt;T&gt;</c> would be ambiguous (CS0104).
/// </remarks>
/// <typeparam name="TAction">The action handled.</typeparam>
public abstract class StateActionHandler<TAction>
(
  IStore store
) : TimeWarp.Mediator.ActionHandler<TAction> where TAction : IAction
{
  protected IStore Store { get; set; } = store;
}
