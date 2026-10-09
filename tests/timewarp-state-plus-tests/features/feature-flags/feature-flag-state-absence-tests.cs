#region Purpose
// Proves the public TimeWarp.State.Plus assembly no longer contains FeatureFlagState.
#endregion

#region Design
// The type was an unimplemented placeholder whose Initialize threw. Removal is the 12.0 beta choice: nothing in
// the repo referenced it, and UseFeatureFlags was never implemented.
#endregion

namespace FeatureFlagState_;

public class Should_
{
  public static void Be_Absent_From_TimeWarp_State_Plus()
  {
    Assembly assembly = typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly;

    assembly.GetType("TimeWarp.State.Plus.Features.FeatureFlags.Actions.FeatureFlagState").ShouldBeNull();
    assembly.GetTypes().Any(type => type.Name == "FeatureFlagState").ShouldBeFalse();
  }
}
