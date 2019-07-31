namespace TestApp.Client.Integration.Tests.Pipeline
{
  using System;
  using System.Threading.Tasks;
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Features.CloneTest;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class CloneStateBehaviorTests
  {
    public CloneStateBehaviorTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
      CounterState = Store.GetState<CounterState>();
    }

    private CounterState CounterState { get; set; }
    private CloneTestState CloneTestState => Store.GetState<CloneTestState>();
    private IMediator Mediator { get; }
    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }

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
      // Send Request
      CounterState = await Mediator.Send(incrementCounterRequest);

      //Assert
      CounterState.Guid.ShouldNotBe(preActionGuid);
    }

    public async Task ShouldRollBackStateAndThrow()
    {
      // Arrange
      // Setup know state.
      CounterState.Initialize(aCount: 22);
      Guid preActionGuid = CounterState.Guid;

      // Act
      var throwExceptionAction = new ThrowExceptionAction
      {
        Message = "Test Rollback of State"
      };

      Exception exception = await Should.ThrowAsync<Exception>(async () =>
      CounterState = await Mediator.Send(throwExceptionAction));

      // Assert
      exception.Message.ShouldBe(throwExceptionAction.Message);
      CounterState = Store.GetState<CounterState>();
      CounterState.Guid.Equals(preActionGuid);
    }

    public async Task ShouldCloneStateUsingOverridenClone()
    {
      //Arrange
      CloneTestState.Initialize(aCount: 15);
      Guid preActionGuid = CloneTestState.Guid;

      // Create request
      var cloneTestAction = new CloneTestAction { };
      //Act
      // Send Request
      _ = await Mediator.Send(cloneTestAction);

      //Assert
      CloneTestState.Guid.ShouldNotBe(preActionGuid);
      CloneTestState.Count.ShouldBe(42);
    }

  }
}
