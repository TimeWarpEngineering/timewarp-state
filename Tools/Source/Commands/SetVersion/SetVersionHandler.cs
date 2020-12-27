namespace Tools.Commands.SetVersion
{
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  internal class SetVersionHandler : IRequestHandler<SetVersionRequest>
  {
    private IMediator Mediator { get; }

    public SetVersionHandler(IMediator aMediator)
    {
      Mediator = aMediator;
    }

    public Task<Unit> Handle(SetVersionRequest aSetVersionRequest, CancellationToken aCancellationToken)
    {
      if (aSetVersionRequest.Project == SetVersionRequest.ProjectList[0])
      {
        Mediator.Send(new SetBlazorStateVersionRequest
        {
          Major = aSetVersionRequest.Major,
          Minor = aSetVersionRequest.Minor,
          Patch = aSetVersionRequest.Patch
        });
      };

      return Unit.Task;
    }
  }
}