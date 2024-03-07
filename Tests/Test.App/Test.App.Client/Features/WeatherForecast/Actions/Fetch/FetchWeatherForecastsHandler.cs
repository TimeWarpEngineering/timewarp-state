namespace Test.App.Client.Features.WeatherForecast;

internal partial class WeatherForecastsState
{
  [UsedImplicitly]
  public class FetchWeatherForecastsHandler
  (
    IStore store,
    HttpClient HttpClient
  ) : BaseActionHandler<FetchWeatherForecastsAction>(store)
  {

    public override async Task Handle
    (
      FetchWeatherForecastsAction action,
      CancellationToken cancellationToken
    )
    {
      var getWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };

      GetWeatherForecastsResponse? getWeatherForecastsResponse =
        await HttpClient.GetFromJsonAsync<GetWeatherForecastsResponse>
        (
          getWeatherForecastsRequest.RouteFactory, 
          cancellationToken: cancellationToken
        );
      
      ArgumentNullException.ThrowIfNull(getWeatherForecastsResponse);
      
      WeatherForecastsState.WeatherForecastList = getWeatherForecastsResponse.WeatherForecasts;
    }
  }
}
