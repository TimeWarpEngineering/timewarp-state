namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  public class NonNestedAction : IAction
  {
    public int Amount { get; set; }
  }
}