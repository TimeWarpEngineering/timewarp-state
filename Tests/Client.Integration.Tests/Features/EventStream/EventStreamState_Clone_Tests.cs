namespace EventStreamState;

using AnyClone;
  using FluentAssertions;
using System.Collections.Generic;
using TestApp.Client.Features.EventStream;
using TestApp.Client.Integration.Tests.Infrastructure;

public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
    EventStreamState = Store.GetState<EventStreamState>();
  }

  private EventStreamState EventStreamState { get; set; }

  public void Clone()
  {
    //Arrange
    var events = new List<string> { "Event 1", "Event 2", "Event 3" };
    EventStreamState.Initialize(events);

    //Act
    var clone = EventStreamState.Clone() as EventStreamState;

    //Assert
    EventStreamState.Events.Count.Should().Be(clone.Events.Count);
    EventStreamState.Guid.Should().NotBe(clone.Guid);
    EventStreamState.Events[0].Should().Be(clone.Events[0]);
  }

}
