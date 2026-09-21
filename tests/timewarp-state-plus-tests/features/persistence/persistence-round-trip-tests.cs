#region Purpose
// Persistence save/load share TimeWarpStateOptions JSON and FullName keys, with Name fallback on load.
#endregion

// ReSharper disable UnusedType.Global
namespace PersistenceRoundTrip_;

public class PersistenceRoundTrip_Should
{
  private static readonly Guid SampleGuid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

  public async Task Round_Trip_Enum_As_String_Under_FullName_Key()
  {
    StorageHarness storageHarness = new();
    LocalWidgetState localWidgetState = new(SampleGuid, WidgetKind.Beta);
    PersistentStatePostProcessor<LocalWidgetState.SaveAction, object> processor =
      CreateLocalProcessor(storageHarness, localWidgetState);

    await processor.Handle(new LocalWidgetState.SaveAction(), _ => Task.FromResult(new object()), CancellationToken.None);

    string expectedKey = typeof(LocalWidgetState).FullName!;
    storageHarness.Items.Keys.ShouldBe([expectedKey]);
    storageHarness.Items[expectedKey].ShouldContain("\"kind\":\"Beta\"");

    PersistenceService persistenceService = CreatePersistenceService(storageHarness);
    object? loaded = await persistenceService.LoadState(typeof(LocalWidgetState), PersistentStateMethod.LocalStorage);

    LocalWidgetState loadedState = loaded.ShouldBeOfType<LocalWidgetState>();
    loadedState.Kind.ShouldBe(WidgetKind.Beta);
    loadedState.Guid.ShouldBe(SampleGuid);
    loadedState.Sender.ShouldBe(storageHarness.Sender);
  }

  public async Task Load_Falls_Back_To_Simple_Name_Key()
  {
    StorageHarness storageHarness = new();
    string legacyJson = JsonSerializer.Serialize
    (
      new LocalWidgetState(SampleGuid, WidgetKind.Alpha),
      typeof(LocalWidgetState),
      storageHarness.TimeWarpStateOptions.JsonSerializerOptions
    );
    storageHarness.Items[typeof(LocalWidgetState).Name] = legacyJson;

    PersistenceService persistenceService = CreatePersistenceService(storageHarness);
    object? loaded = await persistenceService.LoadState(typeof(LocalWidgetState), PersistentStateMethod.LocalStorage);

    LocalWidgetState loadedState = loaded.ShouldBeOfType<LocalWidgetState>();
    loadedState.Kind.ShouldBe(WidgetKind.Alpha);
    loadedState.Guid.ShouldBe(SampleGuid);
  }

  public async Task Load_Falls_Back_To_PascalCase_Name_Key()
  {
    StorageHarness storageHarness = new();
    // Blazored 4.x default SetItemAsync shape: PascalCase property names under the simple Name key.
    JsonSerializerOptions blazoredDefaultOptions = new()
    {
      Converters = { new JsonStringEnumConverter() }
    };
    string legacyJson = JsonSerializer.Serialize
    (
      new LocalWidgetState(SampleGuid, WidgetKind.Alpha),
      typeof(LocalWidgetState),
      blazoredDefaultOptions
    );
    storageHarness.Items[typeof(LocalWidgetState).Name] = legacyJson;

    PersistenceService persistenceService = CreatePersistenceService(storageHarness);
    object? loaded = await persistenceService.LoadState(typeof(LocalWidgetState), PersistentStateMethod.LocalStorage);

    LocalWidgetState loadedState = loaded.ShouldBeOfType<LocalWidgetState>();
    loadedState.Kind.ShouldBe(WidgetKind.Alpha);
    loadedState.Guid.ShouldBe(SampleGuid);
  }

  public async Task Load_Prefers_FullName_Over_Simple_Name()
  {
    StorageHarness storageHarness = new();
    JsonSerializerOptions jsonSerializerOptions = storageHarness.TimeWarpStateOptions.JsonSerializerOptions;
    storageHarness.Items[typeof(LocalWidgetState).Name] = JsonSerializer.Serialize
    (
      new LocalWidgetState(SampleGuid, WidgetKind.Alpha),
      typeof(LocalWidgetState),
      jsonSerializerOptions
    );
    storageHarness.Items[typeof(LocalWidgetState).FullName!] = JsonSerializer.Serialize
    (
      new LocalWidgetState(SampleGuid, WidgetKind.Beta),
      typeof(LocalWidgetState),
      jsonSerializerOptions
    );

    PersistenceService persistenceService = CreatePersistenceService(storageHarness);
    object? loaded = await persistenceService.LoadState(typeof(LocalWidgetState), PersistentStateMethod.LocalStorage);

    loaded.ShouldBeOfType<LocalWidgetState>().Kind.ShouldBe(WidgetKind.Beta);
  }

  public async Task Skip_Save_When_Storage_Is_Not_Registered()
  {
    TimeWarpStateOptions timeWarpStateOptions = CreateOptions();
    IStore store = A.Fake<IStore>();
    A.CallTo(() => store.GetState(typeof(LocalWidgetState))).Returns(new LocalWidgetState(SampleGuid, WidgetKind.Beta));

    PersistentStatePostProcessor<LocalWidgetState.SaveAction, object> processor = new
    (
      store,
      NullLogger<PersistentStatePostProcessor<LocalWidgetState.SaveAction, object>>.Instance,
      timeWarpStateOptions
    );

    object response = await processor.Handle
    (
      new LocalWidgetState.SaveAction(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    response.ShouldNotBeNull();
  }

  public async Task Session_Storage_Write_Uses_FullName_And_String()
  {
    StorageHarness storageHarness = new();
    SessionWidgetState sessionWidgetState = new(SampleGuid, WidgetKind.Beta);
    IStore store = A.Fake<IStore>();
    A.CallTo(() => store.GetState(typeof(SessionWidgetState))).Returns(sessionWidgetState);

    PersistentStatePostProcessor<SessionWidgetState.SaveAction, object> processor = new
    (
      store,
      NullLogger<PersistentStatePostProcessor<SessionWidgetState.SaveAction, object>>.Instance,
      storageHarness.TimeWarpStateOptions,
      storageHarness.SessionStorageService
    );

    await processor.Handle(new SessionWidgetState.SaveAction(), _ => Task.FromResult(new object()), CancellationToken.None);

    string expectedKey = typeof(SessionWidgetState).FullName!;
    storageHarness.Items.Keys.ShouldBe([expectedKey]);
    storageHarness.Items[expectedKey].ShouldContain("\"kind\":\"Beta\"");

    PersistenceService persistenceService = CreatePersistenceService(storageHarness);
    object? loaded = await persistenceService.LoadState(typeof(SessionWidgetState), PersistentStateMethod.SessionStorage);
    loaded.ShouldBeOfType<SessionWidgetState>().Kind.ShouldBe(WidgetKind.Beta);
  }

  private static PersistentStatePostProcessor<LocalWidgetState.SaveAction, object> CreateLocalProcessor
  (
    StorageHarness storageHarness,
    LocalWidgetState localWidgetState
  )
  {
    IStore store = A.Fake<IStore>();
    A.CallTo(() => store.GetState(typeof(LocalWidgetState))).Returns(localWidgetState);
    return new
    (
      store,
      NullLogger<PersistentStatePostProcessor<LocalWidgetState.SaveAction, object>>.Instance,
      storageHarness.TimeWarpStateOptions,
      localSessionStorageService: storageHarness.LocalStorageService
    );
  }

  private static PersistenceService CreatePersistenceService(StorageHarness storageHarness) =>
    new
    (
      storageHarness.Sender,
      storageHarness.SessionStorageService,
      storageHarness.LocalStorageService,
      NullLogger<PersistenceService>.Instance,
      storageHarness.TimeWarpStateOptions
    );

  private static TimeWarpStateOptions CreateOptions()
  {
    TimeWarpStateOptions timeWarpStateOptions = new(new ServiceCollection());
    timeWarpStateOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    return timeWarpStateOptions;
  }

  private sealed class StorageHarness
  {
    public Dictionary<string, string> Items { get; } = new(StringComparer.Ordinal);
    public ILocalStorageService LocalStorageService { get; }
    public ISessionStorageService SessionStorageService { get; }
    public ISender<ClientPipeline> Sender { get; }
    public TimeWarpStateOptions TimeWarpStateOptions { get; }

    public StorageHarness()
    {
      TimeWarpStateOptions = CreateOptions();
      Sender = A.Fake<ISender<ClientPipeline>>();
      LocalStorageService = A.Fake<ILocalStorageService>();
      SessionStorageService = A.Fake<ISessionStorageService>();
      WireLocal();
      WireSession();
    }

    private void WireLocal()
    {
      A.CallTo(() => LocalStorageService.SetItemAsStringAsync(A<string>._, A<string>._, A<CancellationToken>._))
        .ReturnsLazily(call =>
        {
          Items[call.GetArgument<string>(0)!] = call.GetArgument<string>(1)!;
          return ValueTask.CompletedTask;
        });
      A.CallTo(() => LocalStorageService.GetItemAsStringAsync(A<string>._, A<CancellationToken>._))
        .ReturnsLazily(call =>
        {
          Items.TryGetValue(call.GetArgument<string>(0)!, out string? value);
          return new ValueTask<string?>(value);
        });
    }

    private void WireSession()
    {
      A.CallTo(() => SessionStorageService.SetItemAsStringAsync(A<string>._, A<string>._, A<CancellationToken>._))
        .ReturnsLazily(call =>
        {
          Items[call.GetArgument<string>(0)!] = call.GetArgument<string>(1)!;
          return ValueTask.CompletedTask;
        });
      A.CallTo(() => SessionStorageService.GetItemAsStringAsync(A<string>._, A<CancellationToken>._))
        .ReturnsLazily(call =>
        {
          Items.TryGetValue(call.GetArgument<string>(0)!, out string? value);
          return new ValueTask<string?>(value);
        });
    }
  }

  public enum WidgetKind
  {
    Alpha,
    Beta
  }

  [PersistentState(PersistentStateMethod.LocalStorage)]
  public sealed class LocalWidgetState : State<LocalWidgetState>
  {
    public WidgetKind Kind { get; private set; }

    public LocalWidgetState() { }

    [JsonConstructor]
    public LocalWidgetState(Guid guid, WidgetKind kind)
    {
      Guid = guid;
      Kind = kind;
    }

    public override void Initialize() => Kind = WidgetKind.Alpha;

    public sealed class SaveAction : IAction;
  }

  [PersistentState(PersistentStateMethod.SessionStorage)]
  public sealed class SessionWidgetState : State<SessionWidgetState>
  {
    public WidgetKind Kind { get; private set; }

    public SessionWidgetState() { }

    [JsonConstructor]
    public SessionWidgetState(Guid guid, WidgetKind kind)
    {
      Guid = guid;
      Kind = kind;
    }

    public override void Initialize() => Kind = WidgetKind.Alpha;

    public sealed class SaveAction : IAction;
  }
}
