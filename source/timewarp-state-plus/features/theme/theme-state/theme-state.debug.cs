#region Purpose
// Test-only Initialize for ThemeState.
#endregion

#region Design
// Initialize is guarded by ThrowIfNotTestAssembly.
#endregion

namespace TimeWarp.Features.Theme;

public partial class ThemeState
{
  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="currentTheme"></param>
  public void Initialize(Theme currentTheme)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    CurrentTheme = currentTheme;
  }
}
