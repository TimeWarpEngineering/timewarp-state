#region Purpose
// Shouldly assertions for NetArchTest results (ShouldBeSuccessful on TestResult and PolicyResults).
#endregion

#region Design
// Failure messages list the policy, rule and failing type names, so a test failure says exactly which convention
// was broken.
#endregion

namespace TimeWarp.State.Policies.Extensions;

public static class NetArchExtensions
{
  public static void ShouldBeSuccessful(this TestResult result)
  {
    result
      .IsSuccessful
      .ShouldBeTrue(string.Join('\n',result.FailingTypes.Select(t => t.FullName)));
  }

  public static void ShouldBeSuccessful(this PolicyResults results)
  {
    ArgumentNullException.ThrowIfNull(results);
    
    results.HasViolations.ShouldBeFalse(BuildMessage(results));
    return;

    string BuildMessage(PolicyResults policyResults)
    {
      StringBuilder builder = new();
      
      foreach (PolicyResult? result in policyResults.Results.Where(r =>!r.IsSuccessful))
      {
        builder.AppendLine();
        builder.Append("Policy Name:");
        builder.AppendLine(policyResults.Name);
        builder.Append("Description:");
        builder.AppendLine(policyResults.Description);
        builder.Append("Rule Name: ");
        builder.AppendLine(result.Name);
        builder.Append("Rule Description: ");
        builder.AppendLine(result.Description);
        builder.AppendLine("Failing Types: ");
        builder.AppendLine(string.Join('\n', result.FailingTypes.Select(t => t.FullName)));
      }
      return builder.ToString();
    }
  }
}
