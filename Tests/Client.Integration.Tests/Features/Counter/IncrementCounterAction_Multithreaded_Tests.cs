namespace CounterState;
using FluentAssertions;
using System.Threading.Tasks;
using TestApp.Client.Features.Counter;
using TestApp.Client.Integration.Tests.Infrastructure;
using static TestApp.Client.Features.Counter.CounterState;

public class IncrementCounterAction_Multithreaded_Should : BaseTest
{
  private CounterState CounterState => Store.GetState<CounterState>();
  private readonly ManualResetEvent StartEvent = new(false);
  private const int NumberOfThreads = 10;
  private const int NumberOfIncrementsPerThread = 1000;
  private volatile int NumberOfThreadsWaitingToStart = NumberOfThreads;

  public IncrementCounterAction_Multithreaded_Should(ClientHost aClientHost) : base(aClientHost) { }

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
