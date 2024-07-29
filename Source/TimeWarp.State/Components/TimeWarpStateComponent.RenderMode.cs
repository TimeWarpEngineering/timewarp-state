namespace TimeWarp.State;

public partial class TimeWarpStateComponent
{
  private static readonly ConcurrentDictionary<Type, string> ConfiguredRenderModeCache = new();
  private static readonly ConcurrentDictionary<Type, bool> TypeRenderAttributeCache = new();

  private bool UsesRenderMode;
  private bool HasRendered;

  protected string ConfiguredRenderMode =>
    ConfiguredRenderModeCache.GetOrAdd(this.GetType(), type =>
    {
      // Use reflection to get all attributes on the current component type.
      object[] attributes = type.GetCustomAttributes(true);

      foreach (object attribute in attributes)
      {
        // Check if the type name of the attribute contains the expected name.
        Type attributeType = attribute.GetType();
        if (attributeType.Name.Contains("PrivateComponentRenderModeAttribute"))
        {
          // Try to get the 'Mode' property value of the attribute.
          PropertyInfo? modeProperty = attributeType.GetProperty("Mode");
          if (modeProperty != null)
          {
            // Use dynamic to bypass compile-time type checking
            dynamic? modeValue = modeProperty.GetValue(attribute);
            // Return the type name of the Mode property's value.
            return modeValue == null ? "None" : modeValue.GetType().Name;
          }
        }
      }

      // If no matching attribute is found, return a default identifier.
      return "None";// Adjust as needed for your default case.
    });

  /// <summary>
  ///   Indicates if the component is being prerendered.
  /// </summary>
  protected bool IsPreRendering => GetCurrentRenderMode() == State.CurrentRenderMode.PreRendering;
  protected string CurrentRenderMode => GetCurrentRenderMode().ToString();

  private CurrentRenderMode GetCurrentRenderMode()
  {
    UsesRenderMode = true;
    if (OperatingSystem.IsBrowser()) return State.CurrentRenderMode.Wasm;

    if (HasRendered) return State.CurrentRenderMode.Server;

    bool hasRenderAttribute = TypeRenderAttributeCache.GetOrAdd(this.GetType(), type =>
      type.GetCustomAttributes(true)
        .Any(attr => attr.GetType().Name.Contains("PrivateComponentRenderModeAttribute")));

    return hasRenderAttribute ? State.CurrentRenderMode.PreRendering : State.CurrentRenderMode.Static;
  }
}

public enum CurrentRenderMode
{
  Static,
  PreRendering,
  Server,
  Wasm,
}
