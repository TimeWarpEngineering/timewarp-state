namespace Test.App.Client.Features.CloneTest;

internal partial class CloneableState : State<CloneableState>, ICloneable
{
  public int Count { get; private set; }

  /// <summary>
  /// Set the Initial State
  /// </summary>
  public override void Initialize() => Count = 3;
  
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>We are trying to prove ICloneable is used when available instead of AnyClone.</remarks>
  /// <returns>New CloneableState object where Count is always 42</returns>
  public object Clone() => new CloneableState { Count = 42 };
}
