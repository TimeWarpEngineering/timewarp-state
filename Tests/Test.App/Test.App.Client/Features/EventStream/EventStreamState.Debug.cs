namespace Test.App.Client.Features.EventStream;

internal partial class EventStreamState
{
  [UsedImplicitly]
  internal void Initialize(List<string> events)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    EventList = events;
  }
}
