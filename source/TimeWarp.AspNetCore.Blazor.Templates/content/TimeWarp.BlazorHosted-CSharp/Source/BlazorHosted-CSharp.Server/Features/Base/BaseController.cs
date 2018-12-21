namespace BlazorHosted_CSharp.Server.Features.Base
{
  using System.Threading.Tasks;
  using BlazorHosted_CSharp.Shared.Features.Base;
  using MediatR;
  using Microsoft.AspNetCore.Mvc;

  public class BaseController<TRequest, TResponse> : Controller
    where TRequest : IRequest<TResponse>
    where TResponse : BaseResponse
  {
    public BaseController(IMediator aMediator)
    {
      Mediator = aMediator;
    }

    protected IMediator Mediator { get; }

    protected virtual async Task<IActionResult> Send(TRequest aRequest)
    {
      TResponse response = await Mediator.Send(aRequest);

      return Ok(response);
    }
  }
}