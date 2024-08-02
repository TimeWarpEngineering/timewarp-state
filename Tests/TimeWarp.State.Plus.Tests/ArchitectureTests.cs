namespace Architecture_;

public class Should_
{
  public static void FollowActionPolicy()
  {
    Assembly sut = typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly;
    PolicyDefinition policy = Policies.CreateActionPolicy(sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
  
  public static void FollowActionHandlerPolicy()
  {
    Assembly sut = typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly;
    PolicyDefinition policy = Policies.CreateActionHandlerPolicy(sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
}
