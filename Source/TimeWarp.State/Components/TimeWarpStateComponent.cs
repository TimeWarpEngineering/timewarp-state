namespace TimeWarp.State;

/// <summary>
///   A non required Base Class that injects Mediator and Store.
///   And exposes StateHasChanged
/// </summary>
/// <remarks>Implements ITimeWarpStateComponent by Injecting</remarks>
public class TimeWarpStateComponent : ComponentBase, IDisposable, ITimeWarpStateComponent
{
  [Inject] private IStore Store { get; set; } = null!;
  [Inject] private ILogger<TimeWarpStateComponent> Logger { get; set; } = null!;
  [Inject] protected IMediator Mediator { get; set; } = null!;
  
  /// <summary>
  ///   Maintains all components that subscribe to a State.
  ///   Is updated by using the GetState method
  /// </summary>
  [Inject] public Subscriptions Subscriptions { get; set; } = null!;
  
  
  /// <summary>
  ///   Allows for the Assigning of a value one can use to select an element during automated testing.
  /// </summary>
  [Parameter] public string? TestId { get; set; }
  
  private static readonly ConcurrentDictionary<string, int> InstanceCounts = new();

  private static readonly ConcurrentDictionary<Type, string> ConfiguredRenderModeCache = new();

  protected string ConfiguredRenderMode =>
    ConfiguredRenderModeCache.GetOrAdd(this.GetType(), type =>
    {
      // Use reflection to get all attributes on the current component type.
      object[] attributes = type.GetCustomAttributes(true);

      foreach (object attribute in attributes)
      {
        // Check if the type name of the attribute contains the expected name.
        Type attributeType = attribute.GetType();
        if (attributeType.Name.Contains("PrivateComponentRenderModeAttribute"))
        {
          // Try to get the 'Mode' property value of the attribute.
          PropertyInfo? modeProperty = attributeType.GetProperty("Mode");
          if (modeProperty != null)
          {
            // Use dynamic to bypass compile-time type checking
            dynamic? modeValue = modeProperty.GetValue(attribute);
            // Return the type name of the Mode property's value.
            return modeValue == null ? "None" : modeValue.GetType().Name;
          }
        }
      }

      // If no matching attribute is found, return a default identifier.
      return "None"; // Adjust as needed for your default case.
    });

  private bool HasRendered;
  private bool UsesRenderMode;

  public TimeWarpStateComponent()
  {
    string name = GetType().Name;
    int count = InstanceCounts.AddOrUpdate(name, 1, updateValueFactory: (_, value) => value + 1);

    Id = $"{name}-{count}";
  }
  
  /// <summary>
  ///   A generated unique Id based on the Class name and number of times they have been created
  /// </summary>
  public string Id { get; }
  
  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering => GetCurrentRenderMode() == State.CurrentRenderMode.PreRendering;

  private static readonly ConcurrentDictionary<Type, bool> TypeRenderAttributeCache = new();

  private CurrentRenderMode GetCurrentRenderMode()
  {
    UsesRenderMode = true;
    if (OperatingSystem.IsBrowser()) return State.CurrentRenderMode.Wasm;
    
    if (HasRendered) return State.CurrentRenderMode.Server;
    
    bool hasRenderAttribute = TypeRenderAttributeCache.GetOrAdd(this.GetType(), type =>
      type.GetCustomAttributes(true)
        .Any(attr => attr.GetType().Name.Contains("PrivateComponentRenderModeAttribute")));

    return hasRenderAttribute
      ? State.CurrentRenderMode.PreRendering
      : State.CurrentRenderMode.Static;
  }

  protected string CurrentRenderMode => GetCurrentRenderMode().ToString();
  
  /// <summary>
  ///   Exposes StateHasChanged
  /// </summary>
  public void ReRender() => InvokeAsync(StateHasChanged);
  
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

  protected override void OnAfterRender(bool firstRender)
  {
    base.OnAfterRender(firstRender);
    if (!firstRender) return;
    HasRendered = true;
    if (UsesRenderMode)
    {
      StateHasChanged();
    }
  }

  /// <inheritdoc />
  public virtual bool ShouldReRender(Type stateType) => true;
  

  /// <summary>
  /// Determines whether the component should re-render based on changes in a specific state type.
  /// </summary>
  /// <typeparam name="T">The type of state to check.</typeparam>
  /// <param name="stateType">The type of state that has changed.</param>
  /// <param name="condition">A function that evaluates given the previous state and returns true if a re-render is needed.</param>
  /// <returns>True if the component should re-render; otherwise, false.</returns>
  /// <remarks>
  /// This method checks if the changed state type matches the generic type parameter T.
  /// If it matches, it retrieves the previous state and applies the provided condition.
  /// The component will re-render if the condition returns true given the previous state.
  /// </remarks>
  protected bool ShouldReRender<T>(Type stateType, Func<T, bool> condition) where T : class
  {
    if (stateType != typeof(T)) return false;
    T? previousState = GetPreviousState<T>();
    if (previousState == null) return true;  
    bool result = condition(previousState);
    Logger.LogDebug(EventIds.TimeWarpStateComponent_ShouldReRender, "ShouldReRender ComponentType: {ComponentId} StateType: {StateType} Result: {Result}", Id, stateType.FullName, result);
    
    return result;
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
