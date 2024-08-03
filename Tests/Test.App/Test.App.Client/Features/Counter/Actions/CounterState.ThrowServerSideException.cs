namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class ThrowServerSideExceptionActionSet
  {

    internal sealed class Action : IAction
    {
      public string Message { get; }
      public Action(string message) 
      {
        Message = message;
      }
    }
    
    internal sealed class Handler : BaseActionHandler<Action>
    {
      private readonly HttpClient HttpClient;
      public Handler
      (
        IStore store,
        HttpClient httpClient
      ) : base(store)
      {
        HttpClient = httpClient;
      }

      /// <summary>
      /// Intentionally throw so we can test exception handling.
      /// </summary>
      public override async Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        var throwServerSideExceptionRequest = new ThrowServerSideExceptionRequest();

        await HttpClient.GetFromJsonAsync<ThrowServerSideExceptionResponse>
        (
          throwServerSideExceptionRequest.GetRoute()
          , cancellationToken: cancellationToken
        );
      }
    }
  }
}
