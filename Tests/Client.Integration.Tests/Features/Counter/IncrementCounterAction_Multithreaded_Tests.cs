namespace CounterState_;

public class IncrementCounterAction_Multithreaded_Should : BaseTest
{
  private CounterState CounterState => Store.GetState<CounterState>();
  private readonly ManualResetEvent StartEvent = new(false);
  private const int NumberOfThreads = 10;
  private const int NumberOfIncrementsPerThread = 1000;
  private volatile int NumberOfThreadsWaitingToStart = NumberOfThreads;

  public IncrementCounterAction_Multithreaded_Should(ClientHost aClientHost) : base(aClientHost) { }

  [Skip("This test fails but we don't support the use case yet anyway we enable when we do")]
  public void WhenExecutedByMultipleThreads_StateIncrementsCorrectly()
  {
    CounterState.Initialize(aCount: 0);

    var threads = new List<Thread>();
    for (int i = 0; i < NumberOfThreads; i++)
    {
      var thread = new Thread(async () => await IncrementCounterInThreadAsync());
      thread.Start();
      threads.Add(thread);
    }
    while (NumberOfThreadsWaitingToStart > 0)
      Thread.Sleep(50);

    StartEvent.Set();
    foreach (Thread thread in threads)
      thread.Join();

    CounterState.Count.Should().Be(NumberOfThreads * NumberOfIncrementsPerThread);
  }

  private async Task IncrementCounterInThreadAsync()
  {
    Interlocked.Decrement(ref NumberOfThreadsWaitingToStart);
    var incrementCounterRequest = new IncrementCounterAction
    {
      Amount = 5
    };

    StartEvent.WaitOne();
    for (int i = 0; i < NumberOfIncrementsPerThread; i++)
    {
      await Send(incrementCounterRequest);
    }
  }
}
