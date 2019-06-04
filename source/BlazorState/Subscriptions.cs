namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.Extensions.Logging;

  public class Subscriptions
  {
    struct Subscription
    {
      public Type StateType { get; set; }
      public string ComponentId { get; set; }
      public WeakReference<BlazorStateComponent> BlazorStateComponentReference { get; set; }
    }

    public Subscriptions(ILogger<Subscriptions> aLogger)
    {
      Logger = aLogger;
      BlazorStateComponentReferencesList = new List<Subscription>();
    }

    private ILogger Logger { get; }
    private List<Subscription> BlazorStateComponentReferencesList { get; }

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
        var subscription = new Subscription
        {
          ComponentId = aBlazorStateComponent.Id,
          StateType = aType,
          BlazorStateComponentReference = new WeakReference<BlazorStateComponent>(aBlazorStateComponent)
        };
        BlazorStateComponentReferencesList.Add(subscription);
      }

      return this;
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
      foreach (Subscription subscription in subscriptions)
      {
        if (subscription.BlazorStateComponentReference.TryGetTarget(out BlazorStateComponent target))
        {
          Logger.LogDebug($"ReRender: {target.GetType().Name}");
          target.ReRender();
        }
        else
        {
          // If Dispose is called will I ever have items in this list that got Garbage collected?
          // Maybe for those that don't inherit from our BaseComponent.
          BlazorStateComponentReferencesList.Remove(subscription);
        }
      }
    }
  }
}
