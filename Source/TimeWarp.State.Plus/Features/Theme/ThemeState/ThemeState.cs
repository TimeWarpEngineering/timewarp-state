namespace TimeWarp.Features.Theme;

public sealed partial class ThemeState : State<ThemeState>
{
  public Theme CurrentTheme { get; private set; }

  public ThemeState(ISender sender) : base(sender) {}
  public override void Initialize() => CurrentTheme = Theme.System;
  
  /// <summary>
  /// Represents the different themes that the app can have.
  /// </summary>
  public enum Theme
  {
    [UsedImplicitly] Light,
    [UsedImplicitly] Dark,
    [UsedImplicitly] System
  }
}
