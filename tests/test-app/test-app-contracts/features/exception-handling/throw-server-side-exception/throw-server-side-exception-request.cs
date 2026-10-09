#region Purpose
// Request contract and route for the throw-server-side-exception test.
#endregion

#region Design
// GetRoute appends SampleProperty as a query string; the property is a placeholder (its docs still say TODO).
#endregion

namespace Test.App.Contracts.Features.ExceptionHandlings;

public class ThrowServerSideExceptionRequest : IRequest<ThrowServerSideExceptionResponse>
{
  private const string RouteTemplate = "api/ExceptionHandlings/ThrowServerSideException";

  /// <summary>
  /// Set Properties and Update Docs
  /// </summary>
  /// <example>TODO</example>
  public string? SampleProperty { get; set; }

  public string GetRoute() => $"{RouteTemplate}?{nameof(SampleProperty)}={SampleProperty}";
}
