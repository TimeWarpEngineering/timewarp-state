namespace Test.App.Client.Features.WeatherForecast;

using System.Net.Http;
using System.Net.Http.Json;
using Test.App.Contracts.Features.WeatherForecast;

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
      
      if (getWeatherForecastsResponse is null)
      {
        throw new System.ArgumentNullException(null, nameof(getWeatherForecastsResponse));
      }
      WeatherForecastsState._WeatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
    }
  }
}
