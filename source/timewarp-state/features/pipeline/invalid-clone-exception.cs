namespace TimeWarp.Features.StateTransactions;

public class InvalidCloneException
(
  Type enclosingStateType
) : Exception($"State of type {enclosingStateType} has an invalid clone. For the default clone to work, a parameterless constructor is required.")
{
  public Type EnclosingStateType { get; } = enclosingStateType;
}
