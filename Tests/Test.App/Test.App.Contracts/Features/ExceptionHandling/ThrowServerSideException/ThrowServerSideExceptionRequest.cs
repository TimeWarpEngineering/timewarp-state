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
