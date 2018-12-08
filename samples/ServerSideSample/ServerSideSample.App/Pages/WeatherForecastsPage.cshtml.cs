namespace ServerSideSample.App.Pages
{
  using System.Threading.Tasks;
  using ServerSideSample.App.Components;

  public class WeatherForecastsPageModel : BaseComponent
  {
    protected override async Task OnInitAsync() =>
      await Mediator.Send(new Features.WeatherForecast.FetchWeatherForecastsRequest());
  }
}