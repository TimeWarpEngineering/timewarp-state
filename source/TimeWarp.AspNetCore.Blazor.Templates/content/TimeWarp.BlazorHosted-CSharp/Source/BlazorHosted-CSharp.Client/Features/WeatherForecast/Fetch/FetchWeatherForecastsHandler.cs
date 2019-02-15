namespace BlazorHosted_CSharp.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorHosted_CSharp.Shared.Features.WeatherForecast;
  using Microsoft.AspNetCore.Components;

  public partial class WeatherForecastsState
  {
    public class FetchWeatherForecastsHandler : RequestHandler<FetchWeatherForecastsRequest, WeatherForecastsState>
    {
      public FetchWeatherForecastsHandler(IStore aStore, HttpClient aHttpClient) : base(aStore)
      {
        HttpClient = aHttpClient;
      }

      private HttpClient HttpClient { get; }
      private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

      public override async Task<WeatherForecastsState> Handle(
        FetchWeatherForecastsRequest aFetchWeatherForecastsRequest,
        CancellationToken aCancellationToken)
      {
        GetWeatherForecastsResponse getWeatherForecastsResponse =
          await HttpClient.GetJsonAsync<GetWeatherForecastsResponse>
          (GetWeatherForecastsRequest.Route);

        List<WeatherForecastDto> weatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
        WeatherForecastsState._WeatherForecasts = weatherForecasts;
        return WeatherForecastsState;
      }
    }
  }
}