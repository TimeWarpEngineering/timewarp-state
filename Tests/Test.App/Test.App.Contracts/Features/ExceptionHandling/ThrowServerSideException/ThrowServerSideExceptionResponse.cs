namespace Test.App.Contracts.Features.ExceptionHandlings;

using System;
using Test.App.Contracts.Features.Base;


public class ThrowServerSideExceptionResponse : BaseResponse
{
  public ThrowServerSideExceptionResponse() { }

  public ThrowServerSideExceptionResponse(Guid aCorrelationId) : base(aCorrelationId) { }
}
