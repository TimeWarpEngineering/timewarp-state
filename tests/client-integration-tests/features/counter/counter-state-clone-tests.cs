#region Purpose
// Checks that cloning CounterState copies Count and gives the clone a new Guid.
#endregion

#region Design
// Seeds Count with the test-only Initialize(count) and calls the Clone() extension directly.
#endregion

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
