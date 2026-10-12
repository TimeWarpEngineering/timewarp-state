#region Purpose
// Proves StateRenderKindObserver records the render kind only when a host registered IStateRenderKind,
// and skips quietly when RendererInfo throws.
#endregion

#region Design
// Observe takes a service provider and a delegate, so these tests pass a real container and a delegate
// that stands in for RendererInfo.IsInteractive. No component or renderer is needed.
#endregion

namespace StateRenderKindObserverTests;

public class Should_
{
  public static void Skip_When_No_ServiceProvider()
  {
    bool read = false;
    StateRenderKindObserver.Observe(null, () => read = true);
    read.ShouldBeFalse();
  }

  public static void Skip_And_Not_Read_RendererInfo_When_No_Kind_Is_Registered()
  {
    using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
    bool read = false;
    StateRenderKindObserver.Observe(provider, () => read = true);
    read.ShouldBeFalse();
  }

  public static void Leave_The_Kind_Unset_When_RendererInfo_Throws()
  {
    StateRenderKind stateRenderKind = new();
    using ServiceProvider provider = Provider(stateRenderKind);
    StateRenderKindObserver.Observe(provider, () => throw new InvalidOperationException("No renderer."));
    stateRenderKind.Name.ShouldBeNull();
  }

  public static void Record_Interactive_From_RendererInfo()
  {
    StateRenderKind stateRenderKind = new();
    using ServiceProvider provider = Provider(stateRenderKind);
    StateRenderKindObserver.Observe(provider, () => true);
    stateRenderKind.Name.ShouldBe(StateRenderKinds.Interactive);
  }

  private static ServiceProvider Provider(StateRenderKind stateRenderKind)
  {
    ServiceCollection services = new();
    services.AddSingleton<IStateRenderKind>(stateRenderKind);
    return services.BuildServiceProvider();
  }
}
