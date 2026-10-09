#region Purpose
// Thrown when a JavaScript dispatch names a request type that cannot be dispatched or deserialized.
#endregion

#region Design
// Carries the requested type name and appends it to Message. Keeps the standard exception constructors, including
// the legacy serialization constructor.
#endregion

namespace TimeWarp.Features.JavaScriptInterop;

[Serializable]
public class InvalidRequestTypeException : Exception
{
  public string? RequestedTypeAssemblyQualifiedName { get; set; }
  public InvalidRequestTypeException() {}
  public InvalidRequestTypeException(string? message) : base(message) {}
  public InvalidRequestTypeException(string? message, Exception innerException) : base(message, innerException) {}
  public InvalidRequestTypeException(string? message, string requestedTypeAssemblyQualifiedName) : base(message)
  {
    RequestedTypeAssemblyQualifiedName = requestedTypeAssemblyQualifiedName;
  }
  public InvalidRequestTypeException(string? message, string requestedTypeAssemblyQualifiedName, Exception? innerException) : base(message, innerException)
  {
    RequestedTypeAssemblyQualifiedName = requestedTypeAssemblyQualifiedName;
  }

  public override string Message =>
    string.IsNullOrWhiteSpace(RequestedTypeAssemblyQualifiedName) ?
      $"{base.Message}" :
      $"{base.Message} Requested Type: {RequestedTypeAssemblyQualifiedName}";

  // A constructor is needed for serialization when an
  // exception propagates from a remoting server to the client.
  protected InvalidRequestTypeException
  (
    SerializationInfo serializationInfo,
    StreamingContext streamingContext
  ) {}
}
