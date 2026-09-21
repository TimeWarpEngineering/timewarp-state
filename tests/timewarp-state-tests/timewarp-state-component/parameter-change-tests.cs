#region Purpose
// Proves CheckComplexParameterChanged argument order and HandleUnregisteredParameter re-render.
#endregion

namespace ParameterChangeTests;

public class Should_
{
  public static void CheckComplexParameterChanged_Receives_Current_Then_Incoming()
  {
    RecordingComplexParameterComponent sut = new();
    SampleModel currentModel = new() { Version = 1 };
    SampleModel incomingModel = new() { Version = 2 };

    InvokeIgnoringMissingRenderer(() => SetModel(sut, currentModel));
    sut.Model.ShouldBeSameAs(currentModel);
    sut.LastCurrentValue.ShouldBeNull();

    InvokeIgnoringMissingRenderer(() => SetModel(sut, incomingModel));

    sut.LastCurrentValue.ShouldBeSameAs(currentModel);
    sut.LastIncomingValue.ShouldBeSameAs(incomingModel);
  }

  public static void HandleUnregisteredParameter_True_Rerenders_Without_Throwing()
  {
    AllowUnregisteredParameterComponent sut = new();

    InvokeIgnoringMissingRenderer
    (
      () => sut.SetParametersAsync
      (
        ParameterView.FromDictionary
        (
          new Dictionary<string, object?>
          {
            ["Unknown"] = "value"
          }
        )
      )
    );

    bool shouldRender = Should.NotThrow(() => sut.TriggerShouldRender());
    shouldRender.ShouldBeTrue();
    sut.RenderReason.ShouldBe(TimeWarpStateComponent.RenderReasonCategory.ParameterChanged);
    sut.RenderReasonDetail.ShouldBe("Parameter 'Unknown' changed: Unregistered parameter");
  }

  private static Task SetModel(RecordingComplexParameterComponent component, SampleModel model)
  {
    return component.SetParametersAsync
    (
      ParameterView.FromDictionary
      (
        new Dictionary<string, object?>
        {
          [nameof(RecordingComplexParameterComponent.Model)] = model
        }
      )
    );
  }

  private static void InvokeIgnoringMissingRenderer(Func<Task> action)
  {
    try
    {
      action().GetAwaiter().GetResult();
    }
    catch (InvalidOperationException)
    {
      // No renderer in this unit test; parameter checks run before base.SetParametersAsync.
    }
  }
}

[NotTest]
public sealed class SampleModel
{
  public int Version { get; init; }
}

[NotTest]
public abstract class TestTimeWarpStateComponent : TimeWarpStateComponent
{
  protected TestTimeWarpStateComponent()
  {
    SetPrivateInjectedProperty("TimeWarpStateOptions", new TimeWarpStateOptions(new ServiceCollection()));
    SetPrivateInjectedProperty("Logger", NullLogger<TimeWarpStateComponent>.Instance);
  }

  private void SetPrivateInjectedProperty(string propertyName, object value)
  {
    PropertyInfo? propertyInfo = typeof(TimeWarpStateComponent).GetProperty(
      propertyName,
      BindingFlags.NonPublic | BindingFlags.Instance);
    propertyInfo.ShouldNotBeNull();
    propertyInfo!.SetValue(this, value);
  }
}

[NotTest]
public class RecordingComplexParameterComponent : TestTimeWarpStateComponent
{
  [Parameter] public SampleModel? Model { get; set; }

  public object? LastCurrentValue { get; private set; }
  public object? LastIncomingValue { get; private set; }

  protected override bool CheckComplexParameterChanged(string parameterName, object currentValue, object incomingValue)
  {
    LastCurrentValue = currentValue;
    LastIncomingValue = incomingValue;
    return !ReferenceEquals(currentValue, incomingValue);
  }
}

[NotTest]
public class AllowUnregisteredParameterComponent : TestTimeWarpStateComponent
{
  public bool TriggerShouldRender() => ShouldRender();

  protected override bool HandleUnregisteredParameter(ParameterValue parameter) => true;
}
