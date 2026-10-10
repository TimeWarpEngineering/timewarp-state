#region Purpose
// Proves parameter comparison: complex argument order, unregistered re-render,
// same-count collection replacement, and equal value types.
#endregion

#region Design
// Test components set the private injected options and logger by reflection and are driven through SetParametersAsync
// with no renderer; the InvalidOperationException thrown after the parameter checks is ignored.
// CheckedParameterComponent overrides the primitive hook and calls the base method, so comparison runs
// while collection and value-type checks stay on the base implementation.
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

  public static void SameCount_DifferentItems_Rerenders()
  {
    SampleModel firstItem = new() { Version = 1 };
    SampleModel secondItem = new() { Version = 2 };
    CheckedParameterComponent models = ApplyReplacement
    (
      nameof(CheckedParameterComponent.Models),
      new List<SampleModel> { firstItem, secondItem },
      new List<SampleModel>
      {
        new() { Version = 3 },
        new() { Version = 4 }
      }
    );

    models.TriggerShouldRender().ShouldBeTrue();
    models.RenderReason.ShouldBe(TimeWarpStateComponent.RenderReasonCategory.ParameterChanged);
    models.RenderReasonDetail.ShouldBe("Parameter 'Models' changed");

    CheckedParameterComponent numbers = ApplyReplacement
    (
      nameof(CheckedParameterComponent.Numbers),
      new List<int> { 1, 2 },
      new List<int> { 1, 3 }
    );

    numbers.TriggerShouldRender().ShouldBeTrue();
    numbers.RenderReasonDetail.ShouldBe("Parameter 'Numbers' changed");
  }

  public static void SameItems_DoNotReportAChange()
  {
    SampleModel firstItem = new() { Version = 1 };
    SampleModel secondItem = new() { Version = 2 };
    CheckedParameterComponent models = ApplyReplacement
    (
      nameof(CheckedParameterComponent.Models),
      new List<SampleModel> { firstItem, secondItem },
      new List<SampleModel> { firstItem, secondItem }
    );

    models.TriggerShouldRender().ShouldBeFalse();

    CheckedParameterComponent numbers = ApplyReplacement
    (
      nameof(CheckedParameterComponent.Numbers),
      new List<int> { 1, 2 },
      new List<int> { 1, 2 }
    );

    numbers.TriggerShouldRender().ShouldBeFalse();
    numbers.RenderReasonDetail.ShouldBe("Parameter 'Numbers' changed: Null value change");
  }

  public static void Equal_ValueType_Parameters_DoNotReportAChange()
  {
    AssertEqualValueDoesNotReportAChange
    (
      nameof(CheckedParameterComponent.Timestamp),
      new DateTime(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc),
      new DateTime(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc)
    );
    AssertEqualValueDoesNotReportAChange
    (
      nameof(CheckedParameterComponent.Identifier),
      Guid.Parse("11111111-1111-1111-1111-111111111111"),
      Guid.Parse("22222222-2222-2222-2222-222222222222")
    );
    AssertEqualValueDoesNotReportAChange
    (
      nameof(CheckedParameterComponent.Amount),
      12.50m,
      13.00m
    );
    AssertEqualValueDoesNotReportAChange
    (
      nameof(CheckedParameterComponent.NullableAmount),
      12.50m,
      13.00m
    );
    AssertEqualValueDoesNotReportAChange
    (
      nameof(CheckedParameterComponent.Kind),
      SampleKind.Some,
      SampleKind.Other
    );
    AssertEqualValueDoesNotReportAChange
    (
      nameof(CheckedParameterComponent.Point),
      new SamplePoint { X = 1, Y = 2 },
      new SamplePoint { X = 3, Y = 4 }
    );
  }

  public static void Query_IsNotEnumerated_And_ADifferentInstance_Rerenders()
  {
    CheckedParameterComponent sut = new();
    ThrowingQuery query = new();

    Apply(sut, nameof(CheckedParameterComponent.Query), query);
    sut.TriggerShouldRender().ShouldBeTrue();

    ThrowingQuery replacement = new();
    Apply(sut, nameof(CheckedParameterComponent.Query), replacement);
    sut.TriggerShouldRender().ShouldBeTrue();
    sut.RenderReasonDetail.ShouldBe("Parameter 'Query' changed");

    Apply(sut, nameof(CheckedParameterComponent.Query), replacement);
    sut.TriggerShouldRender().ShouldBeFalse();
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

  private static CheckedParameterComponent ApplyReplacement(string parameterName, object current, object incoming)
  {
    CheckedParameterComponent sut = new();
    Apply(sut, parameterName, current);
    sut.TriggerShouldRender().ShouldBeTrue();
    sut.RenderReason.ShouldBe(TimeWarpStateComponent.RenderReasonCategory.ParameterChanged);

    Apply(sut, parameterName, incoming);
    return sut;
  }

  private static void AssertEqualValueDoesNotReportAChange(string parameterName, object value, object different)
  {
    CheckedParameterComponent sut = new();
    Apply(sut, parameterName, value);
    sut.TriggerShouldRender().ShouldBeTrue();
    sut.RenderReason.ShouldBe(TimeWarpStateComponent.RenderReasonCategory.ParameterChanged);
    string? detailAfterChange = sut.RenderReasonDetail;

    Apply(sut, parameterName, value);
    sut.TriggerShouldRender().ShouldBeFalse();
    sut.RenderReasonDetail.ShouldBe(detailAfterChange);

    Apply(sut, parameterName, different);
    sut.TriggerShouldRender().ShouldBeTrue();
    sut.RenderReasonDetail.ShouldBe($"Parameter '{parameterName}' changed");
  }

  private static void Apply(CheckedParameterComponent component, string parameterName, object? value)
  {
    InvokeIgnoringMissingRenderer
    (
      () => component.SetParametersAsync
      (
        ParameterView.FromDictionary
        (
          new Dictionary<string, object?>
          {
            [parameterName] = value
          }
        )
      )
    );
  }

  [NotTest]
  private sealed class ThrowingQuery : IQueryable
  {
    public Type ElementType => typeof(string);

    public Expression Expression => throw new QueryEnumeratedException();

    public IQueryProvider Provider => throw new QueryEnumeratedException();

    public System.Collections.IEnumerator GetEnumerator() => throw new QueryEnumeratedException();
  }

  [NotTest]
  private sealed class QueryEnumeratedException : Exception
  {
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

[NotTest]
public class CheckedParameterComponent : TestTimeWarpStateComponent
{
  [Parameter] public List<SampleModel>? Models { get; set; }

  [Parameter] public List<int>? Numbers { get; set; }

  [Parameter] public IQueryable? Query { get; set; }

  [Parameter] public DateTime Timestamp { get; set; }

  [Parameter] public Guid Identifier { get; set; }

  [Parameter] public decimal Amount { get; set; }

  [Parameter] public decimal? NullableAmount { get; set; }

  [Parameter] public SampleKind Kind { get; set; }

  [Parameter] public SamplePoint Point { get; set; }

  public bool TriggerShouldRender() => ShouldRender();

  protected override bool CheckPrimitiveParameterChanged(object? currentValue, object? newValue)
  {
    return base.CheckPrimitiveParameterChanged(currentValue, newValue);
  }
}

public enum SampleKind
{
  None = 0,
  Some = 1,
  Other = 2
}

public struct SamplePoint
{
  public int X { get; init; }

  public int Y { get; init; }
}
