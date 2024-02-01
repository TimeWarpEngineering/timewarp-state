namespace Test.App.Client.Features.Counter;

public class WrongNesting
{
  public class ImproperNestedAction : IAction
  {
    public int Amount { get; set; }
  }
}
