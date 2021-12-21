namespace TestApp.Client.Features.Counter.Components
{
  using System;
  using System.Globalization;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Components;
  using TestApp.Client.Features.Base.Components;

  public partial class CustomInput<T> : BaseInputComponent<T>
  {
    [Parameter] public string Label { get; set; }
    [Parameter] public Expression<Func<T>> ValidationFor { get; set; }

    protected override bool TryParseValueFromString(string aValue, out T aResult, out string aValidationErrorMessage)
    {
      bool canParse;
      aValidationErrorMessage = null;
      aResult = default;

      if (typeof(T) == typeof(string))
      {
        aResult = (T)(object)aValue;
        canParse = true;
      }
      else if (typeof(T) == typeof(int))
      {
        canParse = int.TryParse(aValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue);
        if (canParse) aResult = (T)(object)parsedValue;
      }
      else if (typeof(T) == typeof(Guid))
      {
        canParse = Guid.TryParse(aValue, out Guid parsedValue);
        if (canParse) aResult = (T)(object)parsedValue; ;
      }
      else if (typeof(T).IsEnum)
      {
        canParse = Enum.TryParse(typeof(T), aValue, out object parsedValue);
        if (canParse) aResult = (T)(object)parsedValue;
      }
      else
      {
        throw new InvalidOperationException($"{GetType()} does not support the type '{typeof(T)}'.");
      }

      if (!canParse)
      {
        aValidationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
      }

      return canParse;
    }

    protected async Task ButtonClick() =>
      _ = await Mediator.Send(new CounterState.IncrementCounterAction { Amount = int.Parse(CurrentValueAsString) });

  }
}