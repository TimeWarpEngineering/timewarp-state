#region Purpose
// Fetches weather forecasts from the server API into WeatherForecastsState on every call (no caching).
#endregion

#region Design
// Uses the scoped HttpClient and GetWeatherForecasts.Query. The query sets Days to 10 and GetRoute puts that value
// on the URL. Throws if the response is null. Compare CacheableWeatherState's version, which caches the result.
#endregion

namespace Test.App.Client.Features.WeatherForecast;

using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

public partial class WeatherForecastsState
{
  public static class FetchWeatherForecastsActionSet
  {
    public sealed class Action : IAction;

    internal sealed  class Handler : BaseActionHandler<Action>
    {
      private readonly HttpClient HttpClient;

      public Handler(IStore store, HttpClient httpClient) : base(store)
      {
        HttpClient = httpClient;
      }

      public override async ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        var query = new Query()
        {
          Days = 10
        };

        Response? getWeatherForecastsResponse =
          await HttpClient.GetFromJsonAsync<Response>
          (
            query.GetRoute(),
            cancellationToken: cancellationToken
          );

        ArgumentNullException.ThrowIfNull(getWeatherForecastsResponse);

        WeatherForecastsState.WeatherForecastList = getWeatherForecastsResponse;
      }
    }
  }
}
