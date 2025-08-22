namespace TimeWarp.Features.ReduxDevTools;

internal class ReduxAction
{
  public ReduxAction(object request)
  {
    ArgumentNullException.ThrowIfNull(request);

    Type = request.GetType().FullName ?? throw new InvalidOperationException();
    Payload = request;
  }

  public object Payload { get; set; }
  public string Type { get; set; }
}
