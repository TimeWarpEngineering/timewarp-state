namespace System.Reflection;

/// <summary>
/// Use reflection to invoke an Async method.
/// </summary>
/// <seealso href="https://stackoverflow.com/questions/39674988/how-to-call-a-generic-async-method-using-reflection"/>
public static class MethodInfoExtensions
{
  public static async Task<object> InvokeAsync(this MethodInfo aMethodInfo, object aObject, params object[] parameters)
  {
    dynamic awaitable = aMethodInfo.Invoke(aObject, parameters);
    await awaitable;
    return awaitable.GetAwaiter().GetResult();
  }
}
