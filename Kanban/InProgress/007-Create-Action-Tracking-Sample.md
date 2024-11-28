# Task 007: Create Sample App Demonstrating Action Tracking Feature

## Description

Create a new sample application based on Sample00Wasm that demonstrates how to use the Action Tracking Feature from TimeWarp.State.Plus. This sample will help developers understand how to utilize action tracking in their applications by providing a clear, well-documented example.

## Requirements

- Create a new sample application based on Sample00Wasm
- Create a new State (e.g., TaskDemoState) that implements actions to be tracked
- Demonstrate usage of existing ActionTrackingState from TimeWarp.State.Plus
- Show how to:
  - Create trackable actions
  - Use TrackActionAttribute
  - Display action tracking information
  - Handle multiple concurrent actions

## Checklist

### Design
- [ ] Review existing Sample00Wasm implementation
- [ ] Design new State for tracked actions
- [ ] Plan UI layout for action tracking display
- [ ] Design sample actions with varying durations

### Implementation
- [ ] Create new sample project based on Sample00Wasm
- [ ] Add TimeWarp.State.Plus package reference
- [ ] Create new State for tracked actions (e.g., TaskDemoState)
- [ ] Implement sample actions (TwoSecondAction, FiveSecondAction)
- [ ] Add TrackActionAttribute to actions
- [ ] Configure ActionTrackingBehavior in pipeline
- [ ] Create UI components for action tracking display
- [ ] Add action trigger buttons
- [ ] Implement action status display
- [ ] Add error handling for failed actions

### Documentation
- [ ] Add detailed comments explaining implementation
- [ ] Create README with setup instructions
- [ ] Document how to use action tracking
- [ ] Add usage examples
- [ ] Update main documentation to reference new sample

### Review
- [ ] Test with different action durations
- [ ] Verify action tracking behavior
- [ ] Ensure clear error handling
- [ ] Review code quality and comments
- [ ] Test build and run process

## Notes

Reference implementations:
- Tests/Test.App/Test.App.Client/Pages/ActiveActionsPage.razor
- Source/TimeWarp.State.Plus/Features/ActionTracking/ActionTrackingState/ActionTrackingState.cs
- Source/TimeWarp.State.Plus/Features/ActionTracking/Pipeline/ActionTrackingBehavior.cs
- Source/TimeWarp.State.Plus/Features/ActionTracking/Pipeline/TrackActionAttribute.cs

The sample should demonstrate:
- Creating actions that can be tracked
- Using TrackActionAttribute properly
- Displaying IsActive state
- Filtering active actions by type
- Handling multiple concurrent actions
- Proper error handling

## Implementation Notes

Key implementation considerations:
1. Base on Sample00Wasm structure
2. Create new State with trackable actions
3. Use existing ActionTrackingState from TimeWarp.State.Plus
4. Include both quick and long-running actions
5. Show proper error handling
6. Demonstrate concurrent action tracking
