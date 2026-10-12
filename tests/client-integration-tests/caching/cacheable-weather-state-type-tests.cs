#region Purpose
// Proves CacheableWeatherState is IState of itself.
#endregion

#region Design
// Reflection only: no host. TimeWarpCacheableState<TState> derives from State<TState>, so the closed
// CacheableWeatherState implements IState of that same argument.
#endregion

namespace CacheableStateTests;

public class CacheableWeatherState_Type_Should
{
  public static void Implement_IState_Of_Itself()
  {
    typeof(IState<CacheableWeatherState>).IsAssignableFrom(typeof(CacheableWeatherState)).ShouldBeTrue();
  }
}
