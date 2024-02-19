namespace Test.App.Client.Features.Purple;

using BlazorState.Features.Persistence.Attributes;
using System.Text.Json.Serialization;

[PersistentState(PersistentStateMethod.LocalStorage)]
public partial class PurpleState : State<PurpleState>
{
  public int Count { get; private set; }

  public PurpleState() { }

  [JsonConstructor]
  public PurpleState(Guid guid, int count)
  {
    Guid = guid;
    Count = count;
  }

  public override void Initialize()
  {
    Console.WriteLine("PurpleState.Initialize");
    Count = 1;
  }
}
