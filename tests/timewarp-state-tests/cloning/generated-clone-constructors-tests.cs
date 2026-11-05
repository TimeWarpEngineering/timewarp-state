#region Purpose
// Runs the generated clone for ICloneable members and subtypes, overloaded and dependency constructors, required members,
// records, private constructors, [CloneShared] injected services, and [field: CloneShared] on an auto-property.
#endregion

#region Design
// Fixtures are in generated-clone-shape-fixtures.cs and generated-clone-extra-fixtures.cs. ServiceState and TestNavigationManager are
// shared with the pipeline tests (shared/service-state-fixtures.cs), as is RecordingLogger (shared/recording-logger.cs); ServiceState is a State<T>, so the generator still emits
// its clone and the module initializer registers it.
#endregion

namespace GeneratedCloneShapeTests;

public partial class Should_
{
  public void Cast_Inherited_ICloneable_Result_To_The_Declared_Type()
  {
    var original = new CloneableHolder
    {
      Cloneable = new CloneableDerived { V = 1, W = 2 },
      Base = new CloneableDerived { V = 3, W = 4 },
      Hidden = CloneableHolder.MakeHidden(5)
    };

    CloneableHolder clone = original.Clone()!;

    // CloneableBase.Clone() returns a CloneableBase even for the subclass; that is the user's Clone(), not a cast error.
    clone.Cloneable.ShouldBeOfType<CloneableBase>().V.ShouldBe(101);
    clone.Base.ShouldBeOfType<CloneableBase>().V.ShouldBe(103);
    clone.Hidden.ShouldNotBeNull().V.ShouldBe(5);
    clone.Hidden.ShouldNotBeSameAs(original.Hidden);
  }

  public void Construct_With_Typed_Default_Arguments()
  {
    var original = new ConstructorShapes { Money = new Money(5m), Required = new RequiredHolder { Name = "n", Count = 2 } };

    ConstructorShapes clone = original.Clone()!;

    clone.Money!.Amount.ShouldBe(5m);
    clone.Required!.Name.ShouldBe("n");
    clone.Required.Count.ShouldBe(2);
  }

  public void Clone_State_Whose_Constructor_Takes_A_Service()
  {
    var original = new ClockState(new Clock()) { Ticks = 7 };

    var clone = (ClockState)StateCloneRegistry.Clone(original);

    clone.Ticks.ShouldBe(7);
    clone.HasClock.ShouldBeFalse();
  }

  public void Share_Injected_Services_By_Reference()
  {
    RecordingLogger<ServiceState> logger = new();
    using HttpClient httpClient = new();
    NavigationManager navigationManager = new TestNavigationManager();
    var original = new ServiceState(logger, httpClient, navigationManager) { Count = 4 };

    var clone = (ServiceState)StateCloneRegistry.Clone(original);

    clone.ShouldNotBeSameAs(original);
    clone.Count.ShouldBe(4);
    clone.Logger.ShouldBeSameAs(logger);
    clone.HttpClient.ShouldBeSameAs(httpClient);
    clone.NavigationManager.ShouldBeSameAs(navigationManager);
    clone.Guid.ShouldNotBe(original.Guid);
    clone.NavigationManager.BaseUri.ShouldBe("https://example.test/");

    clone.Logger.LogInformation("clone kept the logger");

    LogEntry entry = logger.Entries.ShouldHaveSingleItem();
    entry.Level.ShouldBe(LogLevel.Information);
    entry.Message.ShouldBe("clone kept the logger");
  }

  public void Clone_Record_Class_And_Record_Struct_Members()
  {
    Item tag = new() { V = 1 };
    Item pointTag = new() { V = 2 };
    RecordHolder original = new()
    {
      Person = new PersonRecord("ada", tag) { Age = 36 },
      Point = new Point(7, pointTag)
    };

    RecordHolder clone = original.Clone()!;

    clone.Person.ShouldNotBeNull();
    clone.Person.ShouldNotBeSameAs(original.Person);
    clone.Person.Name.ShouldBe("ada");
    clone.Person.Age.ShouldBe(36);
    clone.Person.Tag.V.ShouldBe(1);
    clone.Person.Tag.ShouldNotBeSameAs(tag);
    clone.Point.X.ShouldBe(7);
    clone.Point.Tag.V.ShouldBe(2);
    clone.Point.Tag.ShouldNotBeSameAs(pointTag);
    original.Person.Tag.ShouldBeSameAs(tag);
  }

  public void Clone_Member_With_Private_Constructor()
  {
    HiddenHolder original = new() { Hidden = Hidden.Create(5) };

    HiddenHolder clone = original.Clone()!;

    clone.Hidden.ShouldNotBeNull();
    clone.Hidden.ShouldNotBeSameAs(original.Hidden);
    clone.Hidden.N.ShouldBe(5);
    clone.Hidden.N = 6;
    original.Hidden.N.ShouldBe(5);
  }

  public void Share_Auto_Property_Marked_With_Field_Target_CloneShared()
  {
    Clock clock = new();
    FieldSharedHolder original = new() { Shared = clock, Own = new Item { V = 4 } };

    FieldSharedHolder clone = original.Clone()!;

    clone.Shared.ShouldBeSameAs(clock);
    clone.Own!.V.ShouldBe(4);
    clone.Own.ShouldNotBeSameAs(original.Own);
  }
}
