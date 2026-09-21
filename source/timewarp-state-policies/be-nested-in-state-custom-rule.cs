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
