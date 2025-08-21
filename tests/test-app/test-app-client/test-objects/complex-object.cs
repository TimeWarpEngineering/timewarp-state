// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ArrangeMethodOrOperatorBody
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedParameter.Global
#pragma warning disable CS0067 // Event is never used
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AnyClone.Tests.TestObjects;

public class ComplexObject : IEquatable<ComplexObject>, IDisposable
{
  private bool _isDisposed;
  private readonly int _constField;

  public delegate void TestDelegate(int value);
  public TestDelegate MyTestDelegate { get; set; }
  public event TestDelegate OnTestDelegate;
  public List<string> listOfStrings = [];
  public ATestClass TestClassNoSetter { get; }
#pragma warning disable IDE0044 // Add readonly modifier
  private ATestClass _anotherTestClass;
#pragma warning restore IDE0044 // Add readonly modifier

  public ComplexObject(int initialValue)
  {
    _isDisposed = false;
    _constField = initialValue;
    MyTestDelegate = MyTestMethod;

    TestClassNoSetter = new ATestClass
    {
      Name = "Read-only Property test",
      Description = "A class assigned to a public read-only property",
      TestInterface = new InterfaceObject { BoolValue = false, IntValue = 456 }
    };

    _anotherTestClass = new ATestClass
    {
      Name = "Private field test",
      Description = "A class assigned to a private field",
      TestInterface = new InterfaceObject { BoolValue = true, IntValue = 123 }
    };
  }

  private void MyTestMethod(int value)
  {
    // this doesn't do anything
  }

  public override int GetHashCode() => base.GetHashCode();
  public override bool Equals(object aObject)
  {
    var basicObject = (ComplexObject)aObject;
    return Equals(basicObject);
  }

  public bool Equals(ComplexObject complexObject)
  {
    bool e1 = _isDisposed == complexObject._isDisposed;
    bool e2 = _constField == complexObject._constField;
    bool e3 = listOfStrings.AsEnumerable().SequenceEqual(complexObject.listOfStrings);
    bool e4 = TestClassNoSetter.Equals(complexObject.TestClassNoSetter);
    bool e5 = _anotherTestClass.Equals(complexObject._anotherTestClass);

    return e1 && e2 && e3 && e4 && e5;
  }

  protected virtual void Dispose(bool isDisposing) => _isDisposed = true;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}

public class ATestClass : IEquatable<ATestClass>
{
  public string Name { get; set; }
  public string Description { get; set; }
  public ITestInterface TestInterface { get; set; }

  public override int GetHashCode() => base.GetHashCode();
  public override bool Equals(object aObject)
  {
    var basicObject = (ATestClass)aObject;
    return Equals(basicObject);
  }

  public bool Equals(ATestClass aTestClass)
  {
    return Name.Equals(aTestClass.Name, StringComparison.CurrentCulture)
        && Description.Equals(aTestClass.Description, StringComparison.CurrentCulture)
        && TestInterface.Equals(aTestClass.TestInterface);
  }
}
