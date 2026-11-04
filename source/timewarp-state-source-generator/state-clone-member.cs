#region Purpose
// Carries one field or auto-property the object cloner writes.
#endregion

#region Design
// FieldName is the metadata name, including the compiler backing-field spelling, so the accessor key stays stable.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal readonly struct CloneMember
{
  public CloneMember(ITypeSymbol type, ITypeSymbol definitionType, string fieldName, INamedTypeSymbol declaringType, Location? location)
  {
    Type = type;
    DefinitionType = definitionType;
    FieldName = fieldName;
    DeclaringType = declaringType;
    Location = location;
  }

  public ITypeSymbol Type { get; }
  public ITypeSymbol DefinitionType { get; }
  public string FieldName { get; }
  public INamedTypeSymbol DeclaringType { get; }
  public Location? Location { get; }
}

