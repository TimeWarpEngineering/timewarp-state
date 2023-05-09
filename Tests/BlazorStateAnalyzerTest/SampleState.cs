namespace BlazorStateAnalyzerTest;

using BlazorState;

public class SampleState : IState
{
  public Guid Guid => throw new NotImplementedException();

  public void Initialize() => throw new NotImplementedException();

  public class SampleAction : IAction { }
}

public class InvalidSampleAction : IAction { }
