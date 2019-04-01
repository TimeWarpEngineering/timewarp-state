namespace BlazorState
{
  using System;
  using System.Collections.Generic;

  public class Subscriptions
  {
    public Subscriptions()
    {
      BlazorStateComponentReferencesDictionary = new Dictionary<Type, List<WeakReference<BlazorStateComponent>>>();
    }

    private Dictionary<Type, List<WeakReference<BlazorStateComponent>>> BlazorStateComponentReferencesDictionary;

    public Subscriptions Add<T>(BlazorStateComponent aBlazorStateComponent)
    {
      Type type = typeof(T);

      return Add(type, aBlazorStateComponent);
    }
    public Subscriptions Add(Type aType, BlazorStateComponent aBlazorStateComponent)
    {

      //if (!typeof(IState).IsAssignableFrom(aStateType))
      //{
      //  throw new ArgumentException("Type must implement IState");
      //}

      if (!(BlazorStateComponentReferencesDictionary.TryGetValue(aType, out List<WeakReference<BlazorStateComponent>> blazorStateComponentReferences)))
      {
        blazorStateComponentReferences = new List<WeakReference<BlazorStateComponent>>();
        BlazorStateComponentReferencesDictionary.Add(aType, blazorStateComponentReferences);
      }

      blazorStateComponentReferences.Add(new WeakReference<BlazorStateComponent>(aBlazorStateComponent));

      return this;
    }

    public void ReRenderSubscribers<T>()
    {
      Type type = typeof(T);

      ReRenderSubscribers(type);
    }
    /// <summary>
    /// Will iterate over all subscriptions for the given type and call ReRender on each.
    /// If the target component no longer exists it will remove its subscription.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ReRenderSubscribers(Type aType)
    {
      //GC.Collect();  // I added the collect to test that I am not holding strong references and they were collected.
      bool isAny = BlazorStateComponentReferencesDictionary.TryGetValue(aType, out List<WeakReference<BlazorStateComponent>> blazorStateblazorStateComponentReferencesComponents);
      if (isAny)
      {
        Console.WriteLine($"ReRendering {blazorStateblazorStateComponentReferencesComponents.Count} Subscribers for state of type: {aType.GetType().Name}");
        WeakReference<BlazorStateComponent>[] blazorStateComponentReferencesArray = blazorStateblazorStateComponentReferencesComponents.ToArray();

        foreach (WeakReference<BlazorStateComponent> aBlazorStateComponentReference in blazorStateComponentReferencesArray)
        {
          if (aBlazorStateComponentReference.TryGetTarget(out BlazorStateComponent target))
          {
            Console.WriteLine($"ReRender: {target.GetType().Name}");
            target.ReRender();
          }
          else
          {
            Console.WriteLine($"Removing subscription to previously destroyed component.");
            blazorStateblazorStateComponentReferencesComponents.Remove(aBlazorStateComponentReference);
          }
        }
      }
    }
  }
}
