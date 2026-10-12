#region Purpose
// Proves AddTimeWarpStateBlazor registers the Blazor services once, in either order relative to AddTimeWarpState,
// that AddJavaScriptDispatch registers JsonRequestHandler on its own, and that a host can validate the
// container without a Redux DevTools registration.
#endregion

#region Design
// The registration tests inspect ServiceDescriptors. ValidateOnBuild_Without_ReduxDevTools builds a provider
// the way a Development host does. ContainerProbeAction is public so the mediator generator emits
// AddGeneratedMediator<ClientPipeline> and links every handler the referenced Blazor assembly exposes.
// A handler whose services are missing fails ValidateOnBuild. AddTimeWarpStateBlazor does not register HttpClient.
#endregion

namespace AddTimeWarpStateBlazorTests;

public class Should_
{
  public static void Register_Blazor_Services_After_AddTimeWarpState()
  {
    ServiceCollection services = new();

    services.AddTimeWarpState(options => options.Assemblies = [typeof(Should_).Assembly]);
    services.AddTimeWarpStateBlazor();

    AssertBlazorServicesRegisteredOnce(services);
  }

  public static void Register_Blazor_Services_Before_AddTimeWarpState_And_Twice()
  {
    ServiceCollection services = new();

    services.AddTimeWarpStateBlazor();
    services.AddTimeWarpState(options => options.Assemblies = [typeof(Should_).Assembly]);
    services.AddTimeWarpStateBlazor();

    AssertBlazorServicesRegisteredOnce(services);
  }

  public static void Register_JsonRequestHandler_From_AddJavaScriptDispatch()
  {
    ServiceCollection services = new();

    services.AddJavaScriptDispatch(_ => { });
    services.AddTimeWarpStateBlazor();

    Count<JsonRequestHandler>(services).ShouldBe(1);
  }

  private static void AssertBlazorServicesRegisteredOnce(ServiceCollection services)
  {
    Count<RenderSubscriptionContext>(services).ShouldBe(1);
    Count<JsonRequestHandler>(services).ShouldBe(1);
    Count<JavaScriptDispatchRegistry>(services).ShouldBe(1);
  }

  public static void ValidateOnBuild_Without_ReduxDevTools()
  {
    ServiceCollection services = new();
    services.AddSingleton<IJSRuntime, NullJsRuntime>();
    services.AddGeneratedMediator<ClientPipeline>();
    // The test assembly's fixture states take services a host does not register. Scan the library instead.
    services.AddTimeWarpState(options => options.Assemblies = [typeof(TimeWarpStateOptions).Assembly]);
    services.AddTimeWarpStateBlazor();

    using ServiceProvider provider = services.BuildServiceProvider
    (
      new ServiceProviderOptions
      {
        ValidateOnBuild = true,
        ValidateScopes = true
      }
    );

    using IServiceScope scope = provider.CreateScope();
    scope.ServiceProvider.GetRequiredService<IStore>().ShouldNotBeNull();
  }

  // Public so the mediator generator links it. A private request is invisible to that generator, and then
  // AddGeneratedMediator<ClientPipeline> is not emitted and referenced handlers are not registered.
  public sealed class ContainerProbeAction : IAction;

  public sealed class ContainerProbeHandler : IRequestHandler<ContainerProbeAction>
  {
    public Task Handle(ContainerProbeAction request, CancellationToken cancellationToken) => Task.CompletedTask;
  }

  private static int Count<T>(ServiceCollection services) =>
    services.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(T));

  private sealed class NullJsRuntime : IJSRuntime
  {
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => default;

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => default;
  }
}
