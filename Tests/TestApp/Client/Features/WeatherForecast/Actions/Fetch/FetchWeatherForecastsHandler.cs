namespace TestApp.Client.Features.WeatherForecast
{
  using BlazorState;
  using MediatR;
  using Microsoft.AspNetCore.Components;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using TestApp.Api.Features.WeatherForecast;
  using TestApp.Client.Features.Base;

  internal partial class WeatherForecastsState
  {
    public class FetchWeatherForecastsHandler : BaseHandler<FetchWeatherForecastsAction>
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
          await HttpClient.PostJsonAsync<GetWeatherForecastsResponse>(GetWeatherForecastsRequest.Route, getWeatherForecastsRequest);
        WeatherForecastsState._WeatherForecasts = getWeatherForecastsResponse.WeatherForecasts;
        return Unit.Value;
      }
    }
  }
}