using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Shouldly;

namespace BlazorState.EndToEnd.Tests.Infrastructure
{
  // XUnit assertions, but hooked into Selenium's polling mechanism

  public class WaitAssert
  {
    public static IWebDriver WebDriver;
    private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);
    //public static void Collection<T>(Func<IEnumerable<T>> actualValues, params Action<T>[] elementInspectors)
    //    => WaitAssertCore(() => actualValues().ShouldContain(elementInspectors));

    public static void Contains(string expectedSubstring, Func<string> actualString)
            => WaitAssertCore(() => actualString().ShouldContain(expectedSubstring));

    //public static void Empty(Func<IEnumerable> actualValues)
    //  => WaitAssertCore(() => actualValues().ShouldBeEmpty());

    public static void Equal<T>(T expected, Func<T> actual)
                        => WaitAssertCore(() => actual().ShouldBe(expected));

    public static void False(Func<bool> actual)
        => WaitAssertCore(() => actual().ShouldBeFalse());

    //public static void Single(Func<IEnumerable> actualValues)
    //  => WaitAssertCore(() => actualValues().ShouldHaveSingleItem());

    public static void True(Func<bool> actual)
      => WaitAssertCore(() => actual().ShouldBeTrue());

    private static void WaitAssertCore(Action assertion, TimeSpan timeout = default)
    {
      if (timeout == default)
      {
        timeout = DefaultTimeout;
      }

      try
      {
        new WebDriverWait(WebDriver, timeout).Until(_ =>
        {
          try
          {
            assertion();
            return true;
          }
          catch
          {
            return false;
          }
        });
      }
      catch (WebDriverTimeoutException)
      {
        // Instead of reporting it as a timeout, report the Xunit exception
        assertion();
      }
    }
  }
}