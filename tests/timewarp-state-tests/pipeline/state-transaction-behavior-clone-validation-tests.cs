#region Purpose
// Proves StateTransactionBehavior rejects a clone that copies the Guid or leaves it empty, and fails clearly when a
// state has no clone at all.
#endregion

#region Design
// EqualGuidState.Clone is MemberwiseClone, so Guid is copied. EmptyGuidState.Clone returns Guid.Empty.
// ThrowingConstructorState is not ICloneable and not a State<T>, so the registry has no generated clone and Clone throws
// InvalidOperationException. The clone is made outside the behavior's try, so none of these touch the store or publish.
#endregion

namespace StateTransactionBehaviorTests;

public partial class Should_
{
  public async Task Throw_InvalidCloneException_When_Clone_Copies_Guid()
  {
    EqualGuidState originalState = new(Guid.NewGuid());
    Harness<EqualGuidState.ThrowAction> harness = CreateHarness<EqualGuidState.ThrowAction>(originalState);

    InvalidCloneException exception = await Should.ThrowAsync<InvalidCloneException>
    (
      async () => await harness.Behavior.Handle
      (
        new EqualGuidState.ThrowAction(),
        (_, _) => new ValueTask<Unit>(Unit.Value),
        CancellationToken.None
      )
    );

    exception.EnclosingStateType.ShouldBe(typeof(EqualGuidState));
    exception.CloneCause.ShouldBe(InvalidCloneException.Cause.EqualGuid);
    exception.Message.ShouldContain("equal Guid");
    exception.Message.ShouldContain("[CloneShared]");
    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

  public async Task Throw_InvalidCloneException_When_Clone_Guid_Is_Empty()
  {
    EmptyGuidState originalState = new(Guid.NewGuid());
    Harness<EmptyGuidState.ThrowAction> harness = CreateHarness<EmptyGuidState.ThrowAction>(originalState);

    InvalidCloneException exception = await Should.ThrowAsync<InvalidCloneException>
    (
      async () => await harness.Behavior.Handle
      (
        new EmptyGuidState.ThrowAction(),
        (_, _) => new ValueTask<Unit>(Unit.Value),
        CancellationToken.None
      )
    );

    exception.EnclosingStateType.ShouldBe(typeof(EmptyGuidState));
    exception.CloneCause.ShouldBe(InvalidCloneException.Cause.EmptyGuid);
    exception.Message.ShouldContain("empty Guid");
    exception.Message.ShouldContain("construct the clone so the initializer runs");
    exception.Message.ShouldContain("[CloneShared]");
    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    harness.Publisher.Publications.ShouldBeEmpty();
  }

  public async Task Throw_When_State_Has_No_Clone()
  {
    ThrowingConstructorState originalState = new(seed: 1);
    originalState.Guid.ShouldNotBe(Guid.Empty);
    Harness<ThrowingConstructorState.ThrowAction> harness =
      CreateHarness<ThrowingConstructorState.ThrowAction>(originalState);

    InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>
    (
      async () => await harness.Behavior.Handle
      (
        new ThrowingConstructorState.ThrowAction(),
        (_, _) => new ValueTask<Unit>(Unit.Value),
        CancellationToken.None
      )
    );

    exception.Message.ShouldContain(nameof(ThrowingConstructorState));
    exception.Message.ShouldContain("ICloneable");
    harness.Store.CurrentState.ShouldBeSameAs(originalState);
    harness.Publisher.Publications.ShouldBeEmpty();
  }
}
