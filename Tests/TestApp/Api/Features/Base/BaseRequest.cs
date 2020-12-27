namespace TestApp.Api.Features.Base
{
  using System;

  public abstract class BaseRequest
  {
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Every request should have unique Id
    /// </summary>
    public BaseRequest()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}