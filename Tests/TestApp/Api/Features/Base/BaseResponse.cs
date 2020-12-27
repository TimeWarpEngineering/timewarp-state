namespace TestApp.Api.Features.Base
{
  using System;

  public abstract class BaseResponse
  {
    public Guid CorrelationId { get; set; }
    public BaseResponse(Guid aCorrelationId) : this()
    {
      CorrelationId = aCorrelationId;
    }

    public BaseResponse() { }

  }
}