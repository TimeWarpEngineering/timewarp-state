#region Purpose
// CacheableWeatherState: weather forecasts held by a TimeWarpCacheableState, used to test action result caching.
#endregion

#region Design
// CacheDuration is 10 seconds so tests can observe expiry. Initialize clears the list and invalidates the cache. The
// list is a private field exposed read-only.
#endregion

namespace Test.App.Client.Features.WeatherForecast;

using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

public sealed partial class CacheableWeatherState: TimeWarpCacheableState<CacheableWeatherState>
{
  private Response? WeatherForecastList;

  public IReadOnlyList<WeatherForecastDto>? WeatherForecasts => WeatherForecastList?.AsReadOnly();

  public CacheableWeatherState()
  {
    CacheDuration = TimeSpan.FromSeconds(10);
  } // Set this to short duration for testing
  
  ///<inheritdoc/>
  public override void Initialize()
  {
    WeatherForecastList = null;
    InvalidateCache();
  }
}
