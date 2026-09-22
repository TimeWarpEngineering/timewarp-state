namespace RenderSubscriptionContextTests;

/// <summary>
/// Instance-keyed RenderSubscriptionContext must not leak across dispatches of the same action type.
/// </summary>
public class RenderSubscriptionContext_Should : BaseTest
{
  public RenderSubscriptionContext_Should(ClientHost clientHost) : base(clientHost) { }

  public void ReturnTrue_ForUnregisteredAction()
  {
#pragma warning disable CS0618
    RenderSubscriptionContext.Reset();
#pragma warning restore CS0618
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };

    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);

    shouldFire.ShouldBeTrue();
  }

  public void ReturnFalse_WhenRegisteredWithFalse()
  {
#pragma warning disable CS0618
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };

    RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: false);
#pragma warning restore CS0618
    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);

    shouldFire.ShouldBeFalse();
  }

  public void ReturnTrue_WhenRegisteredWithTrue()
  {
#pragma warning disable CS0618
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };

    RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: true);
#pragma warning restore CS0618
    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);

    shouldFire.ShouldBeTrue();
  }

  public void ReturnTrue_AfterReset()
  {
#pragma warning disable CS0618
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };
    RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: false);
    RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action).ShouldBeFalse();

    RenderSubscriptionContext.Reset();
#pragma warning restore CS0618

    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);
    shouldFire.ShouldBeTrue();
  }

  public void Not_Suppress_A_Second_Instance_Of_The_Same_Action_Type()
  {
#pragma warning disable CS0618
    RenderSubscriptionContext.Reset();
    var firstAction = new BlueState.IncrementCountActionSet.Action { Amount = 1 };
    RenderSubscriptionContext.EnsureAction(firstAction, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    var secondAction = new BlueState.IncrementCountActionSet.Action { Amount = 1 };
    RenderSubscriptionContext.ShouldFireSubscriptionsForAction(firstAction).ShouldBeFalse();
    RenderSubscriptionContext.ShouldFireSubscriptionsForAction(secondAction).ShouldBeTrue();
  }

  public async Task Send_Still_ReRenders_After_EnsureAction_On_Another_Instance()
  {
    var component = new TestableComponent("blue-leak-1");
    Subscriptions.Add<BlueState>(component);

    var dummy = new BlueState.IncrementCountActionSet.Action { Amount = 1 };
#pragma warning disable CS0618
    RenderSubscriptionContext.EnsureAction(dummy, shouldFireSubscriptions: false);
#pragma warning restore CS0618

    await Send(new BlueState.IncrementCountActionSet.Action { Amount = 1 });

    component.ReRenderCount.ShouldBe(1);
  }

  private sealed class TestableComponent : ITimeWarpStateComponent
  {
    public string Id { get; }
    public int ReRenderCount { get; private set; }

    public TestableComponent(string id) => Id = id;
    public void ReRender() => ReRenderCount++;
    public bool ShouldReRender(Type stateType) => true;
  }
}
