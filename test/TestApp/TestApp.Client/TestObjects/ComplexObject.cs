using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AnyClone.Tests.TestObjects
{
    public class ComplexObject : IEquatable<ComplexObject>, IDisposable
    {
        private bool _isDisposed;
        private readonly int _constField;

        public delegate void TestDelegate(int value);
        public TestDelegate MyTestDelegate { get; set; }
#pragma warning disable 0067
        public event TestDelegate OnTestDelegate;
#pragma warning restore 0067

        public List<string> listOfStrings = new List<string>();
        public ATestClass TestClassNoSetter { get; }
        private ATestClass _anotherTestClass;

        public ComplexObject(int initialValue)
        {
            _isDisposed = false;
            _constField = initialValue;
            MyTestDelegate = MyTestMethod;

            TestClassNoSetter = new ATestClass()
            {
                Name = "Read-only Property test",
                Description = "A class assigned to a public read-only property",
                TestInterface = new InterfaceObject { BoolValue = false, IntValue = 456 }
            };

            _anotherTestClass = new ATestClass()
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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var basicObject = (ComplexObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ComplexObject other)
        {
            var e1 = _isDisposed == other._isDisposed;
            var e2 = _constField == other._constField;
            var e3 = listOfStrings.AsEnumerable().SequenceEqual(other.listOfStrings);
            var e4 = TestClassNoSetter.Equals(other.TestClassNoSetter);
            var e5 = _anotherTestClass.Equals(other._anotherTestClass);

            return e1 && e2 && e3 && e4 && e5
                ;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            _isDisposed = true;
        }

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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var basicObject = (ATestClass)obj;
            return Equals(basicObject);
        }

        public bool Equals(ATestClass other)
        {
            return Name.Equals(other.Name, StringComparison.CurrentCulture)
                && Description.Equals(other.Description, StringComparison.CurrentCulture)
                && TestInterface.Equals(other.TestInterface)
                ;
        }
    }
}
