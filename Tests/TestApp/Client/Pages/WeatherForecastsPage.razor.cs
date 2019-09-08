namespace TestApp.Client.Pages
{
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base.Components;
  using static TestApp.Client.Features.WeatherForecast.WeatherForecastsState;

  public class WeatherForecastsPageBase : BaseComponent
  {
    protected override async Task OnInitializedAsync() =>
      await Mediator.Send(new FetchWeatherForecastsAction());
  }
}