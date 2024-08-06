namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  private readonly ConcurrentDictionary<Type, Func<bool>> RenderTriggers = new();

  private TState? GetPreviousState<TState>() where TState:IState => Store.GetPreviousState<TState>();
  private readonly ConcurrentDictionary<(Type StateType, string PropertyName), Func<object, object, bool>> CompiledPropertyComparisons = new();

  /// <summary>
  /// Set this to true if something in the component has changed that requires a re-render.
  /// </summary>
  private bool NeedsRerender;

  /// <summary>
  /// Triggers a re-render of the component by setting the NeedsRerender flag
  /// and invoking the base StateHasChanged method asynchronously.
  /// </summary>
  /// <remarks>
  /// This method provides a public way to force a re-render of the component
  /// from external code or in response to specific events or conditions.
  /// </remarks>
  public void ReRender()
  {
    NeedsRerender = true;
    InvokeAsync(base.StateHasChanged);
  }

  /// <summary>
  /// Overrides the base StateHasChanged method to control the re-rendering behavior
  /// of the component.
  /// </summary>
  /// <remarks>
  /// This method is called by the Blazor framework whenever the component's state
  /// has changed. By overriding it and redirecting the call to ReRender(), we ensure
  /// that our custom re-rendering logic is executed.
  /// </remarks>
  protected new void StateHasChanged()
  {
    ReRender();
  }

  /// <inheritdoc />
  protected override bool ShouldRender()
  {
    // If there are no RenderTriggers or ParameterComparisons, default to true (standard Blazor behavior)
    // If there are RenderTriggers or ParameterComparisons, use NeedsRerender flag
    bool result = (RenderTriggers.IsEmpty && ParameterComparisons.IsEmpty) || NeedsRerender; 
    NeedsRerender = false;
    return result;
  }

  /// <inheritdoc />
  public virtual bool ShouldReRender(Type stateType)
  {
    ArgumentNullException.ThrowIfNull(stateType);

    NeedsRerender = !RenderTriggers.TryGetValue(stateType, out Func<bool>? check) || check();

    return NeedsRerender;
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
    RenderReasonCategory = RenderReasonCategory.Subscription;
    if (stateType != typeof(TState)) return false;
    TState? previousState = GetPreviousState<TState>();
    if (previousState == null) return true;
    bool result = condition(previousState);
    if (result) RenderReasonCategory = RenderReasonCategory.RenderTrigger;
    Logger.LogDebug
    (
      EventIds.TimeWarpStateComponent_ShouldReRender,
      "{Id}: ShouldReRender,  StateType: {StateType} Result: {Result}",
      Id,
      stateType.FullName,
      result
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
