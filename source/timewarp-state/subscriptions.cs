#region Purpose
// Circuit-scoped registry of which components subscribe to which state types, used to re-render after actions.
#endregion

#region Design
// Two indexes under one lock: state type → component id → Subscription, and component id → state types.
// Add is TryAdd (O(1)); Remove walks only that component's types; ReRenderSubscribers snapshots one
// state's values. lock covers mutations and the snapshot; ShouldReRender/ReRender run outside so a
// renderer Dispose cannot deadlock a mediator thread. Dead WeakReferences are dropped after the
// snapshot loop (same path as the old List.Remove). Plain lock, not ReaderWriterLockSlim — per-circuit
// contention is low. Equals/GetHashCode stay reference-based on the indexes so they keep compiling.
#endregion

namespace TimeWarp.State;

public class Subscriptions
{
  private readonly ILogger Logger;
  private readonly object SyncRoot = new();
  private readonly Dictionary<Type, Dictionary<string, Subscription>> SubscriptionsByStateType;
  private readonly Dictionary<string, HashSet<Type>> StateTypesByComponentId;

  public Subscriptions(ILogger<Subscriptions> logger)
  {
    Logger = logger;
    Logger.LogDebug(EventIds.Subscriptions_Initializing, "constructing");
    SubscriptionsByStateType = new();
    StateTypesByComponentId = new();
  }

  public Subscriptions Add<T>(ITimeWarpStateComponent timeWarpStateComponent) where T : IState
  {
    Type type = typeof(T);
    return Add(type, timeWarpStateComponent);
  }

  public Subscriptions Add(Type type, ITimeWarpStateComponent timeWarpStateComponent)
  {
    string componentId = timeWarpStateComponent.Id;
    Subscription subscription = new(
      type,
      componentId,
      new WeakReference<ITimeWarpStateComponent>(timeWarpStateComponent));

    bool added;
    lock (SyncRoot)
    {
      added = TryAddSubscription(type, componentId, subscription);
    }

    if (added)
    {
      Logger.LogDebug
      (
        EventIds.Subscriptions_Adding,
        "{id} Type.Name:{type_name} subscription added",
        componentId,
        type.Name
      );
    }

    return this;
  }

  public override bool Equals(object? aObject) =>
    aObject is Subscriptions subscriptions &&
    EqualityComparer<ILogger>.Default.Equals(Logger, subscriptions.Logger) &&
    EqualityComparer<Dictionary<Type, Dictionary<string, Subscription>>>.Default.Equals(SubscriptionsByStateType, subscriptions.SubscriptionsByStateType) &&
    EqualityComparer<Dictionary<string, HashSet<Type>>>.Default.Equals(StateTypesByComponentId, subscriptions.StateTypesByComponentId);

  public override int GetHashCode() => HashCode.Combine(Logger, SubscriptionsByStateType, StateTypesByComponentId);

  public Subscriptions Remove(ITimeWarpStateComponent timeWarpStateComponent)
  {
    string componentId = timeWarpStateComponent.Id;
    Logger.LogDebug
    (
      EventIds.Subscriptions_RemovingComponentSubscriptions,
      "{ComponentId}: Removing Subscriptions",
      componentId
    );

    lock (SyncRoot)
    {
      RemoveAllForComponent(componentId);
    }

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
    Subscription[] snapshot = Snapshot(stateType);
    List<Subscription>? deadSubscriptions = null;

    try
    {
      foreach (Subscription subscription in snapshot)
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
          LogRemoveSubscription(subscription);
          deadSubscriptions ??= [];
          deadSubscriptions.Add(subscription);
        }
      }
    }
    finally
    {
      if (deadSubscriptions is { Count: > 0 })
      {
        lock (SyncRoot)
        {
          foreach (Subscription deadSubscription in deadSubscriptions)
          {
            RemoveDeadSubscription(deadSubscription);
          }
        }
      }
    }
  }

  private bool TryAddSubscription(Type type, string componentId, Subscription subscription)
  {
    if (!SubscriptionsByStateType.TryGetValue(type, out Dictionary<string, Subscription>? subscriptionsByComponentId))
    {
      subscriptionsByComponentId = new();
      SubscriptionsByStateType[type] = subscriptionsByComponentId;
    }

    if (!subscriptionsByComponentId.TryAdd(componentId, subscription))
    {
      return false;
    }

    if (!StateTypesByComponentId.TryGetValue(componentId, out HashSet<Type>? stateTypes))
    {
      stateTypes = new();
      StateTypesByComponentId[componentId] = stateTypes;
    }

    stateTypes.Add(type);
    return true;
  }

  private void RemoveAllForComponent(string componentId)
  {
    if (!StateTypesByComponentId.Remove(componentId, out HashSet<Type>? stateTypes))
    {
      return;
    }

    foreach (Type stateType in stateTypes)
    {
      if (!SubscriptionsByStateType.TryGetValue(stateType, out Dictionary<string, Subscription>? subscriptionsByComponentId))
      {
        continue;
      }

      subscriptionsByComponentId.Remove(componentId);
      if (subscriptionsByComponentId.Count == 0)
      {
        SubscriptionsByStateType.Remove(stateType);
      }
    }
  }

  private Subscription[] Snapshot(Type stateType)
  {
    lock (SyncRoot)
    {
      if (!SubscriptionsByStateType.TryGetValue(stateType, out Dictionary<string, Subscription>? subscriptionsByComponentId)
          || subscriptionsByComponentId.Count == 0)
      {
        return [];
      }

      Subscription[] snapshot = new Subscription[subscriptionsByComponentId.Count];
      subscriptionsByComponentId.Values.CopyTo(snapshot, 0);
      return snapshot;
    }
  }

  private void RemoveDeadSubscription(Subscription deadSubscription)
  {
    if (!SubscriptionsByStateType.TryGetValue(deadSubscription.StateType, out Dictionary<string, Subscription>? subscriptionsByComponentId))
    {
      return;
    }

    if (!subscriptionsByComponentId.TryGetValue(deadSubscription.ComponentId, out Subscription currentSubscription))
    {
      return;
    }

    if (currentSubscription.TimeWarpStateComponentReference.TryGetTarget(out _))
    {
      return;
    }

    subscriptionsByComponentId.Remove(deadSubscription.ComponentId);
    if (subscriptionsByComponentId.Count == 0)
    {
      SubscriptionsByStateType.Remove(deadSubscription.StateType);
    }

    if (!StateTypesByComponentId.TryGetValue(deadSubscription.ComponentId, out HashSet<Type>? stateTypes))
    {
      return;
    }

    stateTypes.Remove(deadSubscription.StateType);
    if (stateTypes.Count == 0)
    {
      StateTypesByComponentId.Remove(deadSubscription.ComponentId);
    }
  }

  private void LogRemoveSubscription(Subscription subscription) => Logger.LogDebug
  (
    EventIds.Subscriptions_RemoveSubscription,
    "{Id}: StateType:{StateType} subscription removed",
    subscription.ComponentId,
    subscription.StateType.Name
  );

  private void LogReRender(Subscription subscription) => Logger.LogDebug
  (
    EventIds.Subscriptions_ReRenderingSubscribers,
    "{Id}: StateType:{StateType} ReRendered",
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
