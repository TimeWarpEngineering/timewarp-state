namespace TestApp.Client.Integration.Tests.Features.Counter
{
  using System;
  using TestApp.Client.Features.Counter;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class CounterStateCloneTests
  {
    public CounterStateCloneTests(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      IStore store = serviceProvider.GetService<IStore>();
      CounterState = store.GetState<CounterState>();
    }

    private CounterState CounterState { get; set; }

    public void ShouldClone()
    {
      //Arrange
      CounterState.Initialize(aCount: 15);
      
      //Act
      var clone = CounterState.Clone() as CounterState;

      //Assert
      CounterState.Count.ShouldBe(clone.Count);
      CounterState.Guid.ShouldNotBe(clone.Guid);
    }

  }
}
