#region Purpose
// End-to-end check of a [Reducer] action: the State generator emits its Handler and the generated TimeWarp.Mediator
// dispatches to it through the real ClientPipeline (task 144, Option 3b).
#endregion

#region Design
// CounterState.DecrementCountActionSet has no hand-written Handler. If either half of Option 3b broke, the test app
// would not compile (CS0246 in Mediator.g.cs) or Send would throw MissingMessageHandlerException, so these tests fail
// loudly. The state transaction behavior clones the state before the handler runs, so each assertion reads the state
// from the store again after sending.
#endregion

namespace ReducerDispatch;

using Counter = Test.App.Client.Features.Counter;

public class DecrementCount_Should : BaseTest
{
  public DecrementCount_Should(ClientHost clientHost) : base(clientHost) { }

  public async Task Reduce_WhenSentThroughThePipeline()
  {
    // Arrange
    Store.RemoveState<Counter.CounterState>();
    Store.GetState<Counter.CounterState>().Initialize(count: 10);

    // Act
    await Send(new Counter.CounterState.DecrementCountActionSet.Action(amount: 4));

    // Assert
    Store.GetState<Counter.CounterState>().Count.ShouldBe(6);
  }

  public async Task Reduce_WhenCalledThroughTheDispatchMethod()
  {
    // Arrange
    Store.RemoveState<Counter.CounterState>();
    Counter.CounterState counterState = Store.GetState<Counter.CounterState>();
    counterState.Initialize(count: 10);

    // Act
    await counterState.DecrementCount(amount: 3);

    // Assert
    Store.GetState<Counter.CounterState>().Count.ShouldBe(7);
  }

  public void UseTheGeneratedHandler()
  {
    // Act
    GeneratedCodeAttribute? generated = typeof(Counter.CounterState.DecrementCountActionSet.Handler)
      .GetCustomAttribute<GeneratedCodeAttribute>();

    // Assert
    generated.ShouldNotBeNull();
    generated.Tool.ShouldBe("TimeWarp.State.SourceGenerator");
    typeof(Counter.CounterState.DecrementCountActionSet.Handler)
      .IsAssignableTo(typeof(StateActionHandler<Counter.CounterState.DecrementCountActionSet.Action>))
      .ShouldBeTrue();
  }
}
