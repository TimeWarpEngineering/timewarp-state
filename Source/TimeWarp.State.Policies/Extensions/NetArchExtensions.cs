namespace TimeWarp.State.Policies.Extensions;

public static class NetArchExtensions
{
  public static void ShouldBeSuccessful(this TestResult result)
  {
    result
      .IsSuccessful
      .Should()
      .BeTrue(string.Join('\n',result.FailingTypes.Select(t => t.FullName)));
  }

  public static void ShouldBeSuccessful(this PolicyResults results)
  {
    ArgumentNullException.ThrowIfNull(results);
    
    results.HasViolations.Should().BeFalse(BuildMessage(results));
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
