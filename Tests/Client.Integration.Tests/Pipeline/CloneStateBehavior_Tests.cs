namespace CloneStateBehavior
{
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Features.CloneTest;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.CloneTest.CloneTestState;
  using static TestApp.Client.Features.Counter.CounterState;

  public class Should : BaseTest
  {
    private CloneTestState CloneTestState => Store.GetState<CloneTestState>();
    private CounterState CounterState => Store.GetState<CounterState>();
    private ApplicationState ApplicationState => Store.GetState<ApplicationState>();

    public Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

    public async Task CloneState()
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

    public async Task CloneStateUsingOverridenClone()
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

    public async Task RollBackStateAndPublish_When_Exception()
    {
      // Arrange
      // Setup know state.
      CounterState.Initialize(aCount: 22);
      ApplicationState.Initialize("anyname", "");
      Guid preActionGuid = CounterState.Guid;

      var throwExceptionAction = new ThrowExceptionAction
      {
        Message = "Test Rollback of State"
      };

      // Act
      await Send(throwExceptionAction).ConfigureAwait(false);

      // Assert
      ApplicationState.ExceptionMessage.ShouldBe(throwExceptionAction.Message);
      CounterState.Guid.Equals(preActionGuid).ShouldBeTrue();
    }

  }
}