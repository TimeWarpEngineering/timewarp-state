namespace CounterState
{
  using Shouldly;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.CounterState;

  public class IncrementCounterAction_Should : BaseTest
  {
    private CounterState CounterState => Store.GetState<CounterState>();

    public IncrementCounterAction_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

    public async Task Decrement_Count_Given_NegativeAmount()
    {
      //Arrange 
      CounterState.Initialize(aCount: 15);

      var incrementCounterAction = new IncrementCounterAction
      {
        Amount = -2
      };

      //Act
      await Send(incrementCounterAction);

      //Assert
      CounterState.Count.ShouldBe(13);
    }

    public async Task Increment_Count()
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
