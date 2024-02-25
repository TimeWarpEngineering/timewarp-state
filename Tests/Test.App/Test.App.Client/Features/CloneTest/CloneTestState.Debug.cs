namespace Test.App.Client.Features.CloneTest;

internal partial class CloneTestState
{
  public override CloneTestState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    var counterState = new CloneTestState
    {
      Count = Convert.ToInt32(keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Count))].ToString()),
      Guid = 
        new Guid
        (
          keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString() ?? 
          throw new InvalidOperationException()
        )
    };

    return counterState;
  }

  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="aCount"></param>
  [UsedImplicitly] public void Initialize(int aCount)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    Count = aCount;
  }
}
