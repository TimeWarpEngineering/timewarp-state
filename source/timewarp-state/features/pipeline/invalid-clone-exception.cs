#region Purpose
// Thrown by StateTransactionBehavior when a state clone is unusable (its Guid is empty or equal to the original's).
#endregion

#region Design
// Records the enclosing state type. Primary constructor with a fixed message that points at the
// parameterless-constructor requirement.
#endregion

namespace TimeWarp.Features.StateTransactions;

public class InvalidCloneException
(
  Type enclosingStateType
) : Exception($"State of type {enclosingStateType} has an invalid clone. For the default clone to work, a parameterless constructor is required.")
{
  public Type EnclosingStateType { get; } = enclosingStateType;
}
