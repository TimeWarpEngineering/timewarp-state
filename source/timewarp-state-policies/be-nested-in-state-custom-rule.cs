namespace TimeWarp.State.Policies;

public class BeNestedInStateCustomRule:ICustomRule 
{
  public bool MeetsRule(TypeDefinition typeDefinition)
  {
    var type = typeDefinition.ToType();
    
    while (type.DeclaringType != null && !typeof(IState).IsAssignableFrom(type))
    {
      type = type.DeclaringType;
    }

    bool result = typeof(IState).IsAssignableFrom(type);

    return result;
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
