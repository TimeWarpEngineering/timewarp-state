namespace BlazorState.Shared.Features.Base
{
  using System;

  public abstract class BaseRequest
  {
    /// <summary>
    /// Every request should have unique Id
    /// </summary>
    public BaseRequest()
    {
      Id = Guid.NewGuid();
    }

    public Guid Id { get; }
  }
}