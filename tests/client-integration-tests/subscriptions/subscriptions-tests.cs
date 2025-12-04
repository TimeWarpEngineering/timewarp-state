namespace SubscriptionsTests;

using Test.App.Client.Features.Counter;

/// <summary>
/// Integration tests for the Subscriptions system.
/// Tests subscription management, re-rendering, and component lifecycle.
/// </summary>
public class Subscriptions_Should : BaseTest
{
  public Subscriptions_Should(ClientHost clientHost) : base(clientHost) { }

  public void AddSubscription_AndTriggerReRender()
  {
    // Arrange
    var component = new TestableComponent("test-component-1");
    
    // Act
    Subscriptions.Add<CounterState>(component);
    Subscriptions.ReRenderSubscribers<CounterState>();

    // Assert
    component.ReRenderCount.ShouldBe(1);
  }

  public void NotAddDuplicate_ForSameComponentAndState()
  {
    // Arrange
    var component = new TestableComponent("test-component-2");
    
    // Act - add same component twice
    Subscriptions.Add<CounterState>(component);
    Subscriptions.Add<CounterState>(component);
    Subscriptions.ReRenderSubscribers<CounterState>();

    // Assert - should only render once (not duplicated)
    component.ReRenderCount.ShouldBe(1);
  }

  public void RemoveAllSubscriptions_ForComponent()
  {
    // Arrange
    var component = new TestableComponent("test-component-3");
    Subscriptions.Add<CounterState>(component);
    Subscriptions.Add<BlueState>(component);

    // Act
    Subscriptions.Remove(component);
    Subscriptions.ReRenderSubscribers<CounterState>();
    Subscriptions.ReRenderSubscribers<BlueState>();

    // Assert - no re-renders after removal
    component.ReRenderCount.ShouldBe(0);
  }

  public void RespectShouldReRender_ReturnValue()
  {
    // Arrange
    var component = new TestableComponent("test-component-4") { ShouldReRenderValue = false };
    Subscriptions.Add<CounterState>(component);

    // Act
    Subscriptions.ReRenderSubscribers<CounterState>();

    // Assert - should not re-render when ShouldReRender returns false
    component.ReRenderCount.ShouldBe(0);
  }

  public void OnlyReRender_MatchingStateType()
  {
    // Arrange
    var counterComponent = new TestableComponent("counter-component");
    var blueComponent = new TestableComponent("blue-component");
    
    Subscriptions.Add<CounterState>(counterComponent);
    Subscriptions.Add<BlueState>(blueComponent);

    // Act - only trigger CounterState subscribers
    Subscriptions.ReRenderSubscribers<CounterState>();

    // Assert
    counterComponent.ReRenderCount.ShouldBe(1);
    blueComponent.ReRenderCount.ShouldBe(0);
  }

  public void ReRenderMultipleComponents_ForSameState()
  {
    // Arrange
    var component1 = new TestableComponent("multi-component-1");
    var component2 = new TestableComponent("multi-component-2");
    var component3 = new TestableComponent("multi-component-3");
    
    Subscriptions.Add<CounterState>(component1);
    Subscriptions.Add<CounterState>(component2);
    Subscriptions.Add<CounterState>(component3);

    // Act
    Subscriptions.ReRenderSubscribers<CounterState>();

    // Assert - all components should be re-rendered
    component1.ReRenderCount.ShouldBe(1);
    component2.ReRenderCount.ShouldBe(1);
    component3.ReRenderCount.ShouldBe(1);
  }

  /// <summary>
  /// Minimal test double implementing ITimeWarpStateComponent.
  /// </summary>
  private class TestableComponent : ITimeWarpStateComponent
  {
    public string Id { get; }
    public int ReRenderCount { get; private set; }
    public bool ShouldReRenderValue { get; set; } = true;

    public TestableComponent(string id) => Id = id;
    public void ReRender() => ReRenderCount++;
    public bool ShouldReRender(Type stateType) => ShouldReRenderValue;
  }
}
