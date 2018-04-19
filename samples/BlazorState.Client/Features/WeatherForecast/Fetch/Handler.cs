namespace BlazorState.Client.Features.WeatherForecast.Fetch
{
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using BlazorState.Client.State;
  using BlazorState.Handlers;
  using BlazorState.Shared;
  using BlazorState.Store;
  using Microsoft.AspNetCore.Blazor;

  public class Handler : RequestHandler<Request, WeatherForecastsState>
  {
    public Handler(IStore aStore, HttpClient aHttpClient) : base(aStore)
    {
      HttpClient = aHttpClient;
    }

    private HttpClient HttpClient { get; }
    private WeatherForecastsState WeatherForecastsState => Store.GetState<WeatherForecastsState>();

    public override async Task<WeatherForecastsState> Handle(Request request, CancellationToken cancellationToken)
    {
      //TODO: add IsLoading
      //TODO: rename AjaxResponse to ServerResponse
      AjaxResponse ajaxResponse = await HttpClient.GetJsonAsync<AjaxResponse>("/api/weatherforecast");
      List<WeatherForecast> weatherForecasts = ajaxResponse.WeatherForecasts;
      WeatherForecastsState.WeatherForecasts = weatherForecasts;
      return WeatherForecastsState;
    }
  }
}