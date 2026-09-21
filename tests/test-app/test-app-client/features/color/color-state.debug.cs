namespace Test.App.Client.Features.Counter;

using System.Drawing;

public partial class ColorState
{
  public override ColorState Hydrate(IDictionary<string, object> keyValuePairs)
  {
    var colorState = new ColorState
    {
      FavoriteColor = (Color)keyValuePairs[JsonNamingPolicy.CamelCase.ConvertName(nameof(FavoriteColor))],
      MyColorName = (string)keyValuePairs[JsonNamingPolicy.CamelCase.ConvertName(nameof(FavoriteColor))],
      Guid = 
        new Guid
        (
          keyValuePairs[JsonNamingPolicy.CamelCase.ConvertName(nameof(Guid))].ToString() ?? 
            throw new InvalidOperationException()
        )
    };
    
    return colorState;
  }
  
  public void Initialize(Color color, string myColorName)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    FavoriteColor = color;
    MyColorName = myColorName;
  }
}
