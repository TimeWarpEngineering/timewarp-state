namespace CounterState;

using Test.App.Client.Features.Counter;

[UsedImplicitly]
public class JsonSerializer_Should : BaseTest
{
  private CounterState CounterState { get; }
  private JsonSerializerOptions JsonSerializerOptions { get; }
  
  public JsonSerializer_Should(ClientHost webAssemblyHost) : base(webAssemblyHost)
  {
    CounterState = Store.GetState<CounterState>();
    JsonSerializerOptions = new JsonSerializerOptions();
  }

  public void Deserialize_Given_String()
  {
    //Arrange
    const string serializedData = """
      {"Count":8,"Guid":"e83494e8-d177-42c2-ac6f-22b8ba878935"}
      """;

    //Act
    var result = (CounterState?)JsonSerializer.Deserialize(serializedData, typeof(CounterState), JsonSerializerOptions);
    
    //Assert
    result.Should().NotBeNull();
    result!.Count.Should().Be(8);
    result.Guid.ToString().Should().Be("e83494e8-d177-42c2-ac6f-22b8ba878935");
  }
  
  public void Serialize_And_Deserialize()
  {
    //Arrange
    CounterState.Initialize(count: 8);

    //Act
    string data = JsonSerializer.Serialize(CounterState, JsonSerializerOptions);
    CounterState? deserializedCounterState = JsonSerializer.Deserialize<CounterState>(data);

    //Assert
    deserializedCounterState.Should().NotBeNull();
    deserializedCounterState!.Count.Should().Be(8);
  }
}
