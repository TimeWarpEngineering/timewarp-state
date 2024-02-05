namespace BlazorState;

/// <summary>
///   A non required Base Class that injects Mediator and Store.
///   And exposes StateHasChanged
/// </summary>
/// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
public class BlazorStateComponent : ComponentBase, IDisposable, IBlazorStateComponent
{
  private static readonly ConcurrentDictionary<string, int> s_InstanceCounts = new();
  
  private bool HasRendered = false;

  public BlazorStateComponent()
  {
    string name = GetType().Name;
    int count = s_InstanceCounts.AddOrUpdate(name, 1, updateValueFactory: (_, value) => value + 1);

    Id = $"{name}-{count}";
  }

  /// <summary>
  ///   Allows for the Assigning of a value one can use to select an element during automated testing.
  /// </summary>
  [Parameter] public string TestId { get; set; }
  
  /// <summary>
  ///   A generated unique Id based on the Class name and number of times they have been created
  /// </summary>
  public string Id { get; }
  
  [Inject] private IStore Store { get; set; } = null!;
  [Inject] protected IMediator Mediator { get; set; } = null!;
  
  /// <summary>
  ///   Maintains all components that subscribe to a State.
  ///   Is updated by using the GetState method
  /// </summary>
  [Inject] public Subscriptions Subscriptions { get; set; }
  
  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering => GetCurrentRenderMode() == CurrentRenderMode.PreRendering;

  private static readonly ConcurrentDictionary<Type, bool> s_TypeRenderAttributeCache = new();

  private CurrentRenderMode GetCurrentRenderMode()
  {
    if (OperatingSystem.IsBrowser())
    {
      return CurrentRenderMode.Wasm;
    }
    else if (!HasRendered)
    {
      bool hasRenderAttribute = s_TypeRenderAttributeCache.GetOrAdd(this.GetType(), type =>
        type.GetCustomAttributes(true)
          .Any(attr => attr.GetType().Name.Contains("PrivateComponentRenderModeAttribute")));

      return hasRenderAttribute
        ? CurrentRenderMode.PreRendering
        : CurrentRenderMode.Static;
    }
    else
    {
      return CurrentRenderMode.Server;
    }
  }

  protected string RenderModeString => GetCurrentRenderMode().ToString();
  
  /// <summary>
  ///   Exposes StateHasChanged
  /// </summary>
  public void ReRender() => InvokeAsync(StateHasChanged);

  /// <summary>
  ///   Place a Subscription for the calling component
  ///   And returns the requested state
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  protected T GetState<T>()
  {
    Type stateType = typeof(T);
    Subscriptions.Add(stateType, this);
    return Store.GetState<T>();
  }
  
  protected override void OnAfterRender(bool firstRender)
  {
    base.OnAfterRender(firstRender);
    if (!firstRender) return;
    HasRendered = true;
    StateHasChanged();
  }
  
  public virtual void Dispose()
  {
    Subscriptions.Remove(this);
    GC.SuppressFinalize(this);
  }
}

public enum CurrentRenderMode
{
  Static,
  PreRendering,
  Server,
  Wasm,
}
