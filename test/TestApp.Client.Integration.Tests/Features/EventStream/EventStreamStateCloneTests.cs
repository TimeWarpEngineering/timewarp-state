namespace TestApp.Client.Integration.Tests.Features.EventStream
{
  using System;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using TestApp.Client.Features.EventStream;
  using System.Collections.Generic;
  using AnyClone;

  internal class EventStreamCloneTests
  {
    public EventStreamCloneTests(TestFixture aTestFixture)
    {
      IServiceProvider serviceProvider = aTestFixture.ServiceProvider;
      IStore store = serviceProvider.GetService<IStore>();
      EventStreamState = store.GetState<EventStreamState>();
    }

    private EventStreamState EventStreamState { get; set; }

    public void ShouldClone()
    {
      //Arrange
      var events = new List<string> { "Event 1", "Event 2", "Event 3" };
      EventStreamState.Initialize(events);
      
      //Act
      var clone = EventStreamState.Clone() as EventStreamState;

      //Assert
      EventStreamState.Events.Count.ShouldBe(clone.Events.Count);
      EventStreamState.Guid.ShouldNotBe(clone.Guid);
      EventStreamState.Events[0].ShouldBe(clone.Events[0]);
    }

  }
}
