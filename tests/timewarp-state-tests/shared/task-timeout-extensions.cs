#region Purpose
// Bounds every await on a TaskCompletionSource or an overlapping handler task so a broken interleaving fails instead of hanging.
#endregion

#region Design
// Task.WaitAsync(TimeSpan) throws TimeoutException when the task does not finish. The default of ten seconds is far longer than any
// healthy test needs and short enough to fail fast in CI. Tests never use it to order events; TaskCompletionSource does that.
#endregion

namespace SharedTestFixtures;

[NotTest]
public static class TaskTimeoutExtensions
{
  private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

  public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan? timeout = null)
  {
    return task.WaitAsync(timeout ?? DefaultTimeout);
  }

  public static Task WithTimeout(this Task task, TimeSpan? timeout = null)
  {
    return task.WaitAsync(timeout ?? DefaultTimeout);
  }
}
