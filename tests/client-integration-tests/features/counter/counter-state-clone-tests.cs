namespace CounterState;

using Test.App.Client.Features.Counter;

public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost webAssemblyHost) : base(webAssemblyHost)
  {
    CounterState = Store.GetState<CounterState>();
  }

  private CounterState CounterState { get; set; }

  public void Clone()
  {
    //Arrange
    CounterState.Initialize(count: 15);

    //Act
    CounterState? clone = CounterState.Clone();

    //Assert
    CounterState.ShouldNotBeSameAs(clone);
    CounterState.Count.ShouldBe(clone.Count);
    CounterState.Guid.ShouldNotBe(clone.Guid);
  }
}
