namespace TestApp.Client.Features.Counter.Components
{
  using System;
  using System.Globalization;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Components;
  using TestApp.Client.Features.Base.Components;
  using static TestApp.Client.Features.Counter.CounterState;

  public class CustomInputBase<T>: BaseInputComponent<T>
  {
    [Parameter] public string Id { get; set; }
    [Parameter] public string Label { get; set; }
    [Parameter] public Expression<Func<T>> ValidationFor { get; set; }

    protected override bool TryParseValueFromString(string aValue, out T aResult, out string aValidationErrorMessage)
    {
      if (typeof(T) == typeof(string))
      {
        aResult = (T)(object)aValue;
        aValidationErrorMessage = null;

        return true;
      }
      else if (typeof(T) == typeof(int))
      {
        int.TryParse(aValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue);
        aResult = (T)(object)parsedValue;
        aValidationErrorMessage = null;

        return true;
      }
      else if (typeof(T) == typeof(Guid))
      {
        Guid.TryParse(aValue, out Guid parsedValue);
        aResult = (T)(object)parsedValue;
        aValidationErrorMessage = null;

        return true;
      }
      else if (typeof(T).IsEnum)
      {
        try
        {
          aResult = (T)Enum.Parse(typeof(T), aValue);
          aValidationErrorMessage = null;

          return true;
        }
        catch (ArgumentException)
        {
          aResult = default;
          aValidationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";

          return false;
        }
      }

      throw new InvalidOperationException($"{GetType()} does not support the type '{typeof(T)}'.");
    }

    protected async Task ButtonClick() => 
      _ = await Mediator.Send(new CounterState.IncrementCounterAction { Amount = int.Parse(CurrentValueAsString) });

  }
}
