namespace BlazorState.Features.JavaScriptInterop
{
  using System;

  /// <summary>
  /// Maintains a static reference to the JsonRequestHandler
  /// </summary>
  /// <remarks>Yes this is service locater anti-pattern. But is cleanest option so far.</remarks>
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