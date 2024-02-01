namespace Test.App.Client.Pages;

using System.Threading.Tasks;
using Test.App.Client.Features.Base.Components;
using static Test.App.Client.Features.WeatherForecast.WeatherForecastsState;

public partial class WeatherForecastsPage : BaseComponent
{
  protected override async Task OnInitializedAsync() =>
    await Mediator.Send(new FetchWeatherForecastsAction());
}
