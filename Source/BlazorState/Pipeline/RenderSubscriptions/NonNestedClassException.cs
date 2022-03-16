#nullable enable
namespace BlazorState.Pipeline.RenderSubscriptions;

using System;

public class NonNestedClassException : ArgumentException 
{
  public NonNestedClassException(string? message) : base(message) { }
  public NonNestedClassException(string? message, string? paramName) : base(message, paramName) { }
}
