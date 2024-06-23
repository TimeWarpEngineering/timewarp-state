namespace TimeWarp.State.Extensions;

public static class TypeExtensions
{
  public static Type GetEnclosingStateType(this Type type)
  {
    string name = type.Name;
    while (!typeof(IState).IsAssignableFrom(type))
    {
      type = type.DeclaringType!; // Not null because of analyzer
    }
    if (type == null)
    {
      throw new NonNestedClassException
      ($"{name} must be nested in a class that implements {nameof(IState)}");
    }

    return type!; // Not null because of analyzer
  }
}
