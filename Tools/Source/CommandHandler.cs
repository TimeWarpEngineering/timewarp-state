namespace Tools
{
  using System;
  using System.CommandLine;
  using System.CommandLine.Invocation;
  using System.Threading.Tasks;
  using MediatR;

  internal class MediatorCommandHandler : ICommandHandler
  {
    private IMediator Mediator { get; }

    private Type Type { get; }

    public MediatorCommandHandler(Type aType, IMediator aMediator)
    {
      Type = aType;
      Mediator = aMediator;
    }

    public async Task<int> InvokeAsync(InvocationContext aInvocationContext)
    {
      try
      {
        var request = (IRequest)Activator.CreateInstance(Type);
        foreach (SymbolResult symbolResult in aInvocationContext.ParseResult.CommandResult.Children)
        {
          var result = (SuccessfulArgumentParseResult<object>)symbolResult.Result;
          Type.GetProperty(symbolResult.Name).SetValue(request, result.Value);
        }

        await Mediator.Send(request);

        return 0;
      }
      catch (Exception)
      {
        return 1;
      }
    }
  }
}