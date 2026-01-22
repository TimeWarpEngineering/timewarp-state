namespace StoreLifecycle;

using Test.App.Client.Features.Blue;
using Test.App.Client.Features.Counter;

/// <summary>
/// Integration tests for Store state lifecycle management.
/// Tests GetState, Reset, RemoveState, and PreviousState tracking.
/// </summary>
public class GetState_Should : BaseTest
{
  public GetState_Should(ClientHost clientHost) : base(clientHost) { }

  public void ReturnSameStateInstance_OnRepeatedCalls()
  {
    // Act
    CounterState first = Store.GetState<CounterState>();
    CounterState second = Store.GetState<CounterState>();

    // Assert
    first.ShouldBeSameAs(second);
  }

  public void ReturnDifferentInstances_ForDifferentStateTypes()
  {
    // Act
    CounterState counterState = Store.GetState<CounterState>();
    BlueState blueState = Store.GetState<BlueState>();

    // Assert
    counterState.ShouldNotBeNull();
    blueState.ShouldNotBeNull();
    counterState.ShouldNotBeSameAs(blueState);
  }

  public void InitializeState_OnFirstAccess()
  {
    // Act - GetState triggers initialization
    CounterState counterState = Store.GetState<CounterState>();

    // Assert - CounterState.Initialize() sets Count to 3
    counterState.Count.ShouldBe(3);
    counterState.Guid.ShouldNotBe(Guid.Empty);
  }
}

public class Reset_Should : BaseTest
{
  public Reset_Should(ClientHost clientHost) : base(clientHost) { }

  public void CreateNewInstance_AfterReset()
  {
    // Arrange
    CounterState originalState = Store.GetState<CounterState>();
    Guid originalGuid = originalState.Guid;

    // Act
    Store.Reset();
    CounterState newState = Store.GetState<CounterState>();

    // Assert - new instance with different Guid
    newState.ShouldNotBeSameAs(originalState);
    newState.Guid.ShouldNotBe(originalGuid);
  }
}

public class RemoveState_Should : BaseTest
{
  public RemoveState_Should(ClientHost clientHost) : base(clientHost) { }

  public void CreateNewInstance_AfterRemoveState()
  {
    // Arrange
    BlueState originalState = Store.GetState<BlueState>();
    Guid originalGuid = originalState.Guid;

    // Act
    Store.RemoveState<BlueState>();
    BlueState newState = Store.GetState<BlueState>();

    // Assert - new instance with different Guid
    newState.ShouldNotBeSameAs(originalState);
    newState.Guid.ShouldNotBe(originalGuid);
  }
}

public class PreviousState_Should : BaseTest
{
  public PreviousState_Should(ClientHost clientHost) : base(clientHost) { }

  public void ReturnNull_WhenStateNeverExisted()
  {
    // Arrange/Act - get previous state for a state type that was never accessed
    // Using a state that no other test uses to ensure clean state
    // Note: Due to test execution order and shared scopes, previously accessed states
    // may have PreviousState set even after RemoveState/Reset
    
    // Act - get previous state for EventStreamState which is not used by other tests
    var previousState = Store.GetPreviousState<Test.App.Client.Features.EventStream.EventStreamState>();

    // Assert - should be null since this state was never accessed in this scope
    previousState.ShouldBeNull();
  }

  public async Task TrackPreviousState_AfterAction()
  {
    // Arrange
    Store.RemoveState<BlueState>();
    BlueState initialState = Store.GetState<BlueState>();
    int initialCount = initialState.Count;
    Guid initialGuid = initialState.Guid;

    // Act - send action to modify state
    await Send(new BlueState.IncrementCountActionSet.Action { Amount = 5 });

    // Assert
    BlueState? previousState = Store.GetPreviousState<BlueState>();
    previousState.ShouldNotBeNull();
    previousState.Count.ShouldBe(initialCount);
    previousState.Guid.ShouldBe(initialGuid);
  }

  public async Task HaveNewGuid_AfterAction()
  {
    // Arrange
    Store.RemoveState<CounterState>();
    CounterState initialState = Store.GetState<CounterState>();
    Guid initialGuid = initialState.Guid;

    // Act - send action to modify state
    await Send(new CounterState.IncrementCountActionSet.Action { Amount = 1 });

    // Assert
    CounterState currentState = Store.GetState<CounterState>();
    currentState.Guid.ShouldNotBe(initialGuid);
  }
}
