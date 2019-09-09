namespace TestApp.Client.Integration.Tests.Pipeline
{
  using BlazorState;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.CloneTest;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.CloneTest.CloneTestState;
  using static TestApp.Client.Features.Counter.CounterState;

  internal class CloneStateBehaviorTests
  {
    private readonly IMediator Mediator;
    private readonly IStore Store;

    private CloneTestState CloneTestState => Store.GetState<CloneTestState>();
    private CounterState CounterState => Store.GetState<CounterState>();

    public CloneStateBehaviorTests(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      Mediator = serviceProvider.GetService<IMediator>();
      Store = serviceProvider.GetService<IStore>();
    }

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
      _ = await Mediator.Send(incrementCounterRequest);

      //Assert
      CounterState.Guid.ShouldNotBe(preActionGuid);
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
      _ = await Mediator.Send(throwExceptionAction));

      // Assert
      exception.Message.ShouldBe(throwExceptionAction.Message);
      CounterState.Guid.Equals(preActionGuid);
    }
  }
}