namespace EventStreamState
{
  using AnyClone;
  using Shouldly;
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
      EventStreamState.Events.Count.ShouldBe(clone.Events.Count);
      EventStreamState.Guid.ShouldNotBe(clone.Guid);
      EventStreamState.Events[0].ShouldBe(clone.Events[0]);
    }

  }
}