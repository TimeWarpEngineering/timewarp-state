namespace Test.App.Client.Features.EventStream;

public partial class EventStreamState
{
  internal void Initialize(List<string> events)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    EventList = events;
  }
}
