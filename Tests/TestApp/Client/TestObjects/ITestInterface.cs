namespace AnyClone.Tests.TestObjects
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public interface ITestInterface
  {
    bool BoolValue { get; set; }
    int IntValue { get; set; }
    IDictionary<int, BasicObject> DictionaryValue { get; set; }
  }
}