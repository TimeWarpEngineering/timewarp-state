namespace BlazorState.Unit.Tests
{
  using System;
  using System.Runtime.Serialization;
  using Shouldly;

  /// <remarks>This is an abstract class so we need to create a derived class
  /// to test it.  The only method that does anything is Hydrate and it is a
  /// direct call to JsonUtil.Deserialize.  It is tested in that package
  /// extensively.  One test just to show we thought about it.
  /// </remarks>
  public class StateTests
  {
    public void ShouldThrow()
    {
      var state = new SomeState();
      Should.Throw<SerializationException>(() => { state.Hydrate(string.Empty); });
    }
  }

  internal class SomeState : State<SomeState>
  {
    public DateTime SomeDateTime { get; set; }
    public int SomeInt { get; set; }
    public string SomeString { get; set; }

    public override object Clone() => new SomeState();

    protected override void Initialize() { }
  }
}