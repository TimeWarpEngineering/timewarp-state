#region Purpose
// NetArchTest custom rules that check a type is nested in an IState, or in a class whose name ends with 'ActionSet'.
#endregion

#region Design
// They load the Cecil TypeDefinition as a runtime Type and reuse TryGetEnclosingStateType, so the policies apply the
// same nesting logic the runtime uses.
#endregion

namespace TimeWarp.State.Policies;

public class BeNestedInStateCustomRule:ICustomRule 
{
  public bool MeetsRule(TypeDefinition typeDefinition)
  {
    Type type = typeDefinition.ToType();
    return type.TryGetEnclosingStateType(out _);
  }
}

public class BeNestedInActionSetCustomRule:ICustomRule 
{
  public bool MeetsRule(TypeDefinition typeDefinition)
  {
    var type = typeDefinition.ToType();
    bool result = type?.DeclaringType?.Name.EndsWith("ActionSet") == true;

    return result;
  }
}
