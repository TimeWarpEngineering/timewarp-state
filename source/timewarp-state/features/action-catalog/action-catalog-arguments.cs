namespace TimeWarp.State;

/// <summary>
/// Argument helpers used by generated <see cref="ActionCatalogEntry"/> executors.
/// </summary>
public static class ActionCatalogArguments
{
  /// <summary>
  /// Throws when the argument count is outside the action's required/total range.
  /// </summary>
  public static void EnsureCount(string actionName, object?[] arguments, int required, int total)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    if (arguments.Length < required || arguments.Length > total)
    {
      string expected = required == total ? $"{total}" : $"{required} to {total}";
      throw new ArgumentException
      (
        $"Action '{actionName}' expects {expected} argument(s) but received {arguments.Length}.",
        nameof(arguments)
      );
    }
  }

  /// <summary>
  /// Returns the argument at <paramref name="index"/> as <typeparamref name="T"/>.
  /// </summary>
  public static T Get<T>(string actionName, object?[] arguments, int index, string parameterName)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    object? value = arguments[index];
    if (value is T typed) return typed;
    if (value is null && default(T) is null) return default!;

    throw new ArgumentException
    (
      $"Action '{actionName}' parameter '{parameterName}' expects {typeof(T)} but received {value?.GetType().ToString() ?? "null"}.",
      nameof(arguments)
    );
  }
}
