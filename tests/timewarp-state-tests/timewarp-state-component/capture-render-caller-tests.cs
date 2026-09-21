namespace CaptureRenderCaller;

public class Should_
{
  public static void Options_Default_CaptureRenderCaller_Is_False()
  {
    TimeWarpStateOptions timeWarpStateOptions = new(new ServiceCollection());
    timeWarpStateOptions.CaptureRenderCaller.ShouldBeFalse();
  }

  public static void Flag_Off_Leaves_Caller_Properties_Null()
  {
    TestRenderCallerComponent sut = new(captureRenderCaller: false);

    sut.TriggerShouldRender();
    InvokeIgnoringMissingRenderer(sut.TriggerStateHasChanged);
    InvokeIgnoringMissingRenderer(sut.TriggerSetParametersAsync);

    sut.ShouldRenderWasCalledBy.ShouldBeNull();
    sut.StateHasChangedWasCalledBy.ShouldBeNull();
    sut.SetParametersAsyncWasCalledBy.ShouldBeNull();
  }

  public static void Flag_On_Captures_Class_Method_From_ShouldRender()
  {
    TestRenderCallerComponent sut = new(captureRenderCaller: true);

    sut.TriggerShouldRender();

    sut.ShouldRenderWasCalledBy.ShouldBe(
      $"{nameof(TestRenderCallerComponent)}.{nameof(TestRenderCallerComponent.TriggerShouldRender)}");
  }

  public static void Flag_On_Captures_Class_Method_From_StateHasChanged()
  {
    TestRenderCallerComponent sut = new(captureRenderCaller: true);

    InvokeIgnoringMissingRenderer(sut.TriggerStateHasChanged);

    sut.StateHasChangedWasCalledBy.ShouldBe(
      $"{nameof(TestRenderCallerComponent)}.{nameof(TestRenderCallerComponent.TriggerStateHasChanged)}");
  }

  public static void Flag_On_Captures_Class_Method_From_SetParametersAsync()
  {
    TestRenderCallerComponent sut = new(captureRenderCaller: true);

    InvokeIgnoringMissingRenderer(sut.TriggerSetParametersAsync);

    sut.SetParametersAsyncWasCalledBy.ShouldBe(
      $"{nameof(TestRenderCallerComponent)}.{nameof(TestRenderCallerComponent.TriggerSetParametersAsync)}");
  }

  private static void InvokeIgnoringMissingRenderer(Action action)
  {
    try
    {
      action();
    }
    catch (InvalidOperationException)
    {
      // No renderer in this unit test; capture runs before InvokeAsync.
    }
  }

  private static void InvokeIgnoringMissingRenderer(Func<Task> action)
  {
    try
    {
      action().GetAwaiter().GetResult();
    }
    catch (InvalidOperationException)
    {
      // No renderer in this unit test; capture runs before base.SetParametersAsync.
    }
  }
}

[NotTest]
public class TestRenderCallerComponent : TimeWarpStateComponent
{
  public TestRenderCallerComponent(bool captureRenderCaller)
  {
    TimeWarpStateOptions timeWarpStateOptions = new(new ServiceCollection())
    {
      CaptureRenderCaller = captureRenderCaller
    };
    SetPrivateInjectedProperty("TimeWarpStateOptions", timeWarpStateOptions);
    SetPrivateInjectedProperty("Logger", NullLogger<TimeWarpStateComponent>.Instance);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  public bool TriggerShouldRender() => ShouldRender();

  [MethodImpl(MethodImplOptions.NoInlining)]
  public void TriggerStateHasChanged() => StateHasChanged();

  [MethodImpl(MethodImplOptions.NoInlining)]
  public Task TriggerSetParametersAsync() => SetParametersAsync(ParameterView.Empty);

  protected override bool HandleUnregisteredParameter(ParameterValue parameter) => false;

  private void SetPrivateInjectedProperty(string propertyName, object value)
  {
    PropertyInfo? propertyInfo = typeof(TimeWarpStateComponent).GetProperty(
      propertyName,
      BindingFlags.NonPublic | BindingFlags.Instance);
    propertyInfo.ShouldNotBeNull();
    propertyInfo!.SetValue(this, value);
  }
}
