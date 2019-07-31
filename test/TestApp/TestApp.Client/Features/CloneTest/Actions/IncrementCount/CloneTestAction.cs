namespace TestApp.Client.Features.CloneTest
{
  using MediatR;
  using TestApp.Shared.Features.Base;

  public class CloneTestAction : BaseRequest, IRequest<CloneTestState>  {  }
}