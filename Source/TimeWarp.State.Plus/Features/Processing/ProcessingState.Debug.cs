namespace TimeWarp.Features.Processing;

public partial class ProcessingState
{
  public override ProcessingState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    string guidKey = CamelCase.MemberNameToCamelCase(nameof(Guid));
    
    if (!keyValuePairs.TryGetValue(guidKey, out object? guidValue))
    {
      throw new InvalidOperationException($"Expected key '{guidKey}' not found or value is null.");
    }
    
    var processingState = new ProcessingState
    {
      Guid = Guid.Parse(guidValue.ToString()!)
    };
    return processingState;
  }

  internal void Initialize(List<string> processingList)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    ActiveActionsList = processingList;
  }
}
