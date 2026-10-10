#region Purpose
// Proves AddTimeWarpStateBlazor registers the Blazor services once, in either order relative to AddTimeWarpState,
// and that UseReduxDevTools / AddJavaScriptDispatch register JsonRequestHandler on their own.
#endregion

#region Design
// Inspects ServiceDescriptors instead of resolving: JsonRequestHandler needs a mediator, which these tests do not build.
// AddTimeWarpStateBlazor does not register HttpClient.
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

  public static void Register_JsonRequestHandler_From_UseReduxDevTools()
  {
    ServiceCollection services = new();

    services.AddTimeWarpState(options =>
    {
      options.Assemblies = [typeof(Should_).Assembly];
      options.UseReduxDevTools();
    });

    Count<JsonRequestHandler>(services).ShouldBe(1);
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

  private static int Count<T>(ServiceCollection services) =>
    services.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(T));
}
