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
  
  /// <summary>
  ///   Maintains all components that subscribe to a State.
  ///   Is updated by using the GetState method
  /// </summary>
  [Inject] private Subscriptions Subscriptions { get; set; } = null!;
  [Inject] protected IMediator Mediator { get; set; } = null!;

  private static readonly ConcurrentDictionary<string, int> InstanceCounts = new();
  private static readonly ConcurrentDictionary<string, int> RenderCounts = new();
  private static readonly ConcurrentDictionary<Type, string> ConfiguredRenderModeCache = new();
  private static readonly ConcurrentDictionary<Type, bool> TypeRenderAttributeCache = new();

  /// <summary>
  ///   A generated unique Id based on the Class name and number of times they have been created
  /// </summary>
  public string Id { get; }
  
  /// <summary>
  ///   Allows for the Assigning of a value one can use to select an element during automated testing.
  /// </summary>
  [Parameter] public string? TestId { get; set; }
  
  private readonly ConcurrentDictionary <Type, Func<bool>> RenderTriggers = new();
  private readonly ConcurrentDictionary<(Type StateType, string PropertyName), Func<object, object, bool>> CompiledPropertyComparisons = new();
  
  /// <summary>
  /// Set this to true if something in the component has changed that requires a re-render.
  /// </summary>
  protected bool NeedsRerender;
  private bool HasRendered;
  private bool UsesRenderMode;
  
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
      return "None";// Adjust as needed for your default case.
    });

  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering => GetCurrentRenderMode() == State.CurrentRenderMode.PreRendering;
  protected string CurrentRenderMode => GetCurrentRenderMode().ToString();
  public int RenderCount => RenderCounts.GetValueOrDefault(Id, 0);
  
  public TimeWarpStateComponent()
  {
    string name = GetType().Name;
    int count = InstanceCounts.AddOrUpdate(name, 1, updateValueFactory: (_, value) => value + 1);
    Id = $"{name}-{count}";
  }
  
  protected override void OnInitialized()
  {
    Logger.LogDebug(EventIds.TimeWarpStateComponent_Constructed,"TimeWarpStateComponent created: {Id}", Id);    
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
  
  /// <inheritdoc />
  protected override bool ShouldRender()
  {
    // If there are no RenderTriggers, default to true (standard Blazor behavior)
    if (RenderTriggers.Count == 0)
      return true;

    // If there are RenderTriggers, use NeedsRerender flag
    bool result = NeedsRerender;
    NeedsRerender = false;
    return result;
  }

  /// <inheritdoc />
  public virtual bool ShouldReRender(Type stateType)
  {
    ArgumentNullException.ThrowIfNull(stateType);

    NeedsRerender = RenderTriggers.TryGetValue(stateType, out Func<bool>? check) && check();
    return NeedsRerender;
  }
  
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
  
  /// <summary>
  /// Registers a render trigger for a specific state type.
  /// </summary>
  /// <typeparam name="T">The type of state to check. Must be a reference type.</typeparam>
  /// <param name="triggerCondition">A function that takes the previous state of type T and returns a boolean indicating whether a re-render is needed.</param>
  /// <remarks>
  /// This method adds a new entry to the RenderTriggers dictionary. The key is the Type of T, 
  /// and the value is a function that will be called to determine if a re-render is necessary 
  /// when the state of type T changes. The function should compare the previous state 
  /// (passed as an argument) with the current state (which should be accessible within the component).
  /// </remarks>
  /// <example>
  /// <code>
  /// RegisterRenderTrigger&lt;UserState&gt;(previousUserState => UserState.Name != previousUserState.Name);
  /// </code>
  /// </example>
  protected void RegisterRenderTrigger<T>(Func<T, bool> triggerCondition) where T : class
  {
    RenderTriggers[typeof(T)] = () => ShouldReRender(typeof(T), triggerCondition);
  }
  
  /// <summary>
    /// Registers a render trigger for a specific state type using a property selector expression.
    /// </summary>
    /// <typeparam name="TState">The type of state to monitor. Must be a reference type.</typeparam>
    /// <param name="propertySelector">An expression that selects the property to monitor for changes.</param>
    /// <remarks>
    /// This method creates a render trigger that compares a specific property of the state object.
    /// It uses expression trees to build an efficient comparison function, which is cached for subsequent use.
    /// The component will re-render when the selected property's value changes.
    /// </remarks>
    /// <example>
    /// <code>
    /// RegisterRenderTrigger&lt;CounterState&gt;(s => s.Count);
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">Thrown when the propertySelector is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the propertySelector does not represent a simple property access.</exception>
    protected void RegisterRenderTrigger<TState>(Expression<Func<TState, object>> propertySelector) 
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(propertySelector);

        MemberExpression? memberExpression = propertySelector.Body as MemberExpression 
            ?? ((UnaryExpression)propertySelector.Body).Operand as MemberExpression;
        
        if (memberExpression == null)
        {
            throw new ArgumentException("Property selector must be a simple property access expression.", nameof(propertySelector));
        }

        var property = (PropertyInfo)memberExpression.Member;

        Func<object, object, bool> comparisonFunc = CompiledPropertyComparisons.GetOrAdd((typeof(TState), property.Name), _ =>
        {
            ParameterExpression previousParam = Expression.Parameter(typeof(object), "previous");
            ParameterExpression currentParam = Expression.Parameter(typeof(object), "current");

            BinaryExpression notEqualExpression = Expression.NotEqual(
                Expression.Property(Expression.Convert(previousParam, typeof(TState)), property),
                Expression.Property(Expression.Convert(currentParam, typeof(TState)), property)
            );

            return Expression.Lambda<Func<object, object, bool>>(notEqualExpression, previousParam, currentParam).Compile();
        });

        RenderTriggers[typeof(TState)] = () => 
        {
            TState? previousState = GetPreviousState<TState>();
            TState currentState = GetState<TState>(false);
            return previousState == null || comparisonFunc(previousState, currentState);
        };
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
  
  private CurrentRenderMode GetCurrentRenderMode()
  {
    UsesRenderMode = true;
    if (OperatingSystem.IsBrowser()) return State.CurrentRenderMode.Wasm;

    if (HasRendered) return State.CurrentRenderMode.Server;

    bool hasRenderAttribute = TypeRenderAttributeCache.GetOrAdd(this.GetType(), type =>
      type.GetCustomAttributes(true)
        .Any(attr => attr.GetType().Name.Contains("PrivateComponentRenderModeAttribute")));

    return hasRenderAttribute ? State.CurrentRenderMode.PreRendering : State.CurrentRenderMode.Static;
  }


  /// <summary>
  ///   Exposes StateHasChanged
  /// </summary>
  public void ReRender() => InvokeAsync(StateHasChanged);
  
  private void IncrementRenderCount()
  {
    RenderCounts.AddOrUpdate(Id, 1, (_, count) => count + 1);
  }
}

public enum CurrentRenderMode
{
  Static,
  PreRendering,
  Server,
  Wasm,
}
