namespace CounterState;

using Test.App.Client.Features.Counter;

[UsedImplicitly]
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
    CounterState.Should().NotBeSameAs(clone);
    CounterState.Count.Should().Be(clone.Count);
    CounterState.Guid.Should().NotBe(clone.Guid);
  }
}
