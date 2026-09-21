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
