namespace TestApp.Client.Integration.Tests.Clone
{
  using System;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using System.Collections.Generic;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Shared.Features.WeatherForecast;
  using AnyClone;
  using System.Linq;

  public class TestState
  {
    public TestState()
    {
      // Create an array of strings to sort.
      string[] fruits = { "apricot", "orange", "banana", "mango", "apple", "grape", "strawberry" };

      // Sort the strings first by their length and then alphabetically
      // by passing the identity selector function.
      SortedFruits = fruits.OrderBy(aFruit => aFruit.Length).ThenBy(aFruit => aFruit);
    }
    
    public IOrderedEnumerable<string> SortedFruits { get; set; }
    //public IOrderedEnumerable<KeyValuePair<string, int>> SomeOrderedData { get; set; }
  }

  //public partial class SomeState
  //{
  //  public class SomeHandler : RequestHandler<SomeRequest, SomeState>
  //  {
  //    public SomeHandler(IStore store) : base(store) { }

  //    public SomeState SomeState => Store.GetState<SomeState>();

  //    public override async Task<SomeState> Handle(SomeRequest req, CancellationToken token)
  //    {
  //      var dataSource = ...
  //             SomeState.SomeData = from entry in dataSource orderby entry.Key select entry;
  //      return SomeState;
  //    }
  //  }
  //}


  internal class CloneTests
  {
    public CloneTests(TestFixture aTestFixture)    {    }  

    public void ShouldCloneTestState()
    {

      // Arrange
      var testState = new TestState();

      // Act
      TestState clone = testState.Clone();

      // Assert
      clone.SortedFruits.Count().ShouldBe(7);
    }
  }
}
