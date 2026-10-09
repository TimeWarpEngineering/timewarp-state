#region Purpose
// Action that makes a failing server call, to test rollback when the exception comes from an HTTP request.
#endregion

#region Design
// GETs ThrowServerSideExceptionRequest's route through the scoped HttpClient and sends action.Message as
// SampleProperty. The test server maps that route and throws InvalidOperationException with the message, so the
// HTTP call fails and StateTransactionBehavior rolls the counter state back.
#endregion

namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public static class ThrowServerSideExceptionActionSet
  {

    public sealed class Action : IAction
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
      public override async ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        ThrowServerSideExceptionRequest throwServerSideExceptionRequest = new()
        {
          SampleProperty = action.Message
        };

        await HttpClient.GetFromJsonAsync<ThrowServerSideExceptionResponse>
        (
          throwServerSideExceptionRequest.GetRoute()
          , cancellationToken: cancellationToken
        );
      }
    }
  }
}
