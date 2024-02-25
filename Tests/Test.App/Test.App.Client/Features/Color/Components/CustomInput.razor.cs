namespace Test.App.Client.Features.Color.Components;

public partial class CustomInput<T> : BaseInputComponent<T>
{
  [Parameter] [EditorRequired] public required string Label { get; set; }
  [Parameter] [EditorRequired] public required Expression<Func<T>> ValidationFor { get; set; }

  protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out T result, [NotNullWhen(false)] out string? validationErrorMessage)
  {
    bool canParse;
    validationErrorMessage = null;
    result = default;

    if (value is null)
    {
      validationErrorMessage = $"The {FieldIdentifier.FieldName} field is required.";
      canParse = false;
    }
    else if (typeof(T) == typeof(string))
    {
      result = (T)(object)value;
      canParse = true;
    }
    else if (typeof(T) == typeof(int))
    {
      canParse = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue);
      if (canParse) result = (T)(object)parsedValue;
    }
    else if (typeof(T) == typeof(Guid))
    {
      canParse = Guid.TryParse(value, out Guid parsedValue);
      if (canParse) result = (T)(object)parsedValue;
    }
    else if (typeof(T).IsEnum)
    {
      canParse = Enum.TryParse(typeof(T), value, out object? parsedValue);
      result = canParse ? (T)parsedValue! : default;
    }
    else
    {
      throw new InvalidOperationException($"{GetType()} does not support the type '{typeof(T)}'.");
    }

    if (!canParse)
    {
      validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
    }

    return canParse;
  }
}
