namespace TestApp.Api.Features.ExceptionHandlings
{
  using MediatR;
  using TestApp.Api.Features.Base;

  public class ThrowServerSideExceptionRequest : BaseRequest, IRequest<ThrowServerSideExceptionResponse>
  {
    public const string RouteTemplate = "api/ExceptionHandlings/ThrowServerSideException";

    /// <summary>
    /// Set Properties and Update Docs
    /// </summary>
    /// <example>TODO</example>
    public string SampleProperty { get; set; }

    public string GetRoute() => $"{RouteTemplate}?{nameof(SampleProperty)}={SampleProperty}&{nameof(CorrelationId)}={CorrelationId}";
  }
}