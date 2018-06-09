namespace BlazorState.Server.Features.Base
{
  using System.Threading.Tasks;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;

  public class BaseController<TRequest, TResponse> : Controller
    where TRequest : IRequest<TResponse>
    where TResponse : BaseResponse
  {
    public BaseController(ILogger<BaseController<TRequest, TResponse>> aLogger, IMediator aMediator)
    {
      Logger = aLogger;
      Mediator = aMediator;
    }

    protected ILogger Logger { get; }

    protected IMediator Mediator { get; }

    protected virtual async Task<IActionResult> Send(TRequest aRequest)
    {
      TResponse response = await Mediator.Send(aRequest);

      return Ok(response);
    }
  }
}