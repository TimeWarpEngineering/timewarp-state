#region Purpose
// Checks that Test.App.Client follows the TimeWarp.State action, handler and state policies.
#endregion

#region Design
// Uses TimeWarp.State.Policies (NetArchTest) against the test app assembly. Handlers are allowed to be non-public
// (requirePublicHandlers: false) because the test app declares them internal.
#endregion

namespace Architecture_;

public class Should_
{
  public static void FollowActionPolicy()
  {
    Assembly sut = typeof(Test.App.Client.AssemblyMarker).Assembly;
    PolicyDefinition policy = Policies.CreateActionPolicy(sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
  
  public static void FollowActionHandlerPolicy()
  {
    Assembly sut = typeof(Test.App.Client.AssemblyMarker).Assembly;
    PolicyDefinition policy = Policies.CreateActionHandlerPolicy(requirePublicHandlers: false, sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
  
  public static void FollowStatePolicy()
  {
    Assembly sut = typeof(Test.App.Client.AssemblyMarker).Assembly;
    PolicyDefinition policy = Policies.CreateStatePolicy(sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
}
