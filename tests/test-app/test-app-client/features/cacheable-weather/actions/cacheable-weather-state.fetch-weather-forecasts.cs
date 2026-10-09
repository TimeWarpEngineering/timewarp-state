#region Purpose
// Fetches weather forecasts from the server API into CacheableWeatherState, skipping the call while the cache is
// fresh.
#endregion

#region Design
// Delegates to TimeWarpCacheableState.HandleWithCaching, which only invokes UpdateStateAsync when the action's cache
// entry is missing or expired. The query sets Days to 10 and GetRoute includes it. The request uses the scoped HttpClient.
#endregion

namespace Test.App.Client.Features.WeatherForecast;

using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

public partial class CacheableWeatherState
{
  public static class FetchWeatherForecastsActionSet
  {
    public sealed class Action : IAction;

    internal sealed class Handler : BaseActionHandler<Action>
    {
      private readonly HttpClient HttpClient;
      public Handler
      (
        IStore store,
        HttpClient httpClient
      ) : base(store)
      {
        HttpClient = httpClient;
      }

      public override async ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        await CacheableWeatherState.HandleWithCaching(action, UpdateStateAsync, cancellationToken);
      }

      private async Task UpdateStateAsync<TAction>(TAction action, CancellationToken cancellationToken)
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

        CacheableWeatherState.WeatherForecastList = getWeatherForecastsResponse;
      }
    }
  }
}
