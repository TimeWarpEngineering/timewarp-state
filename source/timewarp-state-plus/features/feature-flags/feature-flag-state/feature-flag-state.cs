#region Purpose
// Placeholder state for a feature-flags feature.
#endregion

#region Design
// Not implemented yet: Initialize throws NotImplementedException. It only has the standard DI and [JsonConstructor]
// constructors.
#endregion

namespace TimeWarp.State.Plus.Features.FeatureFlags.Actions;

public sealed class FeatureFlagState : State<FeatureFlagState>
{
  public FeatureFlagState(ISender<ClientPipeline> sender) : base(sender) {}
  
  [JsonConstructor]
  public FeatureFlagState() {}
  
  public override void Initialize() => throw new NotImplementedException();
}
