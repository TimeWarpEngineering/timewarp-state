namespace TestApp.Client.Integration.Tests.Pipeline_Tests
{
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.CloneTest;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.CloneTest.CloneTestState;
  using static TestApp.Client.Features.Counter.CounterState;

  internal class CloneStateBehaviorTests : BaseTest
  {
    private CloneTestState CloneTestState => Store.GetState<CloneTestState>();
    private CounterState CounterState => Store.GetState<CounterState>();

    public CloneStateBehaviorTests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

    public async Task ShouldCloneState()
    {
      //Arrange
      CounterState.Initialize(aCount: 15);
      Guid preActionGuid = CounterState.Guid;

      // Create request
      var incrementCounterRequest = new IncrementCounterAction
      {
        Amount = -2
      };
      //Act
      await Send(incrementCounterRequest);

      //Assert
      CounterState.Guid.ShouldNotBe(preActionGuid);
    }

    public async Task ShouldCloneStateUsingOverridenClone()
    {
      //Arrange
      CloneTestState.Initialize(aCount: 15);
      Guid preActionGuid = CloneTestState.Guid;

      var cloneTestAction = new CloneTestAction { };
      //Act
      await Send(cloneTestAction);

      //Assert
      CloneTestState.Guid.ShouldNotBe(preActionGuid);
      CloneTestState.Count.ShouldBe(42);
    }

    public async Task ShouldRollBackStateAndThrow()
    {
      // Arrange
      // Setup know state.
      CounterState.Initialize(aCount: 22);
      Guid preActionGuid = CounterState.Guid;

      var throwExceptionAction = new ThrowExceptionAction
      {
        Message = "Test Rollback of State"
      };

      // Act
      Exception exception = await Should.ThrowAsync<Exception>(async () =>
      await Send(throwExceptionAction));

      // Assert
      exception.Message.ShouldBe(throwExceptionAction.Message);
      CounterState.Guid.Equals(preActionGuid);
    }
  }
}