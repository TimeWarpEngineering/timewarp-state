namespace TestApp.Client.Features.CloneTest
{
  using MediatR;
  using TestApp.Api.Features.Base;

  public class CloneTestAction : BaseRequest, IRequest<CloneTestState>  {  }
}