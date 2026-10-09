#region Purpose
// State holding the app's current theme (Light, Dark or System).
#endregion

#region Design
// Sealed partial state, split by action set. CurrentTheme has a private setter and Initialize defaults it to System.
#endregion

namespace TimeWarp.Features.Theme;

public sealed partial class ThemeState : State<ThemeState>
{
  public Theme CurrentTheme { get; private set; }

  public ThemeState(ISender<ClientPipeline> sender) : base(sender) {}
  
  [JsonConstructor]
  public ThemeState() {}
  
  public override void Initialize() => CurrentTheme = Theme.System;
  
  /// <summary>
  /// Represents the different themes that the app can have.
  /// </summary>
  public enum Theme
  {
    Light,
    Dark,
    System
  }
}
