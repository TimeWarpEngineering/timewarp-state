namespace Test.App.Client.Features.WindowDimensions;

public sealed partial class WindowDimensionsState : State<WindowDimensionsState>
{
  public int Width { get; private set; }
  public int Height { get; private set; }

  public WindowDimensionsState() { }

  [JsonConstructor]
  public WindowDimensionsState(Guid guid, int width, int height)
  {
    Guid = guid;
    Width = width;
    Height = height;
  }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize()
  {
    Width = 1920; // Default width
    Height = 1080; // Default height
  }
}