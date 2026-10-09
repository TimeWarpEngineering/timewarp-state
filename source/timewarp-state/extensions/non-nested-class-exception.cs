#region Purpose
// Thrown when a type that must be nested in an IState (such as an action) is not.
#endregion

#region Design
// Derives from ArgumentException because it reports an invalid type argument. Only the message constructors are
// provided. The type stays in TimeWarp.State: TypeExtensions.GetEnclosingStateType throws it, and it has no
// Blazor dependency. The namespace stays TimeWarp.Features.RenderSubscriptions.
#endregion

namespace TimeWarp.Features.RenderSubscriptions;

public class NonNestedClassException : ArgumentException
{
  public NonNestedClassException(string? message) : base(message) { }
  public NonNestedClassException(string? message, string? paramName) : base(message, paramName) { }
}
