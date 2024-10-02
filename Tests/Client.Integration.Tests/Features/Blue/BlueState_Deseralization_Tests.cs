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
    result.Should().NotBeNull();
    result!.Count.Should().Be(8);
    result.Guid.ToString().Should().Be("e83494e8-d177-42c2-ac6f-22b8ba878935");
  }
  
  public void Serialize_And_Deserialize()
  {
    //Arrange
    BlueState.Initialize();

    //Act
    string data = JsonSerializer.Serialize(BlueState, JsonSerializerOptions);
    BlueState? deserializedBlueState = JsonSerializer.Deserialize<BlueState>(data);

    //Assert
    deserializedBlueState.Should().NotBeNull();
    deserializedBlueState!.Count.Should().Be(2);
  }
}
