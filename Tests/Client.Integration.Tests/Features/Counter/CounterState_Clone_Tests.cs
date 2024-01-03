namespace CounterState;

using AnyClone;
using FluentAssertions;
using TestApp.Client.Features.Counter;
using TestApp.Client.Integration.Tests.Infrastructure;

public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
    CounterState = Store.GetStateAsync<CounterState>();
  }

  private CounterState CounterState { get; set; }

  public void Clone()
  {
    //Arrange
    CounterState.Initialize(aCount: 15);

    //Act
    var clone = CounterState.Clone() as CounterState;

    //Assert
    CounterState.Should().NotBeSameAs(clone);
    CounterState.Count.Should().Be(clone.Count);
    CounterState.Guid.Should().NotBe(clone.Guid);
  }

}
