#region Purpose
// Request contract and route for the throw-server-side-exception test.
#endregion

#region Design
// GetRoute appends SampleProperty, URL-encoded, as the exception message the server endpoint throws.
#endregion

namespace Test.App.Contracts.Features.ExceptionHandlings;

public class ThrowServerSideExceptionRequest : IRequest<ThrowServerSideExceptionResponse>
{
  public const string RouteTemplate = "api/ExceptionHandlings/ThrowServerSideException";

  /// <summary>
  /// Message the server endpoint throws as <see cref="InvalidOperationException"/>.
  /// </summary>
  public string? SampleProperty { get; set; }

  public string GetRoute() =>
    $"{RouteTemplate}?{nameof(SampleProperty)}={Uri.EscapeDataString(SampleProperty ?? string.Empty)}";
}
