namespace TestApp.Server.Features.Base
{
  using System;

  public class BaseException : Exception
  {
    public BaseException() { }
    public BaseException(string aMessage) : base(aMessage) { }
  }
}