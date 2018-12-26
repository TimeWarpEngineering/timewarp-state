namespace BlazorHosted_CSharp.Shared.Features.Base
{
  using System;

  public abstract class BaseResponse
  {
    public BaseResponse(Guid aRequestId) : this()
    {
      RequestId = aRequestId;
    }

    public BaseResponse()
    {
      ResponseId = Guid.NewGuid();
    }

    /// <summary>
    /// Used to correlate request and response
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Used to correlate request and response
    /// </summary>
    public Guid ResponseId { get; }
  }
}