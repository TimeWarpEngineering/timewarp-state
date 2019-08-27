namespace TestApp.Client.Pages
{
  using System.Threading.Tasks;
  using TestApp.Client.Features.Base.Components;

  public class WeatherForecastsPageBase : BaseComponent
  {
    protected override async Task OnInitializedAsync() =>
      await Mediator.Send(new Features.WeatherForecast.FetchWeatherForecastsAction());
  }
}