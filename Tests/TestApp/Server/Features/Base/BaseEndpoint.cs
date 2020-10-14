namespace TestApp.Server.Features.Base
{
  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.DependencyInjection;
  using System.Threading.Tasks;
  using TestApp.Api.Features.Base;

  [ApiController]
  public class BaseEndpoint<TRequest, TResponse> : ControllerBase
  where TRequest : IRequest<TResponse>
  where TResponse : BaseResponse
  {
    private ISender sender;

    protected ISender Sender => sender ??= HttpContext.RequestServices.GetService<ISender>();

    protected virtual async Task<IActionResult> Send(TRequest aRequest)
    {
      TResponse response = await Sender.Send(aRequest);

      return Ok(response);
    }
  }
}