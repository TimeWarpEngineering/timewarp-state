#region Purpose
// Test-only Initialize that seeds EventStreamState's event list.
#endregion

#region Design
// Guarded by ThrowIfNotTestAssembly so it is test-only; there is no Hydrate override.
#endregion

namespace Test.App.Client.Features.EventStream;

public partial class EventStreamState
{
  internal void Initialize(List<string> events)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    EventList = events;
  }
}
