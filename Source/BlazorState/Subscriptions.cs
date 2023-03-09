namespace BlazorState;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

public class Subscriptions
{
  private readonly ILogger Logger;

  private readonly List<Subscription> BlazorStateComponentReferencesList;

  public Subscriptions(ILogger<Subscriptions> aLogger)
  {
    Logger = aLogger;
    Logger.LogDebug(EventIds.Subscriptions_Initializing, "constructing");
    BlazorStateComponentReferencesList = new List<Subscription>();
  }

  public Subscriptions Add<T>(IBlazorStateComponent aBlazorStateComponent)
  {
    Type type = typeof(T);
    return Add(type, aBlazorStateComponent);
  }

  public Subscriptions Add(Type aType, IBlazorStateComponent aBlazorStateComponent)
  {

    // Add only once.
    if (!BlazorStateComponentReferencesList.Any(aSubscription => aSubscription.StateType == aType && aSubscription.ComponentId == aBlazorStateComponent.Id))
    {
      Logger.LogDebug
      (
        EventIds.Subscriptions_Adding,
        "adding subscription for Id:{id} Type.Name:{type_name}",
        aBlazorStateComponent.Id,
        aType.Name
      );

      var subscription = new Subscription(
        aType,
        aBlazorStateComponent.Id,
        new WeakReference<IBlazorStateComponent>(aBlazorStateComponent));

      BlazorStateComponentReferencesList.Add(subscription);
    }

    return this;
  }

  public override bool Equals(object aObject) =>
    aObject is Subscriptions subscriptions &&
    EqualityComparer<ILogger>.Default.Equals(Logger, subscriptions.Logger) &&
    EqualityComparer<List<Subscription>>.Default.Equals(BlazorStateComponentReferencesList, subscriptions.BlazorStateComponentReferencesList);

  public override int GetHashCode() => HashCode.Combine(Logger, BlazorStateComponentReferencesList);

  public Subscriptions Remove(IBlazorStateComponent aBlazorStateComponent)
  {
    Logger.LogDebug
    (
      EventIds.Subscriptions_RemovingComponentSubscriptions,
      "Removing Subscription for {aBlazorStateComponent_Id}",
      aBlazorStateComponent.Id
    );

    BlazorStateComponentReferencesList.RemoveAll(aRecord => aRecord.ComponentId == aBlazorStateComponent.Id);

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
  /// <param name="aType"></param>
  public void ReRenderSubscribers(Type aType)
  {
    IEnumerable<Subscription> subscriptions = BlazorStateComponentReferencesList.Where(aRecord => aRecord.StateType == aType);
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

    public Subscription(Type aStateType, string aComponentId, WeakReference<IBlazorStateComponent> aBlazorStateComponentReference)
    {
      StateType = aStateType;
      ComponentId = aComponentId;
      BlazorStateComponentReference = aBlazorStateComponentReference;
    }

    public static bool operator !=(Subscription aLeftSubscription, Subscription aRightSubscription) => !(aLeftSubscription == aRightSubscription);

    public static bool operator ==(Subscription aLeftSubscription, Subscription aRightSubscription) => aLeftSubscription.Equals(aRightSubscription);

    public bool Equals(Subscription aSubscription) =>
      EqualityComparer<Type>.Default.Equals(StateType, aSubscription.StateType) &&
      ComponentId == aSubscription.ComponentId &&
      EqualityComparer<WeakReference<IBlazorStateComponent>>.Default.Equals(BlazorStateComponentReference, aSubscription.BlazorStateComponentReference);

    public override bool Equals(object aObject) => this.Equals((Subscription)aObject);

    public override int GetHashCode() => ComponentId.GetHashCode();
  }
}
