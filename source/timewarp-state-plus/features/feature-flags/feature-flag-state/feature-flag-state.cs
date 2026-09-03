namespace TimeWarp.State.Plus.Features.FeatureFlags.Actions;

public sealed class FeatureFlagState : State<FeatureFlagState>
{
  public FeatureFlagState(ISender<ClientPipeline> sender) : base(sender) {}
  
  [JsonConstructor]
  public FeatureFlagState() {}
  
  public override void Initialize() => throw new NotImplementedException();
}
