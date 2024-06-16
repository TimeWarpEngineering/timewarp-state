namespace TimeWarp.State;

/// <summary>
/// A non required Base Class that injects Mediator and Store.
/// And exposes StateHasChanged
/// </summary>
/// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
public abstract class TimeWarpStateInputComponent<TValue> : InputBase<TValue>, IDisposable, ITimeWarpStateComponent
{
  protected TimeWarpStateInputComponent()
  {
    string name = GetType().Name;
    InstanceCounter.IncrementCount<TValue>();
    int count = InstanceCounter.GetCount<TValue>();
    Id = $"{name}-{count}";
  }

  /// <summary>
  /// A generated unique Id based on the Class name and number of times they have been created
  /// </summary>
  public string Id { get; }

  /// <summary>
  /// Allows for the Assigning of a value one can use to select an element during automated testing.
  /// </summary>
  [Parameter] public string? TestId { get; set; }

  [Inject] public IMediator Mediator { get; set; } = null!;
  [Inject] public IStore Store { get; set; } = null!;

  /// <summary>
  /// Maintains all components that subscribe to a State.
  /// Is updated by using the GetState method
  /// </summary>
  [Inject] public Subscriptions Subscriptions { get; set; } = null!;

  /// <summary>
  /// Exposes StateHasChanged
  /// </summary>
  public void ReRender() => base.InvokeAsync(StateHasChanged);

  /// <summary>
  /// Place a Subscription for the calling component
  /// And returns the requested state
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  protected T GetState<T>()
  {
    Type stateType = typeof(T);
    Subscriptions.Add(stateType, this);
    return Store.GetState<T>();
  }
  
  public void Dispose()
  {
    Subscriptions.Remove(this);
    GC.SuppressFinalize(this);
  }
}

public static class InstanceCounter
{
  private static readonly ConcurrentDictionary<Type, int> InstanceCounts = new();

  public static int GetCount<T>() => InstanceCounts.GetOrAdd(typeof(T), 0);

  public static void IncrementCount<T>() => InstanceCounts.AddOrUpdate(typeof(T), 1, (_, count) => count + 1);
}
