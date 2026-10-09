#region Purpose
// Integration tests for TimeWarpCacheableState, using the test app's CacheableWeatherState.
#endregion

#region Design
// Each test calls Store.RemoveState first for a fresh state, then sends the real fetch action through the pipeline to
// the in-process test server. A repeat fetch inside CacheDuration must keep the same CacheKey and TimeStamp.
#endregion

namespace CacheableStateTests;

/// <summary>
/// Integration tests for TimeWarpCacheableState caching behavior.
/// Uses CacheableWeatherState from test-app.
/// </summary>
public class CacheableState_Should : BaseTest
{
  public CacheableState_Should(ClientHost clientHost) : base(clientHost) { }

  private CacheableWeatherState CacheableWeatherState => Store.GetState<CacheableWeatherState>();

  public void HaveNullCacheKey_Initially()
  {
    // Arrange - ensure fresh state
    Store.RemoveState<CacheableWeatherState>();

    // Act
    CacheableWeatherState state = Store.GetState<CacheableWeatherState>();

    // Assert
    state.CacheKey.ShouldBeNull();
    state.TimeStamp.ShouldBeNull();
  }

  public async Task SetCacheKey_AfterFetch()
  {
    // Arrange
    Store.RemoveState<CacheableWeatherState>();
    CacheableWeatherState.CacheKey.ShouldBeNull();

    // Act
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    // Assert - cache key should be set based on action type
    CacheableWeatherState.CacheKey.ShouldNotBeNull();
    CacheableWeatherState.CacheKey.ShouldContain("FetchWeatherForecastsActionSet");
  }

  public async Task SetTimestamp_AfterFetch()
  {
    // Arrange
    Store.RemoveState<CacheableWeatherState>();
    DateTime beforeFetch = DateTime.UtcNow;

    // Act
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    // Assert - timestamp should be set to approximately now
    CacheableWeatherState.TimeStamp.ShouldNotBeNull();
    CacheableWeatherState.TimeStamp!.Value.ShouldBeGreaterThanOrEqualTo(beforeFetch);
    CacheableWeatherState.TimeStamp!.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
  }

  public async Task ReturnCachedData_WhenCacheValid()
  {
    // Arrange
    Store.RemoveState<CacheableWeatherState>();
    
    // First fetch to populate cache
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());
    
    string? firstCacheKey = CacheableWeatherState.CacheKey;
    DateTime? firstTimestamp = CacheableWeatherState.TimeStamp;
    var firstForecasts = CacheableWeatherState.WeatherForecasts;
    
    firstCacheKey.ShouldNotBeNull();
    firstTimestamp.ShouldNotBeNull();
    firstForecasts.ShouldNotBeNull();

    // Act - fetch again (should use cache)
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    // Assert - cache key and timestamp should remain the same (cache was used)
    CacheableWeatherState.CacheKey.ShouldBe(firstCacheKey);
    CacheableWeatherState.TimeStamp.ShouldBe(firstTimestamp);
  }

  public async Task Return_Ten_Forecasts_When_The_Action_Asks_For_Ten_Days()
  {
    Store.RemoveState<CacheableWeatherState>();

    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    CacheableWeatherState.WeatherForecasts.ShouldNotBeNull();
    CacheableWeatherState.WeatherForecasts.Count.ShouldBe(10);
  }
}
