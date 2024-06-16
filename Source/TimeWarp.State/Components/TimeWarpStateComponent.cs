namespace TimeWarp.State;

/// <summary>
///   A non required Base Class that injects Mediator and Store.
///   And exposes StateHasChanged
/// </summary>
/// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
public class TimeWarpStateComponent : ComponentBase, IDisposable, ITimeWarpStateComponent
{
  [Inject] private IStore Store { get; set; } = null!;
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
    if (OperatingSystem.IsBrowser())
    {
      return State.CurrentRenderMode.Wasm;
    }
    else if (!HasRendered)
    {
      bool hasRenderAttribute = TypeRenderAttributeCache.GetOrAdd(this.GetType(), type =>
        type.GetCustomAttributes(true)
          .Any(attr => attr.GetType().Name.Contains("PrivateComponentRenderModeAttribute")));

      return hasRenderAttribute
        ? State.CurrentRenderMode.PreRendering
        : State.CurrentRenderMode.Static;
    }
    else
    {
      return State.CurrentRenderMode.Server;
    }
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
