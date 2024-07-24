namespace TimeWarp.State;

public class Subscriptions
{
  private readonly ILogger Logger;

  private readonly List<Subscription> TimeWarpStateComponentReferencesList;

  public Subscriptions(ILogger<Subscriptions> logger)
  {
    Logger = logger;
    Logger.LogDebug(EventIds.Subscriptions_Initializing, "constructing");
    TimeWarpStateComponentReferencesList = new List<Subscription>();
  }

  public Subscriptions Add<T>(ITimeWarpStateComponent timeWarpStateComponent) where T : IState
  {
    Type type = typeof(T);
    return Add(type, timeWarpStateComponent);
  }

  public Subscriptions Add(Type type, ITimeWarpStateComponent timeWarpStateComponent)
  {

    // Add only once.
    if (!TimeWarpStateComponentReferencesList.Any(subscription => subscription.StateType == type && subscription.ComponentId == timeWarpStateComponent.Id))
    {
      Logger.LogDebug
      (
        EventIds.Subscriptions_Adding,
        "adding subscription for Id:{id} Type.Name:{type_name}",
        timeWarpStateComponent.Id,
        type.Name
      );

      var subscription = new Subscription(
        type,
        timeWarpStateComponent.Id,
        new WeakReference<ITimeWarpStateComponent>(timeWarpStateComponent));

      TimeWarpStateComponentReferencesList.Add(subscription);
    }

    return this;
  }

  public override bool Equals(object? aObject) =>
    aObject is Subscriptions subscriptions &&
    EqualityComparer<ILogger>.Default.Equals(Logger, subscriptions.Logger) &&
    EqualityComparer<List<Subscription>>.Default.Equals(TimeWarpStateComponentReferencesList, subscriptions.TimeWarpStateComponentReferencesList);

  public override int GetHashCode() => HashCode.Combine(Logger, TimeWarpStateComponentReferencesList);

  public Subscriptions Remove(ITimeWarpStateComponent timeWarpStateComponent)
  {
    Logger.LogDebug
    (
      EventIds.Subscriptions_RemovingComponentSubscriptions,
      "Removing Subscription for {timeWarpStateComponent_Id}",
      timeWarpStateComponent.Id
    );

    TimeWarpStateComponentReferencesList.RemoveAll(record => record.ComponentId == timeWarpStateComponent.Id);

    return this;
  }

  /// <summary>
  /// Will iterate over all subscriptions for the given type and call ReRender on each.
  /// If the target component no longer exists it will remove its subscription.
  /// </summary>
  /// <typeparam name="T">The type of state, which must implement IState</typeparam>
  public void ReRenderSubscribers<T>() where T : IState
  {
    Type type = typeof(T);

    ReRenderSubscribers(type);
  }

  /// <summary>
  /// Will iterate over all subscriptions for the given type and call ReRender on each.
  /// If the target component no longer exists it will remove its subscription.
  /// </summary>
  /// <param name="stateType"></param>
  public void ReRenderSubscribers(Type stateType)
  {
    var subscriptions = TimeWarpStateComponentReferencesList
      .Where(record => record.StateType == stateType)
      .ToList();
    
    foreach (Subscription subscription in subscriptions)
    {
      if (subscription.TimeWarpStateComponentReference.TryGetTarget(out ITimeWarpStateComponent? target))
      {
        if (target.ShouldReRender(stateType))
        {
          LogReRender(subscription);
          target.ReRender();
        }
      }
      else
      {
        // If Dispose is called will I ever have items in this list that got Garbage collected?
        // Maybe for those that don't inherit from our BaseComponent?
        LogRemoveSubscription(subscription);
        TimeWarpStateComponentReferencesList.Remove(subscription);
      }
    }
  }
  private void LogRemoveSubscription(Subscription subscription) => Logger.LogDebug
  (
    EventIds.Subscriptions_RemoveSubscription,
    "Removing Subscription for ComponentId:{subscription_ComponentId} StateType.Name:{subscription_StateType_Name}",
    subscription.ComponentId,
    subscription.StateType.Name
  );
  
  private void LogReRender(Subscription subscription) => Logger.LogDebug
  (
    EventIds.Subscriptions_ReRenderingSubscribers,
    "ReRender ComponentId:{subscription_ComponentId} StateType.Name:{subscription_StateType_Name}",
    subscription.ComponentId,
    subscription.StateType.Name
  );

  private readonly struct Subscription : IEquatable<Subscription>
  {
    public WeakReference<ITimeWarpStateComponent> TimeWarpStateComponentReference { get; }

    public string ComponentId { get; }

    public Type StateType { get; }

    public Subscription(Type stateType, string componentId, WeakReference<ITimeWarpStateComponent> timeWarpStateComponentReference)
    {
      StateType = stateType;
      ComponentId = componentId;
      TimeWarpStateComponentReference = timeWarpStateComponentReference;
    }

    public static bool operator !=(Subscription leftSubscription, Subscription rightSubscription) => !(leftSubscription == rightSubscription);

    public static bool operator ==(Subscription leftSubscription, Subscription rightSubscription) => leftSubscription.Equals(rightSubscription);

    public bool Equals(Subscription subscription) =>
      EqualityComparer<Type>.Default.Equals(StateType, subscription.StateType) &&
      ComponentId == subscription.ComponentId &&
      EqualityComparer<WeakReference<ITimeWarpStateComponent>>.Default.Equals(TimeWarpStateComponentReference, subscription.TimeWarpStateComponentReference);

    public override bool Equals(object? aObject) => aObject is Subscription subscription && this.Equals(subscription);

    public override int GetHashCode() => ComponentId.GetHashCode();
  }
}
