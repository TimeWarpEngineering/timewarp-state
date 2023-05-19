namespace BlazorStateAnalyzerTest;

using BlazorState;

public class SampleState : IState
{
  public Guid Guid => throw new NotImplementedException();

  public void Initialize() => throw new NotImplementedException();

  public class SampleClassAction : IAction { }

  public class SampleRecordAction : IAction { }

  public struct SampleStructAction: IAction {}
}

public record SampleInvalidRecordAction : IAction { }
public class SampleInvalidClassAction : IAction { }
public struct SampleInvalidStructAction : IAction { }
