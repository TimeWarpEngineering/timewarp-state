namespace TestApp.Client.Pages
{
  using System;
  using BlazorState.Features.Routing;
  using System.Threading.Tasks;
  using Models;
  using TestApp.Client.Features.Base.Components;

  public class CounterPageBase : BaseComponent
  {
    public Record RecordForm { get; set; } = new Record();

    protected async Task ButtonClick() =>
      _ = await Mediator.Send(new RouteState.ChangeRouteAction { NewRoute = "/" });

    protected void HandleValidSubmit()
    {
      Console.WriteLine("Valid Submit");
    }
  }
}