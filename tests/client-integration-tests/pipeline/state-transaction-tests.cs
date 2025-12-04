namespace StateTransactionTests;

using Counter = Test.App.Client.Features.Counter;

/// <summary>
/// Integration tests for StateTransactionBehavior.
/// Verifies cloning, rollback on exception, and exception notification publishing.
/// Tests run through real mediator pipeline.
/// </summary>
public class StateTransaction_Should : BaseTest
{
  public StateTransaction_Should(ClientHost clientHost) : base(clientHost) { }

  public async Task CloneState_BeforeHandlerExecutes()
  {
    // Arrange
    Store.RemoveState<Counter.CounterState>();
    Counter.CounterState initialState = Store.GetState<Counter.CounterState>();
    Guid initialGuid = initialState.Guid;

    // Act - send action that modifies state
    await Send(new Counter.CounterState.IncrementCountActionSet.Action { Amount = 5 });

    // Assert - state should have new Guid (was cloned)
    Counter.CounterState currentState = Store.GetState<Counter.CounterState>();
    currentState.Guid.ShouldNotBe(initialGuid);
  }

  public async Task PreserveChanges_OnSuccessfulAction()
  {
    // Arrange
    Store.RemoveState<Counter.CounterState>();
    Counter.CounterState initialState = Store.GetState<Counter.CounterState>();
    int initialCount = initialState.Count; // Should be 3 from Initialize()

    // Act
    await Send(new Counter.CounterState.IncrementCountActionSet.Action { Amount = 7 });

    // Assert - changes should be preserved
    Counter.CounterState currentState = Store.GetState<Counter.CounterState>();
    currentState.Count.ShouldBe(initialCount + 7);
  }

  public async Task RollbackState_OnException()
  {
    // Arrange
    Store.RemoveState<Counter.CounterState>();
    Counter.CounterState initialState = Store.GetState<Counter.CounterState>();
    int initialCount = initialState.Count;
    Guid initialGuid = initialState.Guid;

    // First, increment to prove state changes work
    await Send(new Counter.CounterState.IncrementCountActionSet.Action { Amount = 10 });
    Counter.CounterState stateAfterIncrement = Store.GetState<Counter.CounterState>();
    stateAfterIncrement.Count.ShouldBe(initialCount + 10);
    Guid guidAfterIncrement = stateAfterIncrement.Guid;

    // Act - send action that throws exception
    await Send(new Counter.CounterState.ThrowExceptionActionSet.Action("Test exception for rollback"));

    // Assert - state should be rolled back to state before the throwing action
    Counter.CounterState currentState = Store.GetState<Counter.CounterState>();
    currentState.Count.ShouldBe(initialCount + 10); // Should still have the increment
    currentState.Guid.ShouldBe(guidAfterIncrement); // Should be same Guid as before exception
  }

  public async Task PublishExceptionNotification_OnException()
  {
    // Arrange
    Store.RemoveState<ApplicationState>();
    ApplicationState applicationState = Store.GetState<ApplicationState>();
    applicationState.ExceptionMessage.ShouldBeNull(); // Initially null

    const string exceptionMessage = "Test exception message for notification";

    // Act - send action that throws exception
    await Send(new Counter.CounterState.ThrowExceptionActionSet.Action(exceptionMessage));

    // Assert - ApplicationState should have captured the exception message
    // (via ExceptionNotificationHandler)
    ApplicationState updatedApplicationState = Store.GetState<ApplicationState>();
    updatedApplicationState.ExceptionMessage.ShouldBe(exceptionMessage);
  }

  public async Task ContinueWorking_AfterException()
  {
    // Arrange
    Store.RemoveState<Counter.CounterState>();
    Counter.CounterState initialState = Store.GetState<Counter.CounterState>();
    int initialCount = initialState.Count;

    // Act - first throw an exception
    await Send(new Counter.CounterState.ThrowExceptionActionSet.Action("Exception that should not break subsequent actions"));

    // Then send a successful action
    await Send(new Counter.CounterState.IncrementCountActionSet.Action { Amount = 3 });

    // Assert - state should reflect the successful action
    Counter.CounterState currentState = Store.GetState<Counter.CounterState>();
    currentState.Count.ShouldBe(initialCount + 3);
  }
}
