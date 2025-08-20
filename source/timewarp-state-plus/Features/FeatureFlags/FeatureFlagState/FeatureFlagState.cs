namespace TimeWarp.State.Plus.Features.FeatureFlags.Actions;

public sealed class FeatureFlagState : State<FeatureFlagState>
{
  public FeatureFlagState(ISender sender) : base(sender) {}
  
  [JsonConstructor]
  public FeatureFlagState() {}
  
  public override void Initialize() => throw new NotImplementedException();
}
