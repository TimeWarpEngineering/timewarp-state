namespace Test.App.Client.Features.WeatherForecast;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Test.App.Contracts.Features.WeatherForecast;
using Test.App.Client.Features.Base;

internal partial class WeatherForecastsState
{
  public class FetchWeatherForecastsHandler : BaseActionHandler<FetchWeatherForecastsAction>
  {
    private readonly HttpClient HttpClient;

    public FetchWeatherForecastsHandler(IStore store, HttpClient aHttpClient) : base(store)
    {
      HttpClient = aHttpClient;
    }

    public override async Task Handle
    (
      FetchWeatherForecastsAction aFetchWeatherForecastsAction,
      CancellationToken aCancellationToken
    )
    {
      var getWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };

      GetWeatherForecastsResponse getWeatherForecastsResponse =
        await HttpClient.GetFromJsonAsync<GetWeatherForecastsResponse>
        (
          getWeatherForecastsRequest.RouteFactory, 
          cancellationToken: aCancellationToken
        );

      WeatherForecastsState._WeatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
    }
  }
}
