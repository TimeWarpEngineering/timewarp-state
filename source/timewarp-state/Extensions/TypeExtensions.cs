namespace TimeWarp.State.Extensions;

public static class TypeExtensions
{
  public static Type GetEnclosingStateType(this Type type)
  {
    string name = type.Name;
    while (type.DeclaringType != null && !typeof(IState).IsAssignableFrom(type))
    {
      type = type.DeclaringType;
    }

    if (!typeof(IState).IsAssignableFrom(type))
    {
      throw new NonNestedClassException
        ($"{name} must be nested in a class that implements {nameof(IState)}");
    }

    return type;
  }

  public static string GetSimpleName(this Type type)
  {
    ReadOnlySpan<char> nameSpan = type.Name.AsSpan();
    int backtickIndex = nameSpan.IndexOf('`');
    return backtickIndex >= 0 ? nameSpan[..backtickIndex].ToString() : nameSpan.ToString();
  }
}
