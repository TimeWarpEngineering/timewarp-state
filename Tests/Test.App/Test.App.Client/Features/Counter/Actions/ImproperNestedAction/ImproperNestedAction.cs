namespace Test.App.Client.Features.Counter;

[UsedImplicitly]
public class WrongNesting
{
  [UsedImplicitly]
  public class ImproperNestedAction : IAction
  {
    // ReSharper disable once UnusedMember.Global
    public int Amount { get; set; }
  }
}
