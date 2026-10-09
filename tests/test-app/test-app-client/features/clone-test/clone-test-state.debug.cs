#region Purpose
// CloneableState's Redux DevTools Hydrate and a test-only Initialize.
#endregion

#region Design
// Hydrate reads camelCase Count and Guid keys. Initialize(count) is test-only, guarded by ThrowIfNotTestAssembly.
#endregion

namespace Test.App.Client.Features.CloneTest;

public partial class CloneableState
{
  public override CloneableState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    var counterState = new CloneableState
    {
      Count = Convert.ToInt32(keyValuePairs[JsonNamingPolicy.CamelCase.ConvertName(nameof(Count))].ToString()),
      Guid = 
        new Guid
        (
          keyValuePairs[JsonNamingPolicy.CamelCase.ConvertName(nameof(Guid))].ToString() ?? 
          throw new InvalidOperationException()
        )
    };

    return counterState;
  }

  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="count"></param>
  public void Initialize(int count)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    Count = count;
  }
}
