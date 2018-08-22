namespace BlazorState.Client.Pages
{
  using System.Threading.Tasks;
  using BlazorState.Client.Components;

  public class WeatherForecastsPageModel : BaseComponent
  {
    protected override async Task OnInitAsync() =>
      await Mediator.Send(new Features.WeatherForecast.FetchWeatherForecastsRequest());
  }
}
