namespace TimeWarp.Features.JavaScriptInterop;

/// <summary>
/// Represents a base class for JSON requests sent to JavaScript functions.
/// This class ensures that every request includes a request type, which can be used
/// to differentiate between various types of requests in JavaScript.
/// </summary>
/// <remarks>
/// Although this class is not used directly within this library, it serves as a template
/// for consumers of the library to understand how to structure their JSON requests.
/// This helps maintain consistency and clarity when performing JavaScript interop operations.
/// </remarks>
public class BaseJsonRequest
{
  /// <summary>
  /// Gets or sets the type of the request.
  /// This property is used to identify the type of the request in JavaScript.
  /// </summary>
  public string RequestType { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="BaseJsonRequest"/> class.
  /// </summary>
  /// <param name="requestType">The type of the request.</param>
  public BaseJsonRequest(string requestType) 
  {
    RequestType = requestType;
  }
}

/// <summary>
/// Represents a JSON request with a payload sent to JavaScript functions.
/// This class extends <see cref="BaseJsonRequest"/> by adding a payload, allowing for
/// flexible and type-safe data to be included in the request.
/// </summary>
/// <typeparam name="TPayload">The type of the payload included in the request.</typeparam>
/// <remarks>
/// Although this class is not used directly within this library, it serves as a template
/// for consumers of the library to understand how to structure their JSON requests with payloads.
/// This helps maintain consistency and clarity when performing JavaScript interop operations.
/// </remarks>
public class JsonRequest<TPayload> : BaseJsonRequest
{
  /// <summary>
  /// Gets or sets the payload of the request.
  /// This property contains the data being sent to the JavaScript function.
  /// </summary>
  public TPayload Payload { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonRequest{TPayload}"/> class.
  /// </summary>
  /// <param name="payload">The payload of the request.</param>
  /// <param name="requestType">The type of the request.</param>
  public JsonRequest(TPayload payload, string requestType)
      : base(requestType)
  {
    Payload = payload;
  }
}
