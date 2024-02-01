namespace Test.App.Client.Features.Counter;

public partial class ColorState : State<ColorState>
{

  public int Count { get; private set; }

  public ColorState() { }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize() => Count = 3;
}
