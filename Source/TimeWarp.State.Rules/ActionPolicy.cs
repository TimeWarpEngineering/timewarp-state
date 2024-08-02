namespace TimeWarp.State.Rules;

public static class Policies
{
  
  // TODO: Action and Handler should be in the same file which should be named `<State>.<MethodName>.cs` and located in a `Actions` folder. 
  
  
  public static PolicyDefinition CreateActionPolicy(params Assembly[] assemblies)
  {
    BeNestedInStateCustomRule beNestedInState = new();
    BeNestedInActionSetCustomRule beNestedInActionSet = new();
    
    return Policy.Define("TimeWarp Action Policy", "See https://timewarpengineering.github.io/timewarp-architecture/")
      .For(Types.InAssemblies(assemblies))
      .Add
      (
        t => t
          .That()
          .ImplementInterface(typeof(IAction))
          .Should()
          .HaveName("Action"),
        "Action Naming", 
        "Actions must be named Action."
      )
      .Add
      (
        t => t
          .That()
          .ImplementInterface(typeof(IAction))
          .Should()
          .MeetCustomRule(beNestedInActionSet),
        "Action Nesting", 
        "Actions must be nested in a class with suffix of 'ActionSet'."
      )
      .Add
      (
        t => t
          .That()
          .ImplementInterface(typeof(IAction))
          .Should()
          .MeetCustomRule(beNestedInState),
        "Action Nesting", 
        "Actions must be nested in the State they act upon."
      )
      .Add
      (
        t => t
          .That()
          .ImplementInterface(typeof(IAction))
          .Should()
          .BeSealed()
          .And()
          .BeInternal(),
        "Action class Modifiers", 
        "Actions should be `internal sealed`."
      );
  }
  
  public static PolicyDefinition CreateActionHandlerPolicy(params Assembly[] assemblies)
  {
    BeNestedInStateCustomRule beNestedInState = new();
    return Policy.Define("TimeWarp Action Handler Policy", "See https://timewarpengineering.github.io/timewarp-architecture/")
      .For(Types.InAssemblies(assemblies))
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .MeetCustomRule(beNestedInState),
        "Nest Handlers", 
        "Action Handlers must be nested in the State they act upon."
      )
      .Add
      (
        t => t
          .That()
          .Inherit(typeof(ActionHandler<>))
          .And()
          .AreNotAbstract()
          .Should()
          .BeSealed()
          .And()
          .BeInternal(),
        "internal sealed Handler", 
        "Handler should be `internal sealed`."
      );
  }
}
