using System;

namespace BlazorState.Behaviors.JavaScriptInterop
{
  public static class JavaScriptInstanceHelper
  {
    public static JsonRequestHandler JsonRequestHandler { get; set; }

    public static void Handle(string aRequestAsJson)
    {
      if (JsonRequestHandler == null)
      {
        throw new ArgumentNullException(nameof(JsonRequestHandler));
      }

      JsonRequestHandler.Handle(aRequestAsJson);
    }
  }
}