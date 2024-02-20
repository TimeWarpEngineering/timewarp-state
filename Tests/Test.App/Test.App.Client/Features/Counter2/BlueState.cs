namespace Test.App.Client.Features.Blue;

[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class BlueState : State<BlueState>
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
