namespace TimeWarp.Features.Processing;

public partial class ActiveActionState
{
  public override ActiveActionState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    string guidKey = CamelCase.MemberNameToCamelCase(nameof(Guid));
    
    if (!keyValuePairs.TryGetValue(guidKey, out object? guidValue))
    {
      throw new InvalidOperationException($"Expected key '{guidKey}' not found or value is null.");
    }
    
    var processingState = new ActiveActionState
    {
      Guid = Guid.Parse(guidValue.ToString()!)
    };
    return processingState;
  }

  internal void Initialize(List<IAction> processingList)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    ActiveActionsList = processingList;
  }
}
