#region Purpose
// Checks that BlueState serializes and deserializes with System.Text.Json, including its Guid.
#endregion

#region Design
// Uses default JsonSerializerOptions (PascalCase) so the [JsonConstructor] (guid, count) is exercised; the round trip
// test relies on Initialize setting Count to 2.
#endregion

namespace BlueState_;

using Test.App.Client.Features.Blue;

public class JsonSerializer_Should : BaseTest
{
  private BlueState BlueState { get; }
  private JsonSerializerOptions JsonSerializerOptions { get; }
  
  public JsonSerializer_Should(ClientHost webAssemblyHost) : base(webAssemblyHost)
  {
    BlueState = Store.GetState<BlueState>();
    JsonSerializerOptions = new JsonSerializerOptions();
  }

  public void Deserialize_Given_String()
  {
    //Arrange
    const string serializedData = """
      {"Count":8,"Guid":"e83494e8-d177-42c2-ac6f-22b8ba878935"}
      """;

    //Act
    var result = (BlueState?)JsonSerializer.Deserialize(serializedData, typeof(BlueState), JsonSerializerOptions);
    
    //Assert
    result.ShouldNotBeNull();
    result!.Count.ShouldBe(8);
    result.Guid.ToString().ShouldBe("e83494e8-d177-42c2-ac6f-22b8ba878935");
  }
  
  public void Serialize_And_Deserialize()
  {
    //Arrange
    BlueState.Initialize();

    //Act
    string data = JsonSerializer.Serialize(BlueState, JsonSerializerOptions);
    BlueState? deserializedBlueState = JsonSerializer.Deserialize<BlueState>(data);

    //Assert
    deserializedBlueState.ShouldNotBeNull();
    deserializedBlueState!.Count.ShouldBe(2);
  }
}
