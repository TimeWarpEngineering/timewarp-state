#nullable enable
namespace BlazorState.Extensions;

public static class TypeExtensions
{
  public static Type GetEnclosingStateType(this Type type)
  {
    while (type != null && !typeof(IState).IsAssignableFrom(type))
    {
      type = type.DeclaringType!; // Not null because of analyzer
    }

    return type!; // Not null because of analyzer
  }
}
