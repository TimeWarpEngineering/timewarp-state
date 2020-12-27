namespace TestApp.Client.Features.WeatherForecast
{
  using BlazorState;
  using MediatR;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Api.Features.WeatherForecast;
  using TestApp.Client.Features.Base;

  internal partial class WeatherForecastsState
  {
    public class FetchWeatherForecastsHandler : BaseActionHandler<FetchWeatherForecastsAction>
    {
      private readonly HttpClient HttpClient;

      public FetchWeatherForecastsHandler(IStore aStore, HttpClient aHttpClient) : base(aStore)
      {
        HttpClient = aHttpClient;
      }

      public override async Task<Unit> Handle
      (
        FetchWeatherForecastsAction aFetchWeatherForecastsAction,
        CancellationToken aCancellationToken
      )
      {
        var getWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };

        GetWeatherForecastsResponse getWeatherForecastsResponse =
          await HttpClient.GetFromJsonAsync<GetWeatherForecastsResponse>(getWeatherForecastsRequest.RouteFactory);

        WeatherForecastsState._WeatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
        return Unit.Value;
      }

    }
  }
}