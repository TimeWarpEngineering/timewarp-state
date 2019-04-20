namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using Microsoft.Extensions.Logging;

  public class Subscriptions
  {
    public Subscriptions(ILogger<Subscriptions> aLogger)
    {
      Logger = aLogger;
      BlazorStateComponentReferencesDictionary = new Dictionary<Type, List<WeakReference<BlazorStateComponent>>>();
    }

    private ILogger Logger { get; }
    private Dictionary<Type, List<WeakReference<BlazorStateComponent>>> BlazorStateComponentReferencesDictionary { get; }

    public Subscriptions Add<T>(BlazorStateComponent aBlazorStateComponent)
    {
      Type type = typeof(T);

      return Add(type, aBlazorStateComponent);
    }
    public Subscriptions Add(Type aType, BlazorStateComponent aBlazorStateComponent)
    {

      if (!(BlazorStateComponentReferencesDictionary.TryGetValue(aType, out List<WeakReference<BlazorStateComponent>> blazorStateComponentReferences)))
      {
        blazorStateComponentReferences = new List<WeakReference<BlazorStateComponent>>();
        BlazorStateComponentReferencesDictionary.Add(aType, blazorStateComponentReferences);
      }

      blazorStateComponentReferences.Add(new WeakReference<BlazorStateComponent>(aBlazorStateComponent));

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
      //GC.Collect();  // I added the collect to test that I am not holding strong references and they were collected.
      bool isAny = BlazorStateComponentReferencesDictionary.TryGetValue(aType, out List<WeakReference<BlazorStateComponent>> blazorStateblazorStateComponentReferencesComponents);
      if (isAny)
      {
        Logger.LogDebug($"ReRendering {blazorStateblazorStateComponentReferencesComponents.Count} Subscribers for state of type: {aType.Name}");
        WeakReference<BlazorStateComponent>[] blazorStateComponentReferencesArray = blazorStateblazorStateComponentReferencesComponents.ToArray();

        foreach (WeakReference<BlazorStateComponent> aBlazorStateComponentReference in blazorStateComponentReferencesArray)
        {
          if (aBlazorStateComponentReference.TryGetTarget(out BlazorStateComponent target))
          {
            Logger.LogDebug($"ReRender: {target.GetType().Name}");
            target.ReRender();
          }
          else
          {
            Logger.LogDebug($"Removing subscription to previously destroyed component.");
            blazorStateblazorStateComponentReferencesComponents.Remove(aBlazorStateComponentReference);
          }
        }
      }
    }
  }
}
