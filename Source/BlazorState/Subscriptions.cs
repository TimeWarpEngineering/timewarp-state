namespace BlazorState
{
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
      BlazorStateComponentReferencesList = new List<Subscription>();
    }

    public Subscriptions Add<T>(BlazorStateComponent aBlazorStateComponent)
    {
      Type type = typeof(T);

      return Add(type, aBlazorStateComponent);
    }

    public Subscriptions Add(Type aType, BlazorStateComponent aBlazorStateComponent)
    {
      // Add only once.
      if (!BlazorStateComponentReferencesList.Any(aSubscription => aSubscription.StateType == aType && aSubscription.ComponentId == aBlazorStateComponent.Id))
      {
        var subscription = new Subscription(
          aType,
          aBlazorStateComponent.Id,
          new WeakReference<BlazorStateComponent>(aBlazorStateComponent));

        BlazorStateComponentReferencesList.Add(subscription);
      }

      return this;
    }

    public override bool Equals(object obj) => obj is Subscriptions subscriptions && EqualityComparer<ILogger>.Default.Equals(Logger, subscriptions.Logger) && EqualityComparer<List<Subscription>>.Default.Equals(BlazorStateComponentReferencesList, subscriptions.BlazorStateComponentReferencesList);

    public override int GetHashCode()
    {
      var hashCode = -914156548;
      hashCode = hashCode * -1521134295 + EqualityComparer<ILogger>.Default.GetHashCode(Logger);
      hashCode = hashCode * -1521134295 + EqualityComparer<List<Subscription>>.Default.GetHashCode(BlazorStateComponentReferencesList);
      return hashCode;
    }

    public Subscriptions Remove(BlazorStateComponent aBlazorStateComponent)
    {
      Logger.LogDebug($"Removing Subscription for {aBlazorStateComponent.Id}");
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
        if (subscription.BlazorStateComponentReference.TryGetTarget(out BlazorStateComponent target))
        {
          Logger.LogDebug($"ReRender ComponentId:{subscription.ComponentId} StateType.Name:{subscription.StateType.Name}");
          target.ReRender();
        }
        else
        {
          // If Dispose is called will I ever have items in this list that got Garbage collected?
          // Maybe for those that don't inherit from our BaseComponent?
          Logger.LogDebug($"Removing Subscription for ComponentId:{subscription.ComponentId} StateType.Name:{subscription.StateType.Name}");
          BlazorStateComponentReferencesList.Remove(subscription);
        }
      }
    }

    private readonly struct Subscription : IEquatable<Subscription>
    {
      public WeakReference<BlazorStateComponent> BlazorStateComponentReference { get; }

      public string ComponentId { get; }

      public Type StateType { get; }

      public Subscription(Type aStateType, string aComponentId, WeakReference<BlazorStateComponent> aBlazorStateComponentReference)
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
        EqualityComparer<WeakReference<BlazorStateComponent>>.Default.Equals(BlazorStateComponentReference, aSubscription.BlazorStateComponentReference);

      public override bool Equals(object aObject) => this.Equals((Subscription)aObject);

      public override int GetHashCode() => ComponentId.GetHashCode();
    }
  }
}