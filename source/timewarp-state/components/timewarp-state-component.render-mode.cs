namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  public int RenderCount { get; private set; }

  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering => !RendererInfo.IsInteractive;
  
  protected override void OnAfterRender(bool firstRender)
  {
    base.OnAfterRender(firstRender);
    RenderCount++;

    Logger.LogTrace
    (
      EventIds.TimeWarpStateComponent_OnAfterRender, 
      "{ComponentId}: Rendered, {Details} ",
      Id,
      new
      {
        RenderCount,
        RenderReason,
        RenderReasonDetail,
        ShouldRenderWasCalledBy,
        SetParametersAsyncWasCalledBy,
      }
    );
    ResetLifeCycleProperties();
  }

  private void ResetLifeCycleProperties()
  {
    ParameterTriggered = false;
    ReRenderWasCalled = false;
    RenderReason = RenderReasonCategory.None;
    RenderReasonDetail = null;
    SetParametersAsyncWasCalled = false;
    SetParametersAsyncWasCalledBy = null;
    ShouldReRenderWasCalled = false;
    ShouldRenderWasCalledBy = null;
    StateHasChangedWasCalled = false;
    StateHasChangedWasCalledBy = null;
    SubscriptionTriggered = false;
  }
}
