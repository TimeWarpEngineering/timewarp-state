namespace BlazorState;

/// <summary>
///   A non required Base Class that injects Mediator and Store.
///   And exposes StateHasChanged
/// </summary>
/// <remarks>Implements IBlazorStateComponent by Injecting</remarks>
public class BlazorStateComponent : ComponentBase, IDisposable, IBlazorStateComponent
{
  private const string IsPreRenderCompleteKey = "IsPreRenderComplete";
  private static readonly ConcurrentDictionary<string, int> s_InstanceCounts = new();
  private PersistingComponentStateSubscription PersistingComponentStateSubscription;

  public BlazorStateComponent()
  {
    string name = GetType().Name;
    int count = s_InstanceCounts.AddOrUpdate(name, 1, updateValueFactory: (aKey, aValue) => aValue + 1);

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
  [Inject] private RenderPhaseService RenderPhaseService { get; set; } = null!;
  [Inject] private PersistentComponentState PersistentComponentState { get; set; } = null!;
  [Inject] protected IMediator Mediator { get; set; } = null!;
  
  /// <summary>
  ///   Maintains all components that subscribe to a State.
  ///   Is updated by using the GetState method
  /// </summary>
  [Inject] public Subscriptions Subscriptions { get; set; }
  
  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering { get; private set; } = true;

  private CurrentRenderMode Mode => GetCurrentRenderMode();

  private CurrentRenderMode GetCurrentRenderMode()
  {
    if (OperatingSystem.IsBrowser())
    {
      return CurrentRenderMode.Wasm;
    }
    else if (IsPreRendering)
    {
      return CurrentRenderMode.PreRendering;
    }
    else
    {
      return CurrentRenderMode.Server;
    }
  }

  protected string RenderModeString => Mode.ToString();
  
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

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();
    if (OperatingSystem.IsBrowser())
    {
      IsPreRendering = false;
    }
    else
    {
      PersistingComponentStateSubscription = PersistentComponentState.RegisterOnPersisting(Persist);

      bool foundInState =
        PersistentComponentState.TryTakeFromJson(IsPreRenderCompleteKey, out bool _);

      if (foundInState) { IsPreRendering = false; }
    }
  }

  private Task Persist()
  {
    // This will fire only once just prior to transitioning to wasm. 
    PersistentComponentState.PersistAsJson("IsPreRenderComplete", true);
    return Task.CompletedTask;
  }

  protected override void OnAfterRender(bool firstRender)
  {
    base.OnAfterRender(firstRender);
    if (firstRender)
    {
      RenderPhaseService.SetInteractive();
    }
  }
  
  public virtual void Dispose()
  {
    PersistingComponentStateSubscription.Dispose();
    Subscriptions.Remove(this);
    GC.SuppressFinalize(this);
  }
}

public enum CurrentRenderMode
{
  Wasm,
  PreRendering,
  Server
}
