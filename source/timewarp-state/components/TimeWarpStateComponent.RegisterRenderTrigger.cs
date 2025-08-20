namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  private readonly ConcurrentDictionary<Type, Func<bool>> RenderTriggers = new();

  private TState? GetPreviousState<TState>() where TState:IState => Store.GetPreviousState<TState>();
  private readonly ConcurrentDictionary<(Type StateType, string PropertyName), Func<object, object, bool>> CompiledPropertyComparisons = new();

  private bool ReRenderWasCalled;
  private bool SubscriptionTriggered;
  private bool ShouldReRenderWasCalled;
  private bool StateHasChangedWasCalled;
  public string? StateHasChangedWasCalledBy  { get; private set; }

  /// <summary>
  /// Triggers a re-render of the component by setting the ReRenderWasCalled flag
  /// and invoking the base StateHasChanged method asynchronously.
  /// </summary>
  /// <remarks>
  /// This is the preferred method to force a re-render of the component
  /// from external code or in response to specific events or conditions.
  /// Use this method when you want to trigger a re-render due to a state change that
  /// Blazor might not automatically detect, such as changes to fields or properties
  /// that are not marked as parameters.
  /// </remarks>
  public void ReRender()
  {
    // Subscriptions will call this if SubscriptionTriggered was true.
    // But the user could call it directly. Or indirectly via StateHasChanged.
    ReRenderWasCalled = true;
    InvokeAsync(base.StateHasChanged);
  }

  /// <summary>
  /// Notifies the component that its state has changed and triggers a re-render.
  /// </summary>
  /// <remarks>
  /// This method overrides the base Blazor StateHasChanged method to provide additional
  /// tracking of when StateHasChanged is called directly. It sets the StateHasChangedWasCalled
  /// flag and then invokes the base implementation asynchronously.
  /// 
  /// Note: This method is called automatically by Blazor in many scenarios, such as
  /// after event handlers complete. For manual re-render requests, prefer using the ReRender()
  /// method instead of calling StateHasChanged() directly.
  /// </remarks>
  protected new void StateHasChanged()
  {
    StackFrame? frame = new StackTrace().GetFrame(1);
    MethodBase? method = frame?.GetMethod();
    string className = method?.DeclaringType?.Name ?? "Unknown";
    string methodName = method?.Name ?? "Unknown";
    StateHasChangedWasCalledBy = $"{className}.{methodName}";
    StateHasChangedWasCalled = true;
    InvokeAsync(base.StateHasChanged);
  }

  /// <inheritdoc />
  public virtual bool ShouldReRender(Type stateType)
  {
    // If this method returns true, Subscriptions will call ReRender on this component right away.
    // Given that we do NOT need to store multiple reasons for a re-render
    ArgumentNullException.ThrowIfNull(stateType);

    ShouldReRenderWasCalled = true;
    bool triggerFound = RenderTriggers.TryGetValue(stateType, out Func<bool>? check);
    bool triggerResult = triggerFound && check!();
    SubscriptionTriggered = triggerResult || !triggerFound;

    return SubscriptionTriggered;
  }

  /// <summary>
  /// Determines whether the component should re-render based on changes in a specific state type.
  /// </summary>
  /// <typeparam name="TState">The type of state to check.</typeparam>
  /// <param name="stateType">The type of state that has changed.</param>
  /// <param name="condition">A function that evaluates given the previous state and returns true if a re-render is needed.</param>
  /// <returns>True if the component should re-render; otherwise, false.</returns>
  /// <remarks>
  /// This method checks if the changed state type matches the generic type parameter T.
  /// If it matches, it retrieves the previous state and applies the provided condition.
  /// The component will re-render if the condition returns true given the previous state.
  /// </remarks>
  protected bool ShouldReRender<TState>(Type stateType, Func<TState, bool> condition) where TState : IState
  {
    RenderReason = RenderReasonCategory.Subscription;
    if (stateType != typeof(TState)) return false;
    TState? previousState = GetPreviousState<TState>();
    if (previousState == null) return true;
    bool result = condition(previousState);
    if (result) RenderReason = RenderReasonCategory.RenderTrigger;
    Logger.LogDebug
    (
      EventIds.TimeWarpStateComponent_ShouldReRender
      ,"{ComponentId}: ShouldReRender {Details}"
      ,Id
      ,new
        {
          StateName = stateType.FullName
          ,Result = result
        }
    );

    return result;
  }
  
  /// <summary>
  /// Registers a render trigger for a specific state type.
  /// </summary>
  /// <typeparam name="TState">The type of state to check. Must be a reference type.</typeparam>
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
  protected void RegisterRenderTriggerCondition<TState>(Func<TState, bool> triggerCondition) where TState : IState
  {
    RenderTriggers[typeof(TState)] = () => ShouldReRender(typeof(TState), triggerCondition);
  }
  protected void RegisterRenderTrigger<TState>(Expression<Func<TState, object?>> propertySelector) where TState : IState
  {
    ArgumentNullException.ThrowIfNull(propertySelector);

    Func<TState, TState, bool> comparisonFunc = CreateComparisonFunc(propertySelector);

    RenderTriggers[typeof(TState)] = () =>
    {
      TState? previousState = GetPreviousState<TState>();
      TState currentState = GetState<TState>(false);
      return previousState == null || comparisonFunc(previousState, currentState);
    };
  }

  private static Func<TState, TState, bool> CreateComparisonFunc<TState>(Expression<Func<TState, object?>> propertySelector) where TState : IState
  {
    Func<TState, object?> compiledSelector = propertySelector.Compile();

    return (previous, current) =>
    {
      object? previousValue = compiledSelector(previous);
      object? currentValue = compiledSelector(current);

      return !Equals(previousValue, currentValue);
    };
  }
}
