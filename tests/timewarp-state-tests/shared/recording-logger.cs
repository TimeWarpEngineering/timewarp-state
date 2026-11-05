#region Purpose
// One recording ILogger<T> for every test that asserts a log call was made: the transaction behavior's own logs and the log
// calls made through a cloned state's [CloneShared] logger.
#endregion

#region Design
// Top-level in SharedTestFixtures so the pipeline and cloning tests share it instead of keeping copies. IsEnabled is always
// true and each entry records level, event id, and the formatted message.
#endregion

namespace SharedTestFixtures;

[NotTest]
public sealed class RecordingLogger<T> : ILogger<T>
{
  public List<LogEntry> Entries { get; } = [];

  public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

  public bool IsEnabled(LogLevel logLevel) => true;

  public void Log<TState>
  (
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter
  )
  {
    Entries.Add(new LogEntry(logLevel, eventId, formatter(state, exception)));
  }

  private sealed class NullScope : IDisposable
  {
    public static readonly NullScope Instance = new();

    public void Dispose() { }
  }
}

[NotTest]
public sealed class LogEntry
{
  public LogEntry(LogLevel level, EventId eventId, string message)
  {
    Level = level;
    EventId = eventId;
    Message = message;
  }

  public LogLevel Level { get; }
  public EventId EventId { get; }
  public string Message { get; }
}
