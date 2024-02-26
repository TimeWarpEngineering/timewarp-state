namespace Test.App.Client.Features.Theme;

public partial class ThemeState
{
  public override ThemeState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    string currentThemeKey = CamelCase.MemberNameToCamelCase(nameof(CurrentTheme));
    string guidKey = CamelCase.MemberNameToCamelCase(nameof(Guid));

    if (!keyValuePairs.TryGetValue(currentThemeKey, out object? themeValue))
    {
      throw new InvalidOperationException($"Expected key '{currentThemeKey}' not found or value is null.");
    }

    if (!keyValuePairs.TryGetValue(guidKey, out object? guidValue))
    {
      throw new InvalidOperationException($"Expected key '{guidKey}' not found or value is null.");
    }

    var themeState = new ThemeState
    {
      CurrentTheme = (Theme)Enum.Parse(typeof(Theme), themeValue.ToString()!, true),
      Guid = Guid.Parse(guidValue.ToString()!)
    };

    return themeState;
  }

  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="currentTheme"></param>
  [UsedImplicitly] 
  public void Initialize(Theme currentTheme)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    CurrentTheme = currentTheme;
  }
}
