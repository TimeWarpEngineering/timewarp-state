namespace Console_CSharp.Commands.SampleCommand
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  internal class SampleCommandHandler
    : IRequestHandler<SampleCommandRequest>
  {
    public Task<Unit> Handle(SampleCommandRequest aSampleCommandRequest, CancellationToken aCancellationToken)
    {
      Console.WriteLine($"Parameter1:{aSampleCommandRequest.Parameter1}");
      Console.WriteLine($"Parameter1:{aSampleCommandRequest.Parameter2}");
      Console.WriteLine($"Parameter1:{aSampleCommandRequest.Parameter3}");
      return Unit.Task;
    }
  }
}
