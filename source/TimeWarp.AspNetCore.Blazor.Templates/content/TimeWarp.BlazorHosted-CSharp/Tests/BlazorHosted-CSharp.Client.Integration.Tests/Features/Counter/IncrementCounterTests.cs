namespace BlazorHosted_CSharp.Client.Integration.Tests.Features.Counter
{
  using System;
  using System.Threading.Tasks;
  using BlazorHosted_CSharp.Client.Features.Counter;
  using BlazorState;
  using BlazorState.Integration.Tests.Infrastructure;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;

  internal class IncrementCounterTests
  {
    public IncrementCounterTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
      CounterState = Store.GetState<CounterState>();
    }

    private CounterState CounterState { get; set; }
    private IMediator Mediator { get; }
    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }

    public async Task Should_Decrement_Counter()
    {
      //Arrange
      CounterState.Initialize(aCount: 15);

      // Create request
      var incrementCounterRequest = new Client.Features.Counter.IncrementCount.IncrementCounterAction
      {
        Amount = -2
      };
      //Act
      // Send Request
      CounterState = await Mediator.Send(incrementCounterRequest);

      //Assert
      CounterState.Count.ShouldBe(13);
    }

    public async Task Should_Increment_Counter()
    {
      //Arrange

      // Setup know state.
      CounterState.Initialize(aCount: 22);

      // Create request
      var incrementCounterRequest = new Client.Features.Counter.IncrementCount.IncrementCounterAction
      {
        Amount = 5
      };
      //Act
      CounterState = await Mediator.Send(incrementCounterRequest);

      //Assert
      CounterState.Count.ShouldBe(27);
    }
  }
}
