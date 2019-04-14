namespace TestApp.Client.Pages
{
  using System.Threading.Tasks;
  using TestApp.Client.Components;

  public class WeatherForecastsPageModel : BaseComponent
  {
    protected override async Task OnInitAsync() =>
      await Mediator.Send(new Features.WeatherForecast.FetchWeatherForecastsRequest());
  }
}