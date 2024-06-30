namespace TimeWarp.State.Analyzer.Tests;

// FixieVerifier is a basic implementation since Fixie doesn't have a verifier like xUnit or NUnit.
internal class FixieVerifier : IVerifier
{
  public void Empty<T>(string collectionName, IEnumerable<T> collection)
  {
    if (collection.Any())
    {
      throw new Exception($"Expected collection '{collectionName}' to be empty, but it has elements.");
    }
  }

  public void Equal<T>(T expected, T actual, string? message = null)
  {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
      throw new Exception($"Assertion Failed: {message}\nExpected: {expected}\nActual: {actual}");
    }
  }

  public void True(bool assert, string? message = null)
  {
    if (!assert)
    {
      throw new Exception($"Assertion Failed: {message}\nExpected: true\nActual: false");
    }
  }

  public void False(bool assert, string? message = null)
  {
    if (assert)
    {
      throw new Exception($"Assertion Failed: {message}\nExpected: false\nActual: true");
    }
  }

  [DoesNotReturn]
  public void Fail(string? message = null) => throw new Exception($"Fail: {message}");

  public void LanguageIsSupported(string language)
  {
    if (language != LanguageNames.CSharp)
    {
      throw new NotSupportedException($"Language '{language}' is not supported.");
    }
  }
  public void NotEmpty<T>(string collectionName, IEnumerable<T> collection)
  {
    if (!collection.Any())
    {
      throw new Exception($"Expected collection '{collectionName}' not to be empty.");
    }
  }

  public void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T>? equalityComparer = null, string? message = null)
  {
    equalityComparer ??= EqualityComparer<T>.Default;

    IEnumerable<T> expectedArray = expected as T[] ?? expected.ToArray();
    IEnumerable<T> actualArray = actual as T[] ?? actual.ToArray();

    bool areEqual = expectedArray.SequenceEqual(actualArray, equalityComparer);

    if (!areEqual)
    {
      throw new Exception
      (
      $"SequenceEqual Assertion Failed: {message}\nExpected: {string.Join(", ", expectedArray)}\nActual: {string.Join(", ", actualArray)}"
      );
    }
  }
  
  public IVerifier PushContext(string context) => this; // No operation. Context information is not used.
  
  // ReSharper disable once UnusedMember.Global
  [SuppressMessage("Performance", "CA1822:Mark members as static")]
  public Task Verify(string actual, string expected, string? message = null, string? path = null, int? line = null, int? column = null)
  {
    if (actual != expected)
    {
      throw new Exception($"Verification Failed: {message}\nExpected: {expected}\nActual: {actual}\nPath: {path}\nLine: {line}\nColumn: {column}");
    }
    return Task.CompletedTask;
  }
}
