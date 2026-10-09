#region Purpose
// Options listing named timers and their configuration.
#endregion

#region Design
// A Dictionary bound from configuration (init-only), keyed by timer name.
#endregion

namespace TimeWarp.State.Plus.Features.Timers;

public class MultiTimerOptions
{
  // ReSharper disable once CollectionNeverUpdated.Global Set from Configuration
  public Dictionary<string, TimerConfig> Timers { get; init; } = new();
}
