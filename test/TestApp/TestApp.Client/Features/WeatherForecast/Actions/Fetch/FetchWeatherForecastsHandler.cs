namespace TestApp.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using TestApp.Shared.Features.WeatherForecast;
  using Microsoft.AspNetCore.Components;

  internal partial class WeatherForecastsState
  {
    public class FetchWeatherForecastsHandler : RequestHandler<FetchWeatherForecastsAction, WeatherForecastsState>
    {
      public FetchWeatherForecastsHandler
      (
        IStore aStore,
        HttpClient aHttpClient
      ) : base(aStore)
      {
        HttpClient = aHttpClient;
      }

      private HttpClient HttpClient { get; }
      private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

      public override async Task<WeatherForecastsState> Handle(
        FetchWeatherForecastsAction aFetchWeatherForecastsRequest,
        CancellationToken aCancellationToken)
      {
        using HttpResponseMessage httpResponseMessage = await HttpClient.GetAsync(GetWeatherForecastsRequest.Route);
        string content = await httpResponseMessage.Content.ReadAsStringAsync();
        GetWeatherForecastsResponse getWeatherForecastsResponse =
          await HttpClient.GetJsonAsync<GetWeatherForecastsResponse>(GetWeatherForecastsRequest.Route);
        List<WeatherForecastDto> weatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
        WeatherForecastsState._WeatherForecasts = weatherForecasts;
        return WeatherForecastsState;
      }
    }
  }
}