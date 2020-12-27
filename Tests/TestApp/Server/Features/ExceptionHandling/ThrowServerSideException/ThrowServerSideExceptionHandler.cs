namespace TestApp.Client.Features.ExceptionHandlings
{
  using MediatR;
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Api.Features.ExceptionHandlings;

  public class ThrowServerSideExceptionHandler : IRequestHandler<ThrowServerSideExceptionRequest, ThrowServerSideExceptionResponse>
  {

    public Task<ThrowServerSideExceptionResponse> Handle
    (
      ThrowServerSideExceptionRequest aThrowServerSideExceptionRequest,
      CancellationToken aCancellationToken
    ) => throw new Exception("Server side excpetion");
  }
}
