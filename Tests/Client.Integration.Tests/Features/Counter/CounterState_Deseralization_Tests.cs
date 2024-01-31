namespace CounterState_;

public class JsonSerializer_Should : BaseTest
{
  public JsonSerializer_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
    CounterState = Store.GetState<CounterState>();
  }

  private CounterState CounterState { get; set; }

  public void Deserialize_Given_String()
  {
    //Arrange

    string serializedData = """
      {"Count":8,"Guid":"e83494e8-d177-42c2-ac6f-22b8ba878935"}
      """;

    JsonSerializerOptions JsonSerializerOptions = new();
    //{
    //  DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    //  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    //  IgnoreReadOnlyProperties = true,
    //  PropertyNameCaseInsensitive = true,
    //  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //  ReadCommentHandling = JsonCommentHandling.Skip,
    //  WriteIndented = false,
    //};

    //Act
    CounterState result = (CounterState)JsonSerializer.Deserialize(serializedData, typeof(CounterState), JsonSerializerOptions);


    //Assert
    result.Count.Should().Be(8);
    result.Guid.ToString().Should().Be("e83494e8-d177-42c2-ac6f-22b8ba878935");
  }


  public void Serialize_And_Deserialize()
  {
    //Arrange

    CounterState.Initialize(aCount: 8);

    //Act
    var data = JsonSerializer.Serialize(CounterState, new JsonSerializerOptions());

    CounterState deserializedCounterState = JsonSerializer.Deserialize<CounterState>(data);

    //Assert
    deserializedCounterState.Count.Should().Be(8);

    deserializedCounterState = (CounterState)JsonSerializer.Deserialize(data, typeof(CounterState));

    deserializedCounterState.Count.Should().Be(8);
  }

}
