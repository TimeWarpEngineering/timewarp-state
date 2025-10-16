namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  public int RenderCount => RenderCounts.GetValueOrDefault(Id, 0);
  
  private static readonly ConcurrentDictionary<string, int> RenderCounts = new();

  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering => !RendererInfo.IsInteractive;
  
  protected override void OnAfterRender(bool firstRender)
  {
    base.OnAfterRender(firstRender);
    IncrementRenderCount();
    int renderCount = RenderCounts[Id];
    
    Logger.LogTrace
    (
      EventIds.TimeWarpStateComponent_OnAfterRender, 
      "{ComponentId}: Rendered, {Details} ",
      Id,
      new
      {
        RenderCount = renderCount,
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

  private void IncrementRenderCount()
  {
    RenderCounts.AddOrUpdate(Id, 1, (_, count) => count + 1);
  }
}
