namespace TimeWarp.Features.ReduxDevTools;

internal class ReduxAction
{
  public ReduxAction(object aRequest)
  {
    if (aRequest == null)
    {
      throw new ArgumentNullException(nameof(aRequest));
    }

    Type = aRequest.GetType().Name;
    Payload = aRequest;
  }

  public object Payload { get; set; }
  public string Type { get; set; }
}
