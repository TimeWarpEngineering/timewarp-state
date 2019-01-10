namespace Console_CSharp.Commands.SetVersion
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using MediatR;

  internal class SampleCommandHandler
    : IRequestHandler<SampleCommandRequest>
  {
    private string BasePath { get; set; }

    public Task<Unit> Handle(SampleCommandRequest aSampleCommandRequest, CancellationToken aCancellationToken)
    {
      Console.WriteLine($"Parameter1:{aSampleCommandRequest.Parameter1}");
      Console.WriteLine($"Parameter1:{aSampleCommandRequest.Parameter2}");
      Console.WriteLine($"Parameter1:{aSampleCommandRequest.Parameter3}");
      return Unit.Task;
    }
  }
}
