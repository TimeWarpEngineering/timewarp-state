namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class ThrowExceptionActionSet
  {
    internal sealed class Action : IAction
    {
      public required string Message { get; init; }
    }
    
    internal sealed class Handler : BaseActionHandler<Action>
    {
      public Handler(IStore store) : base(store) {}
      
      /// <summary>
      /// Intentionally throw so we can test exception handling.
      /// </summary>
      public override Task<Unit> Handle
      (
        Action action,
        CancellationToken cancellationToken
      ) => throw new Exception(action.Message);
    }
  }
}
