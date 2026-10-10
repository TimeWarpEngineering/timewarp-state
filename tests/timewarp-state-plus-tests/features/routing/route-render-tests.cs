#region Purpose
// TwPageTitle and TimeWarpPageRenderNotifier send PushRouteInfo after render.
#endregion

#region Design
// There is no bUnit host. The test sets the injected members and calls OnAfterRenderAsync.
// TwPageTitle sends only on the first render. The notifier sends on every render.
#endregion

namespace RouteRender_;

public class RouteRender_Should
{
  public async Task TwPageTitle_Pushes_Route_Info_On_The_First_Render_Only()
  {
    RecordingSender sender = new();
    RouteState routeState = new(sender);
    IStore store = A.Fake<IStore>();
    A.CallTo(() => store.GetState<RouteState>()).Returns(routeState);

    TwPageTitle pageTitle = new();
    SetNonPublic(pageTitle, "Store", store);

    await InvokeAfterRender(pageTitle, firstRender: true);
    await InvokeAfterRender(pageTitle, firstRender: false);

    sender.Sent.Count.ShouldBe(1);
    sender.Sent[0].ShouldBeOfType<RouteState.PushRouteInfoActionSet.Action>();
  }

  public async Task TimeWarpPageRenderNotifier_Pushes_Route_Info_On_Every_Render()
  {
    RecordingSender sender = new();
    TimeWarpPageRenderNotifier notifier = new();
    SetNonPublic(notifier, "Sender", sender);

    await InvokeAfterRender(notifier, firstRender: true);
    await InvokeAfterRender(notifier, firstRender: false);

    sender.Sent.Count.ShouldBe(2);
    sender.Sent.ShouldAllBe(sent => sent is RouteState.PushRouteInfoActionSet.Action);
  }

  private static async Task InvokeAfterRender(object component, bool firstRender)
  {
    MethodInfo? method = null;
    for (Type? type = component.GetType(); type != null && method is null; type = type.BaseType)
    {
      method = type.GetMethod
      (
        "OnAfterRenderAsync",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
        binder: null,
        types: [typeof(bool)],
        modifiers: null
      );
    }

    Task task = (Task)method!.Invoke(component, [firstRender])!;
    await task;
  }

  private static void SetNonPublic(object target, string propertyName, object value)
  {
    for (Type? type = target.GetType(); type != null; type = type.BaseType)
    {
      PropertyInfo? property = type.GetProperty
      (
        propertyName,
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
      );
      if (property is null)
      {
        continue;
      }

      property.SetValue(target, value);
      return;
    }

    throw new InvalidOperationException($"Property {propertyName} was not found on {target.GetType().FullName}.");
  }

  private sealed class RecordingSender : ISender<ClientPipeline>
  {
    public List<object> Sent { get; } = [];

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
      Sent.Add(request!);
      return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      return Task.FromResult<object?>(null);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>
    (
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default
    ) =>
      throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }
}
