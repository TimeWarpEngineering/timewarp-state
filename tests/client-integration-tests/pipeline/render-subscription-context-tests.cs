namespace RenderSubscriptionContextTests;

/// <summary>
/// Integration tests for RenderSubscriptionContext.
/// Controls whether subscriptions fire for specific actions.
/// </summary>
public class RenderSubscriptionContext_Should : BaseTest
{
  public RenderSubscriptionContext_Should(ClientHost clientHost) : base(clientHost) { }

  public void ReturnTrue_ForUnregisteredAction()
  {
    // Arrange
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };

    // Act
    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);

    // Assert - unregistered actions should fire subscriptions by default
    shouldFire.ShouldBeTrue();
  }

  public void ReturnFalse_WhenRegisteredWithFalse()
  {
    // Arrange
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };

    // Act
    RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: false);
    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);

    // Assert
    shouldFire.ShouldBeFalse();
  }

  public void ReturnTrue_WhenRegisteredWithTrue()
  {
    // Arrange
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };

    // Act
    RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: true);
    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);

    // Assert
    shouldFire.ShouldBeTrue();
  }

  public void ReturnTrue_AfterReset()
  {
    // Arrange - register an action to not fire
    RenderSubscriptionContext.Reset();
    var action = new BlueState.IncrementCountActionSet.Action { Amount = 1 };
    RenderSubscriptionContext.EnsureAction(action, shouldFireSubscriptions: false);
    RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action).ShouldBeFalse();

    // Act
    RenderSubscriptionContext.Reset();

    // Assert - after reset, unregistered actions should fire by default
    bool shouldFire = RenderSubscriptionContext.ShouldFireSubscriptionsForAction(action);
    shouldFire.ShouldBeTrue();
  }
}
