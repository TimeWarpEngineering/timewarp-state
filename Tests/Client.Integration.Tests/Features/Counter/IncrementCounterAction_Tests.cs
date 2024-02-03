namespace CounterState_;

public class IncrementCounterAction_Should
(
  ClientHost aWebAssemblyHost
) : BaseTest(aWebAssemblyHost)
{
  private CounterState CounterState => Store.GetState<CounterState>();

  public async Task Decrement_Count_Given_NegativeAmount()
  {
    //Arrange 
    CounterState.Initialize(aCount: 15);

    var incrementCounterAction = new CounterState.IncrementCounterAction
    {
      Amount = -2
    };

    //Act
    await Send(incrementCounterAction);

    //Assert
    CounterState.Count.Should().Be(13);
  }

  public async Task Increment_Count()
  {
    //Arrange
    CounterState.Initialize(aCount: 22);

    var incrementCounterRequest = new CounterState.IncrementCounterAction
    {
      Amount = 5
    };

    //Act
    await Send(incrementCounterRequest);

    //Assert
    CounterState.Count.Should().Be(27);
  }
}
