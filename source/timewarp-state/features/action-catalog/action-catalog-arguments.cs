using System.Globalization;

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
  /// <remarks>
  /// An argument that is already a <typeparamref name="T"/> is returned as is. Otherwise the values the generated
  /// input schema advertises are converted without reflection: enums from a member name (case-insensitive) or an
  /// integral number; <see cref="Guid"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="DateOnly"/>,
  /// <see cref="TimeOnly"/> and <see cref="TimeSpan"/> from a string; and numeric, <see cref="bool"/>, <see cref="char"/>
  /// and <see cref="string"/> parameters from any <see cref="IConvertible"/> (for example int to long or "5" to int),
  /// all using the invariant culture. Anything else, or a failed conversion, throws <see cref="ArgumentException"/>.
  /// </remarks>
  public static T Get<T>(string actionName, object?[] arguments, int index, string parameterName)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    object? value = arguments[index];
    if (value is T typed) return typed;
    if (value is null && default(T) is null) return default!;

    Exception? inner = null;
    if (value is not null)
    {
      try
      {
        if (TryConvert(value, typeof(T), out object? converted) && converted is T convertedTyped) return convertedTyped;
      }
      catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentException)
      {
        inner = exception;
      }
    }

    throw new ArgumentException
    (
      $"Action '{actionName}' parameter '{parameterName}' expects {typeof(T)} but received {value?.GetType().ToString() ?? "null"}.",
      nameof(arguments),
      inner
    );
  }

  private static bool TryConvert(object value, Type targetType, out object? converted)
  {
    Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;
    converted = null;

    if (type.IsEnum)
    {
      if (value is string name)
      {
        converted = Enum.Parse(type, name, ignoreCase: true);
        return true;
      }

      if (value is sbyte or byte or short or ushort or int or uint or long or ulong)
      {
        converted = Enum.ToObject(type, value);
        return true;
      }

      return false;
    }

    if (value is string text)
    {
      if (type == typeof(Guid)) { converted = Guid.Parse(text); return true; }
      if (type == typeof(DateTime)) { converted = DateTime.Parse(text, CultureInfo.InvariantCulture); return true; }
      if (type == typeof(DateTimeOffset)) { converted = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture); return true; }
      if (type == typeof(DateOnly)) { converted = DateOnly.Parse(text, CultureInfo.InvariantCulture); return true; }
      if (type == typeof(TimeOnly)) { converted = TimeOnly.Parse(text, CultureInfo.InvariantCulture); return true; }
      if (type == typeof(TimeSpan)) { converted = TimeSpan.Parse(text, CultureInfo.InvariantCulture); return true; }
    }

    if (value is IConvertible && (type.IsPrimitive || type == typeof(string) || type == typeof(decimal)))
    {
      converted = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
      return true;
    }

    return false;
  }
}
