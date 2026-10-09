#region Purpose
// BlueState: a counter persisted to session storage, used by persistence, subscription and deserialization tests.
#endregion

#region Design
// [PersistentState(SessionStorage)] opts it into PersistentStatePostProcessor. The [JsonConstructor] (guid, count)
// lets System.Text.Json rebuild it from storage with its Guid; Initialize sets Count to 2.
#endregion

namespace Test.App.Client.Features.Blue;

[PersistentState(PersistentStateMethod.SessionStorage)]
public sealed partial class BlueState : State<BlueState>
{
  public int Count { get; private set; }

  public BlueState() { }

  [JsonConstructor]
  public BlueState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  public override void Initialize() => Count = 2;
}
