namespace Another_Tests
{
  using Another.Tests;
  using Shouldly;

  public class CalculatorTests
  {
    public void ShouldAdd()
    {
      var calculator = new Calculator();
      calculator.Add(2, 3).ShouldBe(5);
    }

    public void ShouldSubtract()
    {
      var calculator = new Calculator();
      calculator.Subtract(5, 3).ShouldBe(2);
    }

    // Should not be visible in Test Explorer
    public void Setup() 
    {

    }
  }
}
