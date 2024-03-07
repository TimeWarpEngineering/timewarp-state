namespace TimeWarp.Features.ReduxDevTools;

internal class ReduxAction
{
  public ReduxAction(object request)
  {
    ArgumentNullException.ThrowIfNull(request);

    Type = request.GetType().FullName;
    Payload = request;
  }

  public object Payload { get; set; }
  public string Type { get; set; }
}
