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

  protected override void OnInitialized()
  {
    Logger.LogDebug(EventIds.TimeWarpStateComponent_Constructed, "{Id}: created", Id);
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
  
  /// <inheritdoc />
  protected override bool ShouldRender()
  {
    // If there are no RenderTriggers or ParameterComparisons, default to true (standard Blazor behavior)
    // If there are RenderTriggers or ParameterComparisons, use NeedsRerender flag
    bool result = (RenderTriggers.IsEmpty && ParameterTriggers.IsEmpty) || NeedsRerender; 
    NeedsRerender = false;
    return result;
  }
}
