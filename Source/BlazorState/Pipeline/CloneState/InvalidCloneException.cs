namespace BlazorState.Pipeline.CloneState;

public class InvalidCloneException : Exception
{
  public Type EnclosingStateType { get; }

  public InvalidCloneException(Type enclosingStateType) : 
    base($"State of type {enclosingStateType} has an invalid clone. For the default clone to work, a parameterless constructor is required.")
  {
    EnclosingStateType = enclosingStateType;
  }
}
