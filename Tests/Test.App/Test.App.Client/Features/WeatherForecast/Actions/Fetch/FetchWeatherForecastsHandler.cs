namespace Test.App.Client.Features.WeatherForecast;

internal partial class WeatherForecastsState
{
  public class FetchWeatherForecastsHandler
  (
    IStore store,
    HttpClient HttpClient
  ) : BaseActionHandler<FetchWeatherForecastsAction>(store)
  {

    public override async Task Handle
    (
      FetchWeatherForecastsAction aFetchWeatherForecastsAction,
      CancellationToken aCancellationToken
    )
    {
      var getWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };

      GetWeatherForecastsResponse? getWeatherForecastsResponse =
        await HttpClient.GetFromJsonAsync<GetWeatherForecastsResponse>
        (
          getWeatherForecastsRequest.RouteFactory, 
          cancellationToken: aCancellationToken
        );
      
      ArgumentNullException.ThrowIfNull(getWeatherForecastsResponse);
      
      WeatherForecastsState.WeatherForecastList = getWeatherForecastsResponse.WeatherForecasts;
    }
  }
}
