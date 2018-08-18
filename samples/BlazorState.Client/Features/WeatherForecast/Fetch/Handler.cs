namespace BlazorState.Client.Features.WeatherForecast
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorState.Shared;
  using Microsoft.AspNetCore.Blazor;

  public partial class WeatherForecastsState
  {
    public class Handler : RequestHandler<FetchWeatherForecastsRequest, WeatherForecastsState>
    {
      public Handler(IStore aStore, HttpClient aHttpClient) : base(aStore)
      {
        HttpClient = aHttpClient;
      }

      private HttpClient HttpClient { get; }
      private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

      public override async Task<WeatherForecastsState> Handle(FetchWeatherForecastsRequest request, CancellationToken cancellationToken)
      {
        //TODO: add IsLoading
        AjaxResponse ajaxResponse = await HttpClient.GetJsonAsync<AjaxResponse>("/api/weatherforecast");
        List<WeatherForecast> weatherForecasts = ajaxResponse.WeatherForecasts;
        WeatherForecastsState._WeatherForecasts = weatherForecasts;
        return WeatherForecastsState;
      }
    }
  }
}