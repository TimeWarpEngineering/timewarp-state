namespace TestApp.Api.Features.ExceptionHandlings
{
  using System;
  using TestApp.Api.Features.Base;


  public class ThrowServerSideExceptionResponse : BaseResponse
  {
    public ThrowServerSideExceptionResponse() { }

    public ThrowServerSideExceptionResponse(Guid aCorrelationId) : base(aCorrelationId) { }
  }
}
