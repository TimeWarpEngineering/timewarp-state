namespace TestApp.Client.Features.ExceptionHandlings
{
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;
  using TestApp.Api.Features.ExceptionHandlings;
  using TestApp.Server.Features.Base;

  public class ThrowServerSideExceptionEndpoint : BaseEndpoint<ThrowServerSideExceptionRequest, ThrowServerSideExceptionResponse>
  {
    /// <summary>
    /// Your summary these comments will show in the Open API Docs
    /// </summary>
    /// <param name="aThrowServerSideExceptionRequest"><see cref="ThrowServerSideExceptionRequest"/></param>
    /// <returns><see cref="ThrowServerSideExceptionResponse"/></returns>
    [HttpGet(ThrowServerSideExceptionRequest.RouteTemplate)]
    public async Task<IActionResult> Process(ThrowServerSideExceptionRequest aThrowServerSideExceptionRequest) =>
      await Send(aThrowServerSideExceptionRequest);
  }
}
