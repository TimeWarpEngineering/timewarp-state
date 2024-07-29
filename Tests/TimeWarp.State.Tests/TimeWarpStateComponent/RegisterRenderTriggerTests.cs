namespace RegisterRenderTrigger;

using TimeWarp.Fixie;

public class Should_ 
{
  public static void Register_RenderTrigger_With_NullableString_PropertySelector()
  {
    TestComponent sut = new();
    TestStore store = new();
    
    // Use reflection to set the private Store property
    store.SetComponentStore(sut);
    // Make three states to test with
    
    TestState state0 = new() { NullableStringProperty = "Initial" };
    TestState state1 = new() { NullableStringProperty = "Changed" };
    TestState state2 = new() { NullableStringProperty = "Changed", AnotherProperty = "Changed some other property" };
    
    store.SetState(state0); // Previous state will be null
    Expression<Func<TestState, object?>> propertySelector = t => t.NullableStringProperty;

    // Act
    sut.RegisterRenderTrigger(propertySelector);

    // Assert
    sut.Should().NotBeNull();
    sut.ShouldReRender(typeof(TestState)).Should().BeTrue(); // Because PreviousState is null
    store.SetState(state1); 
    sut.ShouldReRender(typeof(TestState)).Should().BeTrue(); // Because the property we are watching has changed
    store.SetState(state2);
    sut.ShouldReRender(typeof(TestState)).Should().BeFalse(); // Because the state has not changed
  }
  
  public static void Register_RenderTrigger_With_Derived_Bool_PropertySelector()
  {
    TestComponent sut = new();
    TestStore store = new();
    
    // Use reflection to set the private Store property
    store.SetComponentStore(sut);
    // Make three states to test with
    
    TestState state0 = new() { IntProperty = 0 };
    TestState state1 = new() { IntProperty = 0 };
    TestState state2 = new() { IntProperty = 1 };
    TestState state3 = new() { IntProperty = 1 };
    
    Expression<Func<TestState, object?>> propertySelector = t => t.DerivedBoolProperty;

    // Act
    sut.RegisterRenderTrigger(propertySelector);

    // Assert
    store.SetState(state0); // Previous state will be null
    sut.ShouldReRender(typeof(TestState)).Should().BeTrue(); // Because PreviousState is null
    store.SetState(state1); 
    sut.ShouldReRender(typeof(TestState)).Should().BeFalse(); // IsActive should be the same
    store.SetState(state2);
    sut.ShouldReRender(typeof(TestState)).Should().BeTrue(); // IsActive should have changed
    store.SetState(state3);
    sut.ShouldReRender(typeof(TestState)).Should().BeFalse(); // IsActive should be the same
  }
  
  // Write tests that test the following Expressions
  // propertySelector = t => t.IntProperty;
  // propertySelector = t => t.NullableIntProperty;
  // propertySelector = t => t.StringProperty;
  // propertySelector = t => t.NullableStringProperty;
  // propertySelector = t => t.NullableReferenceProperty;
  // propertySelector = t => t.ReferenceProperty;
  // propertySelector = t => t.NullableReferenceProperty.
  
  
  // Test setup code below
  private class TestState:IState
  {
    public string StringProperty { get; set; } = null!;
    public string? NullableStringProperty { get; set; }
    public int IntProperty { get; set; }
    public int? NullableIntProperty { get; set; }
    public string? AnotherProperty { get; set; }
    
    public bool DerivedBoolProperty => IntProperty > 0;
    public TestThing? NullableReferenceProperty { get; set; }
    public TestThing ReferenceProperty { get; set; } = null!;


    // properties below are not used by the tests
    public ISender Sender { get; set; } = null!;
    public Guid Guid { get; } = Guid.Empty; 
    public void Initialize() => throw new NotImplementedException();
  }

  private class TestThing
  {
    public string? NullableStringProperty { get; set; }
    public string? AnotherProperty { get; set; }
    public string StringProperty { get; set; } = null!;
    public int? NullableIntProperty { get; set; }
    public int IntProperty { get; set; }
  }
}

[NotTest]
public class TestComponent : TimeWarpStateComponent
{
  public new void RegisterRenderTrigger<T>(Expression<Func<T, object?>> propertySelector) where T : class
  {
    base.RegisterRenderTrigger(propertySelector);
  }
}

[NotTest]
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
  
  public void SetComponentStore(TimeWarpStateComponent component)
  {
    PropertyInfo? storeProperty = typeof(TimeWarpStateComponent).GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance);
    storeProperty?.SetValue(component, this);
  }
  
  // Properties below are not used by the tests
  public Guid Guid { get; } = Guid.Empty;
  public SemaphoreSlim GetSemaphore(Type stateType) => throw new NotImplementedException();
  public void Reset() => throw new NotImplementedException();
}
