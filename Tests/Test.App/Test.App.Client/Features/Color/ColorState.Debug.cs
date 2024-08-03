namespace Test.App.Client.Features.Counter;

using System.Drawing;

public partial class ColorState
{
  public override ColorState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    var colorState = new ColorState
    {
      FavoriteColor = (Color)keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(FavoriteColor))],
      MyColorName = (string)keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(FavoriteColor))],
      Guid = 
        new Guid
        (
          keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString() ?? 
            throw new InvalidOperationException()
        )
    };
    
    return colorState;
  }

  [UsedImplicitly]
  public void Initialize(Color color, string myColorName)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    FavoriteColor = color;
    MyColorName = myColorName;
  }
}
