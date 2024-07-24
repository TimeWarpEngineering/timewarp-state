namespace Test.App.Client.Features.WeatherForecast;

using TimeWarp.State.Plus.State;
using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

internal partial class CacheableWeatherState: TimeWarpCacheableState<CacheableWeatherState>
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
