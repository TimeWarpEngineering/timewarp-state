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
    PolicyDefinition policy = Policies.CreateActionHandlerPolicy(sut);
    PolicyResults results = policy.Evaluate();
    results.ShouldBeSuccessful();
  }
}
