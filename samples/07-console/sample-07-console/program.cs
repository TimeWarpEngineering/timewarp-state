#region Purpose
// Console host that dispatches one action through TimeWarp.State without Blazor.
#endregion

#region Design
// ServiceCollection, AddGeneratedMediator<ClientPipeline>, and AddTimeWarpState. This project does not
// reference TimeWarp.State.Blazor. Initialize sets Count to 3; IncrementCount adds 2; the process
// exits 0 when Count is 5.
#endregion

namespace Sample07Console;

public static class Program
{
  public static async Task<int> Main()
  {
    ServiceCollection services = new();
    services.AddGeneratedMediator<ClientPipeline>();
    services.AddTimeWarpState(options => options.Assemblies = [typeof(Program).Assembly]);

    await using ServiceProvider provider = services.BuildServiceProvider();
    ISender<ClientPipeline> sender = provider.GetRequiredService<ISender<ClientPipeline>>();
    await sender.Send(new CounterState.IncrementCountActionSet.Action(2));

    CounterState counterState = provider.GetRequiredService<IStore>().GetState<CounterState>();
    // The sample contract is one stdout line. Console is the host API for that line.
#pragma warning disable RS0030
    Console.WriteLine($"Count={counterState.Count}");
#pragma warning restore RS0030
    return counterState.Count == 5 ? 0 : 1;
  }
}
