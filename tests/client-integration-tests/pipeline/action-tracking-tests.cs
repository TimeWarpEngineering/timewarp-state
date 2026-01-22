namespace ActionTrackingTests;

/// <summary>
/// Integration tests for ActionTrackingState and ActiveActionBehavior.
/// Tests verify action tracking lifecycle through real pipeline.
/// </summary>
public class ActionTracking_Should : BaseTest
{
  public ActionTracking_Should(ClientHost clientHost) : base(clientHost) { }

  private ActionTrackingState ActionTrackingState => Store.GetState<ActionTrackingState>();

  public void NotBeActive_Initially()
  {
    // Arrange - ensure fresh state
    Store.RemoveState<ActionTrackingState>();

    // Act
    ActionTrackingState state = Store.GetState<ActionTrackingState>();

    // Assert
    state.IsActive.ShouldBeFalse();
    state.ActiveActions.ShouldBeEmpty();
  }

  public async Task TrackAction_DuringExecution()
  {
    // Arrange
    Store.RemoveState<ActionTrackingState>();
    ActionTrackingState.IsActive.ShouldBeFalse();
    
    // Act - start action without awaiting to check tracking during execution
    // We use a short delay action that has [TrackAction] attribute
    Task actionTask = Send(new ApplicationState.TwoSecondTaskActionSet.Action());
    
    // Give it a moment to start
    await Task.Delay(100);
    
    // Assert - should be active during execution
    ActionTrackingState.IsActive.ShouldBeTrue();
    ActionTrackingState.IsAnyActive(typeof(ApplicationState.TwoSecondTaskActionSet.Action)).ShouldBeTrue();
    
    // Wait for completion
    await actionTask;
    
    // Assert - should no longer be active after completion
    ActionTrackingState.IsActive.ShouldBeFalse();
  }

  public void ReturnFalse_ForUnknownActionType()
  {
    // Arrange
    Store.RemoveState<ActionTrackingState>();
    
    // Act & Assert - checking for an action type that's not active
    ActionTrackingState.IsAnyActive(typeof(BlueState.IncrementCountActionSet.Action)).ShouldBeFalse();
  }

  public async Task TrackMultipleActions_Independently()
  {
    // Arrange
    Store.RemoveState<ActionTrackingState>();
    ActionTrackingState.IsActive.ShouldBeFalse();
    
    // Act - start two actions without awaiting
    Task twoSecondTask = Send(new ApplicationState.TwoSecondTaskActionSet.Action());
    Task fiveSecondTask = Send(new ApplicationState.FiveSecondTaskActionSet.Action());
    
    // Give them a moment to start
    await Task.Delay(100);
    
    // Assert - both should be tracked
    ActionTrackingState.IsActive.ShouldBeTrue();
    ActionTrackingState.ActiveActions.Count.ShouldBe(2);
    ActionTrackingState.IsAnyActive(typeof(ApplicationState.TwoSecondTaskActionSet.Action)).ShouldBeTrue();
    ActionTrackingState.IsAnyActive(typeof(ApplicationState.FiveSecondTaskActionSet.Action)).ShouldBeTrue();
    
    // Wait for two-second task to complete
    await twoSecondTask;
    
    // Assert - only five-second task should still be active
    ActionTrackingState.IsActive.ShouldBeTrue();
    ActionTrackingState.ActiveActions.Count.ShouldBe(1);
    ActionTrackingState.IsAnyActive(typeof(ApplicationState.TwoSecondTaskActionSet.Action)).ShouldBeFalse();
    ActionTrackingState.IsAnyActive(typeof(ApplicationState.FiveSecondTaskActionSet.Action)).ShouldBeTrue();
    
    // Wait for five-second task to complete
    await fiveSecondTask;
    
    // Assert - neither should be active now
    ActionTrackingState.IsActive.ShouldBeFalse();
    ActionTrackingState.ActiveActions.ShouldBeEmpty();
  }
}
