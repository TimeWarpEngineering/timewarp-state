namespace Test.App.Client.Features.Counter;

using System.Drawing;

public sealed partial class ColorState : State<ColorState>
{
  public string MyColorName { get; private set; } = null!;

  public Color FavoriteColor { get; private set; }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize()
  {
    MyColorName = "Fools Gold";
    FavoriteColor = Color.Gold;
  }
}
