namespace TimeWarp.State.Extensions;

public static class TypeExtensions
{
  public static bool TryGetEnclosingStateType(this Type type, out Type? enclosingStateType)
  {
    Type currentType = type;
    while (currentType.DeclaringType != null && !typeof(IState).IsAssignableFrom(currentType))
    {
      currentType = currentType.DeclaringType;
    }

    if (!typeof(IState).IsAssignableFrom(currentType))
    {
      enclosingStateType = null;
      return false;
    }

    enclosingStateType = currentType;
    return true;
  }

  public static Type GetEnclosingStateType(this Type type)
  {
    if (TryGetEnclosingStateType(type, out Type? enclosingStateType))
    {
      return enclosingStateType!;
    }

    throw new NonNestedClassException
      ($"{type.Name} must be nested in a class that implements {nameof(IState)}");
  }

  public static string GetSimpleName(this Type type)
  {
    ReadOnlySpan<char> nameSpan = type.Name.AsSpan();
    int backtickIndex = nameSpan.IndexOf('`');
    return backtickIndex >= 0 ? nameSpan[..backtickIndex].ToString() : nameSpan.ToString();
  }
}
