#region Purpose
// Checks that TimeWarp.State.Plus follows the TimeWarp.State action, handler and state policies.
#endregion

#region Design
// Uses TimeWarp.State.Policies (NetArchTest) with the default handler policy, unlike the test app's architecture
// tests, which pass requirePublicHandlers: false.
#endregion

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
  
  public static void FollowStatePolicy()
  {
    Assembly sut = typeof(TimeWarp.State.Plus.AssemblyMarker).Assembly;
    PolicyDefinition policy = Policies.CreateStatePolicy(sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
}
