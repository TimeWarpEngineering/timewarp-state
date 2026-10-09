#region Purpose
// Thrown when a type that must be nested in an IState (such as an action) is not.
#endregion

#region Design
// Derives from ArgumentException because it reports an invalid type argument. Only the message constructors are
// provided.
#endregion

namespace TimeWarp.Features.RenderSubscriptions;

public class NonNestedClassException : ArgumentException
{
  public NonNestedClassException(string? message) : base(message) { }
  public NonNestedClassException(string? message, string? paramName) : base(message, paramName) { }
}
