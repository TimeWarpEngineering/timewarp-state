using System.Threading.Tasks;
using BlazorState.Client.Features.Counter;
using BlazorState.Integration.Tests.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace BlazorState.Client.Integration.Tests.Features.Counter
{
  class IncrementCounterTests
  {
    public IncrementCounterTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
      CounterState = Store.GetState<CounterState>();
    }

    private ServiceProvider ServiceProvider { get; }
    private IMediator Mediator { get; }
    private IStore Store { get; }
    private CounterState CounterState { get; set; }

    public async Task Should_Increment_Counter()
    {
      //Arrange

      // Setup know state.
      // Given our TestFixture the state defaults to the InitialState.
      // IStore store = ServiceProvider.GetService<IStore>();

      CounterState.Initialize(aCount: 22);

      // Create request
      var incrementCounterRequest = new BlazorState.Client.Features.Counter.IncrementCount.IncrementCounterRequest
      {
        Amount = 5
      };
      //Act
      // Send Request
      CounterState = await Mediator.Send(incrementCounterRequest);

      //Assert
      CounterState.Count.ShouldBe(27);
    }

    public async Task Should_Decrement_Counter()
    {
      //Arrange
      CounterState.Initialize(aCount: 15);

      // Create request
      var incrementCounterRequest = new BlazorState.Client.Features.Counter.IncrementCount.IncrementCounterRequest
      {
        Amount = -2
      };
      //Act
      // Send Request
      CounterState = await Mediator.Send(incrementCounterRequest);

      //Assert
      CounterState.Count.ShouldBe(13);
    }
  }
}
