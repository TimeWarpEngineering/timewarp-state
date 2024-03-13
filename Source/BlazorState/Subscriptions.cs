namespace BlazorState;

public class Subscriptions
{
  private readonly ILogger Logger;

  private readonly List<Subscription> BlazorStateComponentReferencesList;

  public Subscriptions(ILogger<Subscriptions> logger)
  {
    Logger = logger;
    Logger.LogDebug(EventIds.Subscriptions_Initializing, "constructing");
    BlazorStateComponentReferencesList = new List<Subscription>();
  }

  public Subscriptions Add<T>(IBlazorStateComponent blazorStateComponent)
  {
    Type type = typeof(T);
    return Add(type, blazorStateComponent);
  }

  public Subscriptions Add(Type type, IBlazorStateComponent blazorStateComponent)
  {

    // Add only once.
    if (!BlazorStateComponentReferencesList.Any(subscription => subscription.StateType == type && subscription.ComponentId == blazorStateComponent.Id))
    {
      Logger.LogDebug
      (
        EventIds.Subscriptions_Adding,
        "adding subscription for Id:{id} Type.Name:{type_name}",
        blazorStateComponent.Id,
        type.Name
      );

      var subscription = new Subscription(
        type,
        blazorStateComponent.Id,
        new WeakReference<IBlazorStateComponent>(blazorStateComponent));

      BlazorStateComponentReferencesList.Add(subscription);
    }

    return this;
  }

  public override bool Equals(object aObject) =>
    aObject is Subscriptions subscriptions &&
    EqualityComparer<ILogger>.Default.Equals(Logger, subscriptions.Logger) &&
    EqualityComparer<List<Subscription>>.Default.Equals(BlazorStateComponentReferencesList, subscriptions.BlazorStateComponentReferencesList);

  public override int GetHashCode() => HashCode.Combine(Logger, BlazorStateComponentReferencesList);

  public Subscriptions Remove(IBlazorStateComponent blazorStateComponent)
  {
    Logger.LogDebug
    (
      EventIds.Subscriptions_RemovingComponentSubscriptions,
      "Removing Subscription for {aBlazorStateComponent_Id}",
      blazorStateComponent.Id
    );

    BlazorStateComponentReferencesList.RemoveAll(record => record.ComponentId == blazorStateComponent.Id);

    return this;
  }

  /// <summary>
  /// Will iterate over all subscriptions for the given type and call ReRender on each.
  /// If the target component no longer exists it will remove its subscription.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public void ReRenderSubscribers<T>()
  {
    Type type = typeof(T);

    ReRenderSubscribers(type);
  }

  /// <summary>
  /// Will iterate over all subscriptions for the given type and call ReRender on each.
  /// If the target component no longer exists it will remove its subscription.
  /// </summary>
  /// <param name="type"></param>
  public void ReRenderSubscribers(Type type)
  {
    IEnumerable<Subscription> subscriptions = BlazorStateComponentReferencesList.Where(record => record.StateType == type);
    foreach (Subscription subscription in subscriptions.ToList())
    {
      if (subscription.BlazorStateComponentReference.TryGetTarget(out IBlazorStateComponent target))
      {
        Logger.LogDebug
        (
          EventIds.Subscriptions_ReRenderingSubscribers,
          "ReRender ComponentId:{subscription_ComponentId} StateType.Name:{subscription_StateType_Name}",
          subscription.ComponentId,
          subscription.StateType.Name
        );

        target.ReRender();
      }
      else
      {
        // If Dispose is called will I ever have items in this list that got Garbage collected?
        // Maybe for those that don't inherit from our BaseComponent?
        Logger.LogDebug
        (
          EventIds.Subscriptions_RemoveSubscription,
          "Removing Subscription for ComponentId:{subscription_ComponentId} StateType.Name:{subscription_StateType_Name}",
          subscription.ComponentId,
          subscription.StateType.Name
        );

        BlazorStateComponentReferencesList.Remove(subscription);
      }
    }
  }

  private readonly struct Subscription : IEquatable<Subscription>
  {
    public WeakReference<IBlazorStateComponent> BlazorStateComponentReference { get; }

    public string ComponentId { get; }

    public Type StateType { get; }

    public Subscription(Type stateType, string componentId, WeakReference<IBlazorStateComponent> blazorStateComponentReference)
    {
      StateType = stateType;
      ComponentId = componentId;
      BlazorStateComponentReference = blazorStateComponentReference;
    }

    public static bool operator !=(Subscription leftSubscription, Subscription rightSubscription) => !(leftSubscription == rightSubscription);

    public static bool operator ==(Subscription leftSubscription, Subscription rightSubscription) => leftSubscription.Equals(rightSubscription);

    public bool Equals(Subscription subscription) =>
      EqualityComparer<Type>.Default.Equals(StateType, subscription.StateType) &&
      ComponentId == subscription.ComponentId &&
      EqualityComparer<WeakReference<IBlazorStateComponent>>.Default.Equals(BlazorStateComponentReference, subscription.BlazorStateComponentReference);

    public override bool Equals(object aObject) => this.Equals((Subscription)aObject);

    public override int GetHashCode() => ComponentId.GetHashCode();
  }
}
