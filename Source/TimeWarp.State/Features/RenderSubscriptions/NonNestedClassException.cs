namespace TimeWarp.Features.RenderSubscriptions;

public class NonNestedClassException : ArgumentException
{
  public NonNestedClassException(string? message) : base(message) { }
  public NonNestedClassException(string? message, string? paramName) : base(message, paramName) { }
}
