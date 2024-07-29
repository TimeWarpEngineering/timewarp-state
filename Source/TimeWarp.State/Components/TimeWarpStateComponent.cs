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

  private static readonly ConcurrentDictionary<string, int> InstanceCounts = new();
  private static readonly ConcurrentDictionary<string, int> RenderCounts = new();
  /// <summary>
  ///   A generated unique Id based on the Class name and number of times they have been created
  /// </summary>
  public string Id { get; }

  /// <summary>
  ///   Allows for the Assigning of a value one can use to select an element during automated testing.
  /// </summary>
  [Parameter] public string? TestId { get; set; }

  public int RenderCount => RenderCounts.GetValueOrDefault(Id, 0);

  public TimeWarpStateComponent()
  {
    string name = GetType().Name;
    int count = InstanceCounts.AddOrUpdate(name, 1, updateValueFactory: (_, value) => value + 1);
    Id = $"{name}-{count}";
  }

  protected override void OnInitialized()
  {
    Logger.LogDebug(EventIds.TimeWarpStateComponent_Constructed, "TimeWarpStateComponent created: {Id}", Id);
  }

  protected override void OnAfterRender(bool firstRender)
  {
    base.OnAfterRender(firstRender);
    IncrementRenderCount();
    int renderCount = RenderCounts[Id];
    Logger.LogTrace(EventIds.TimeWarpStateComponent_RenderCount, "{Id}: Rendered, RenderCount: {RenderCount}", Id, renderCount);
    if (!firstRender) return;
    HasRendered = true;
    if (UsesRenderMode)
    {
      StateHasChanged();
    }
  }

  public virtual void Dispose()
  {
    Logger.LogDebug(EventIds.TimeWarpStateComponent_Disposing, "{Id}: Disposing, removing subscriptions. Total renders: {RenderCount}", Id, RenderCount);
    Subscriptions.Remove(this);
    RenderCounts.TryRemove(Id, out _);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  ///   Place a Subscription for the calling component
  ///   And returns the requested state 
  /// </summary>
  /// <param name="placeSubscription"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  protected T GetState<T>(bool placeSubscription = true)
  {
    Type stateType = typeof(T);
    if (placeSubscription) Subscriptions.Add(stateType, this);
    return Store.GetState<T>();
  }

  private T? GetPreviousState<T>() => Store.GetPreviousState<T>();

  /// <summary>
  ///   Exposes StateHasChanged
  /// </summary>
  public void ReRender() => InvokeAsync(StateHasChanged);

  private void IncrementRenderCount()
  {
    RenderCounts.AddOrUpdate(Id, 1, (_, count) => count + 1);
  }
}
