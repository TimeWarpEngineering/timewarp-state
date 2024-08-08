namespace TimeWarp.State;

/// <summary>
///   A non required Base Class that injects Mediator and Store.
///   And exposes StateHasChanged
/// </summary>
/// <remarks>Implements ITimeWarpStateComponent by Injecting</remarks>
public partial class TimeWarpStateComponent : ComponentBase, IDisposable, ITimeWarpStateComponent
{
  [Inject] private IStore Store { get; set; } = null!;
  [Inject] private ILogger<TimeWarpStateComponent> Logger { get; set; } = null!;

  /// <summary>
  ///   Maintains all components that subscribe to a State.
  ///   Is updated by using the GetState method
  /// </summary>
  [Inject] private Subscriptions Subscriptions { get; set; } = null!;
  [Inject] protected IMediator Mediator { get; set; } = null!;

  protected CancellationTokenSource CancellationTokenSource { get; } = new();
  protected CancellationToken CancellationToken => CancellationTokenSource.Token;
  
  private static readonly ConcurrentDictionary<string, int> InstanceCounts = new();
  /// <summary>
  ///   A generated unique Id based on the Class name and number of times they have been created
  /// </summary>
  public string Id { get; }

  /// <summary>
  ///   Allows for the Assigning of a value one can use to select an element during automated testing.
  /// </summary>
  [Parameter] public string? TestId { get; set; }

  public TimeWarpStateComponent()
  {
    string name = GetType().Name;
    int count = InstanceCounts.AddOrUpdate(name, 1, updateValueFactory: (_, value) => value + 1);
    Id = $"{name}-{count}";
  }

  public virtual void Dispose()
  {
    Logger.LogDebug(EventIds.TimeWarpStateComponent_Disposing, "{Id}: Disposing, removing subscriptions. Total renders: {RenderCount}", Id, RenderCount);
    Subscriptions.Remove(this);
    RenderCounts.TryRemove(Id, out _);

    // Cancel and dispose the CancellationTokenSource
    if (!CancellationTokenSource.IsCancellationRequested)
    {
      CancellationTokenSource.Cancel();
    }
    CancellationTokenSource.Dispose();

    GC.SuppressFinalize(this);
  }

  /// <summary>
  ///   Place a Subscription for the calling component
  ///   And returns the requested state 
  /// </summary>
  /// <param name="placeSubscription"></param>
  /// <typeparam name="TState"></typeparam>
  /// <returns></returns>
  protected TState GetState<TState>(bool placeSubscription = true) where TState : IState
  {
    Type stateType = typeof(TState);
    if (placeSubscription) Subscriptions.Add(stateType, this);
    return Store.GetState<TState>();
  }

  /// <summary>
  /// Allows for the removal of a state and its previous state.
  /// </summary>
  /// <typeparam name="TState"></typeparam>
  /// <remarks>Use this in Dispose to free up memory</remarks>
  protected void RemoveState<TState>() where TState : IState
  {
    Store.RemoveState<TState>();
  }
  
  protected override bool ShouldRender()
  {
    // Determine render trigger:
    // 1. Event: If neither ShouldReRender, SetParametersAsync, nor ReRender (StateHasChanged) was called,
    //    it's likely an event-triggered render that directly called Blazor's StateHasChanged.
    // 2. Parameter change: Detected when SetParametersAsync was called, setting ParameterTriggered.
    // 3. Subscription update: When a subscribed state changes, setting SubscriptionTriggered.
    // 4. Forced render: Explicit request to re-render, setting ForceRender.
    // 5. Manual StateHasChanged: Detected when ReRender was called, typically from custom logic.
    
    bool shouldRender = false;

    bool eventTriggered =
      !SetParametersAsyncWasCalled && // Came from Parent
      !ShouldReRenderWasCalled && // Came from State Subscription
      !ReRenderWasCalled; // Could have been called from custom logic

    if (eventTriggered)
    {
      RenderReason = RenderReasonCategory.Event;
      shouldRender = true;
    }
    else if (ParameterTriggered)
    {
      RenderReason = RenderReasonCategory.ParameterChanged;
      shouldRender = true;
    }
    else if (SubscriptionTriggered)
    {
      RenderReason = RenderReasonCategory.Subscription;
      shouldRender = true;
    }
    else if (ReRenderWasCalled)
    {
      RenderReason = RenderReasonCategory.Forced;
      shouldRender = true;
    }
    else if (StateHasChangedWasCalled)
    {
      RenderReason = RenderReasonCategory.StateHasChanged;
      shouldRender = true;
    }

    Logger.LogTrace
    (
      EventIds.TimeWarpStateComponent_Disposing,
      "ShouldRender triggered: {ComponentId} {RenderDetails}",
      Id,
      new
      {
        EventTriggered = eventTriggered,
        SetParametersAsyncWasCalled,
        ShouldReRenderWasCalled,
        ReRenderWasCalled,
        ParameterTriggered,
        SubscriptionTriggered,
        StateHasChangedWasCalled,
        RenderReason,
        ShouldRender = shouldRender
      }
    );
    
    // Reset flags for next render cycle
    ShouldReRenderWasCalled = false;
    SetParametersAsyncWasCalled = false;
    ReRenderWasCalled = false;
    ParameterTriggered = false;
    SubscriptionTriggered = false;

    return shouldRender;
  }
}
