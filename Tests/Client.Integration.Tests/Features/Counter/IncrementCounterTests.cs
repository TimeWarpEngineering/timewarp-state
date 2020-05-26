namespace TestApp.Client.Integration.Tests.Features.Counter_Tests
{
  using Shouldly;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.CounterState;

  public class IncrementCounterTests : BaseTest
  {
    private CounterState CounterState => Store.GetState<CounterState>();

    public IncrementCounterTests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

    public async Task Should_Decrement_Counter()
    {
      //Arrange 
      CounterState.Initialize(aCount: 15);

      var incrementCounterRequest = new IncrementCounterAction
      {
        Amount = -2
      };

      //Act
      await Send(incrementCounterRequest);

      //Assert
      CounterState.Count.ShouldBe(13);
    }

    public async Task Should_Increment_Counter()
    {
      //Arrange
      CounterState.Initialize(aCount: 22);

      var incrementCounterRequest = new IncrementCounterAction
      {
        Amount = 5
      };

      //Act
      await Send(incrementCounterRequest);

      //Assert
      CounterState.Count.ShouldBe(27);
    }
  }
}
