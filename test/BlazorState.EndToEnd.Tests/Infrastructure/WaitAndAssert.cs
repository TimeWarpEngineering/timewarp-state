using System;
using System.Collections;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Shouldly;

namespace BlazorState.EndToEnd.Tests.Infrastructure
{
  /// <summary>
  /// Shouldly assertions, but hooked into Selenium's WebDriverWait mechanism 
  /// </summary>
  public class WaitAndAssert
  {
    public static IWebDriver WebDriver;
    private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);

    //public static void Collection<T>(Func<IEnumerable<T>> actualValues, params Action<T>[] elementInspectors)
    //    => WaitAssertCore(() => Assert.Collection(actualValues(), elementInspectors));

    public static void WaitAndAssertContains(string expectedSubstring, Func<string> actualString)
      => WaitAssertCore(() => actualString().ShouldContain(expectedSubstring));

    public static void WaitAndAssertEmpty<T>(Func<IEnumerable<T>> actualValues)
      => WaitAssertCore(() => actualValues().ShouldBeEmpty());

    public static void WaitAndAssertNotEmpty<T>(Func<IEnumerable<T>> actualValues)
      => WaitAssertCore(() => actualValues().ShouldNotBeEmpty());

    public static void WaitAndAssertEqual<T>(T expected, Func<T> actual)
      => WaitAssertCore(() => actual().ShouldBe(expected));

    public static void False(Func<bool> actual)
      => WaitAssertCore(() => actual().ShouldBeFalse());

    public static void Single<T>(Func<IEnumerable<T>> actualValues)
      => WaitAssertCore(() => actualValues().ShouldHaveSingleItem());

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
        // Instead of reporting it as a timeout, report the exception
        assertion();
      }
    }
  }
}