namespace RegisterRenderTrigger;

public class Should_ 
{
  public static void Register_RenderTrigger_With_Valid_PropertySelector()
  {
    TestComponent sut = new();
    TestStore store = new();
    
    // Use reflection to set the private Store property
    PropertyInfo? storeProperty = typeof(TimeWarpStateComponent).GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance);
    storeProperty?.SetValue(sut, store);
    
    TestClass testInstance = new() { SomeProperty = "Initial" };
    store.SetState(testInstance); // Previous state will be null
    Expression<Func<TestClass, object?>> propertySelector = t => t.SomeProperty;

    // Act
    sut.RegisterRenderTrigger(propertySelector);

    // Assert
    sut.Should().NotBeNull();
    sut.ShouldReRender(typeof(TestClass)).Should().BeTrue(); // Because PreviousState is null
    store.SetState(testInstance); // Set the state to the same value so previous state will be the same
    sut.ShouldReRender(typeof(TestClass)).Should().BeFalse(); // Because the state has not changed
  }

  public static void RegisterRenderTrigger_NullPropertySelector_ThrowsArgumentNullException()
  {
    // Arrange
    TestComponent sut = new();
    Expression<Func<object, object?>> propertySelector = null!;

    // Act and Assert
    sut.Invoking(c => c.RegisterRenderTrigger(propertySelector))
      .Should().Throw<ArgumentNullException>();
  }
  
  private class TestClass:IState
  {
    public string? SomeProperty { get; set; }
    public int AnotherProperty { get; set; }
    // properties below are not used by the tests
    public ISender Sender { get; set; } = null!;
    public Guid Guid { get; } = Guid.Empty; 
    public void Initialize() => throw new NotImplementedException();
  }
}

public class TestComponent : TimeWarpStateComponent
{
  public new void RegisterRenderTrigger<T>(Expression<Func<T, object?>> propertySelector) where T : class
  {
    base.RegisterRenderTrigger(propertySelector);
  }
}

public class TestStore : IStore
{
  private IState State { get; set; } = null!;
  private IState PreviousState { get; set; } = null!;
  
  public object GetState(Type stateType) => State;
  public TState GetState<TState>() => (TState)State;
  public void SetState(IState newState)
  {
    PreviousState = State;
    State = newState;
  }
  public TState? GetPreviousState<TState>() => (TState?)PreviousState;
  
  // Properties below are not used by the tests
  public Guid Guid { get; } = Guid.Empty;
  public SemaphoreSlim GetSemaphore(Type stateType) => throw new NotImplementedException();
  public void Reset() => throw new NotImplementedException();
}
