namespace TestApp.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using TestApp.Api.Features.WeatherForecast;
  using Microsoft.AspNetCore.Components;

  internal partial class WeatherForecastsState
  {
    public class FetchWeatherForecastsHandler : RequestHandler<FetchWeatherForecastsAction, WeatherForecastsState>
    {
      public FetchWeatherForecastsHandler(IStore aStore, HttpClient aHttpClient) : base(aStore)
      {
        HttpClient = aHttpClient;
      }

      private HttpClient HttpClient { get; }
      private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

      public override async Task<WeatherForecastsState> Handle
      (
        FetchWeatherForecastsAction aFetchWeatherForecastsRequest,
        CancellationToken aCancellationToken
      )
      {
        var getWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };
        GetWeatherForecastsResponse getWeatherForecastsResponse =
          await HttpClient.PostJsonAsync<GetWeatherForecastsResponse>(GetWeatherForecastsRequest.Route, getWeatherForecastsRequest);
        WeatherForecastsState._WeatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
        return WeatherForecastsState;
      }
    }
  }
}