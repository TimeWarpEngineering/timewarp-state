#region Purpose
// TimeWarpStateComponent's render bookkeeping: RenderCount, IsPreRendering and the per-render reset of lifecycle flags.
#endregion

#region Design
// OnAfterRender counts and trace-logs each render, then clears every tracking flag so the next cycle's render
// reason starts clean. IsPreRendering is derived from RendererInfo.IsInteractive.
#endregion

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
