namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  public class WrongNesting
  {
    public class ImproperNestedAction : IAction
    {
      public int Amount { get; set; }
    }
  }
}