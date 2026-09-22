#region Purpose
// [SuppressRender] skips re-render; IInternalAction still re-renders; instance flags do not leak.
#endregion

namespace RenderSubscriptionsPostProcessorTests;

public class Should_
{
  public async Task Skip_ReRender_When_Action_Has_SuppressRender()
  {
    Harness harness = CreateHarness();

    await harness.SilentProcessor.Handle
    (
      new RenderTestState.SilentAction(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(0);
  }

  public async Task ReRender_When_Action_Is_Not_Suppressed()
  {
    Harness harness = CreateHarness();

    await harness.UserProcessor.Handle
    (
      new RenderTestState.UserAction(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(1);
  }

  public async Task ReRender_When_Action_Is_Internal_Without_SuppressRender()
  {
    Harness harness = CreateHarness();

    await harness.InternalProcessor.Handle
    (
      new RenderTestState.InternalAction(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(1);
  }

  public async Task Not_Leak_Suppression_To_Later_Dispatch_Of_Same_Action_Type()
  {
    Harness harness = CreateHarness();
    RenderTestState.UserAction firstAction = new();
#pragma warning disable CS0618
    harness.RenderSubscriptionContext.EnsureAction(firstAction, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    await harness.UserProcessor.Handle
    (
      firstAction,
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(0);

    await harness.UserProcessor.Handle
    (
      new RenderTestState.UserAction(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(1);
  }

  public async Task Not_Leak_Suppression_To_Other_Action_Types()
  {
    Harness harness = CreateHarness();
    RenderTestState.UserAction userAction = new();
#pragma warning disable CS0618
    harness.RenderSubscriptionContext.EnsureAction(userAction, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    await harness.OtherProcessor.Handle
    (
      new RenderTestState.OtherAction(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(1);

    await harness.UserProcessor.Handle
    (
      userAction,
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(1);
  }

  public async Task Skip_ReRender_Only_For_The_Registered_Instance()
  {
    Harness harness = CreateHarness();
    RenderTestState.UserAction suppressedAction = new();
    RenderTestState.UserAction liveAction = new();
#pragma warning disable CS0618
    harness.RenderSubscriptionContext.EnsureAction(suppressedAction, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    await harness.UserProcessor.Handle
    (
      liveAction,
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(1);
  }

  public async Task Clear_Instance_Flag_When_Next_Throws()
  {
    Harness harness = CreateHarness();
    RenderTestState.UserAction action = new();
#pragma warning disable CS0618
    harness.RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    InvalidOperationException? caught = null;
    try
    {
      await harness.UserProcessor.Handle
      (
        action,
        _ => throw new InvalidOperationException("handler failed"),
        CancellationToken.None
      );
    }
    catch (InvalidOperationException exception)
    {
      caught = exception;
    }

    caught.ShouldNotBeNull();
    caught.Message.ShouldBe("handler failed");
    harness.RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action).ShouldBeTrue();
  }

  public async Task Clear_Instance_Flag_After_Successful_Handle()
  {
    Harness harness = CreateHarness();
    RenderTestState.UserAction action = new();
#pragma warning disable CS0618
    harness.RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    await harness.UserProcessor.Handle
    (
      action,
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    harness.Component.ReRenderCount.ShouldBe(0);
    harness.RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action).ShouldBeTrue();
  }

  private static Harness CreateHarness()
  {
    Subscriptions subscriptions = new(NullLogger<Subscriptions>.Instance);
    RenderSubscriptionContext renderSubscriptionContext = new();
    TestableComponent component = new("render-1");
    subscriptions.Add<RenderTestState>(component);

    return new Harness
    {
      Component = component,
      RenderSubscriptionContext = renderSubscriptionContext,
      UserProcessor = new
      (
        NullLogger<RenderSubscriptionsPostProcessor<RenderTestState.UserAction, object>>.Instance,
        subscriptions,
        renderSubscriptionContext
      ),
      SilentProcessor = new
      (
        NullLogger<RenderSubscriptionsPostProcessor<RenderTestState.SilentAction, object>>.Instance,
        subscriptions,
        renderSubscriptionContext
      ),
      InternalProcessor = new
      (
        NullLogger<RenderSubscriptionsPostProcessor<RenderTestState.InternalAction, object>>.Instance,
        subscriptions,
        renderSubscriptionContext
      ),
      OtherProcessor = new
      (
        NullLogger<RenderSubscriptionsPostProcessor<RenderTestState.OtherAction, object>>.Instance,
        subscriptions,
        renderSubscriptionContext
      )
    };
  }

  private sealed class Harness
  {
    public required TestableComponent Component { get; init; }
    public required RenderSubscriptionContext RenderSubscriptionContext { get; init; }
    public required RenderSubscriptionsPostProcessor<RenderTestState.UserAction, object> UserProcessor { get; init; }
    public required RenderSubscriptionsPostProcessor<RenderTestState.SilentAction, object> SilentProcessor { get; init; }
    public required RenderSubscriptionsPostProcessor<RenderTestState.InternalAction, object> InternalProcessor { get; init; }
    public required RenderSubscriptionsPostProcessor<RenderTestState.OtherAction, object> OtherProcessor { get; init; }
  }

  [NotTest]
  private sealed class RenderTestState : IState
  {
    public ISender<ClientPipeline> Sender { get; set; } = null!;
    public Guid Guid { get; } = Guid.Empty;
    public void Initialize() { }
    public void CancelOperations() { }

    public sealed class UserAction : IAction;

    [SuppressRender]
    public sealed class SilentAction : IAction;

    public sealed class InternalAction : IInternalAction;

    public sealed class OtherAction : IAction;
  }

  [NotTest]
  private sealed class TestableComponent : ITimeWarpStateComponent
  {
    public string Id { get; }
    public int ReRenderCount { get; private set; }

    public TestableComponent(string id) => Id = id;
    public void ReRender() => ReRenderCount++;
    public bool ShouldReRender(Type stateType) => true;
  }
}
