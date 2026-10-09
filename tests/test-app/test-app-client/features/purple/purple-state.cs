#region Purpose
// PurpleState: a counter persisted to local storage, used by the persistence tests.
#endregion

#region Design
// [PersistentState(LocalStorage)] opts it into PersistentStatePostProcessor; compare BlueState, which uses session
// storage. The [JsonConstructor] restores Count and Guid; Initialize sets Count to 1.
#endregion

namespace Test.App.Client.Features.Purple;

[PersistentState(PersistentStateMethod.LocalStorage)]
public sealed partial class PurpleState : State<PurpleState>
{
  public int Count { get; private set; }

  public PurpleState() { }

  [JsonConstructor]
  public PurpleState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  public override void Initialize() => Count = 1;
}
