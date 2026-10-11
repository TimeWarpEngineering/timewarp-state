#region Purpose
// Proves the inspector shows inspected-app spans and hides its own actions.
#endregion

#region Design
// The host is the DevTools app. Ingest is the observation channel. UI actions go through this
// process's store and must not append a row. The DevTools assembly does not reference telemetry,
// and its clone registry does contain InspectorState: that registration belongs to this process.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorLoop_Should
{
  public static async Task ShowInspectedActionsAndHideItsOwn()
  {
    await using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();

    HttpResponseMessage first = await client.PostAsJsonAsync
    (
      "/ingest/spans",
      AppSpan("IncrementCount", """{"Count":4}"""),
      SpanIngestJsonContext.Default.SpanIngestDocument
    );
    first.EnsureSuccessStatusCode();
    (await first.Content.ReadFromJsonAsync<int>()).ShouldBe(1);

    InspectorLog log = factory.Services.GetRequiredService<InspectorLog>();
    log.Snapshot().Count.ShouldBe(1);
    log.Snapshot()[0].SnapshotJson.ShouldBe("""{"Count":4}""");

    using IServiceScope scope = factory.Services.CreateScope();
    ISender<ClientPipeline> sender = scope.ServiceProvider.GetRequiredService<ISender<ClientPipeline>>();
    await sender.Send(new InspectorState.SetFilterActionSet.Action("count"));
    await sender.Send(new InspectorState.SelectEntryActionSet.Action(1));
    await sender.Send(new InspectorState.NoteLogChangedActionSet.Action());
    await sender.Send(new InspectorState.TogglePauseActionSet.Action());

    IStore store = scope.ServiceProvider.GetRequiredService<IStore>();
    InspectorState state = store.GetState<InspectorState>();
    state.Filter.ShouldBe("count");
    state.Paused.ShouldBeTrue();
    state.SelectedSequence.ShouldBe(1);
    Names(log).ShouldBe(["IncrementCount"]);

    HttpResponseMessage whilePaused = await client.PostAsJsonAsync
    (
      "/ingest/spans",
      AppSpan("IncrementCount", """{"Count":5}"""),
      SpanIngestJsonContext.Default.SpanIngestDocument
    );
    whilePaused.EnsureSuccessStatusCode();
    Names(log).ShouldBe(["IncrementCount"]);

    await sender.Send(new InspectorState.TogglePauseActionSet.Action());
    store.GetState<InspectorState>().Paused.ShouldBeFalse();
    Names(log).ShouldBe(["IncrementCount", "IncrementCount"]);

    HttpResponseMessage inspectorSpan = await client.PostAsJsonAsync
    (
      "/ingest/spans",
      InspectorSpan(),
      SpanIngestJsonContext.Default.SpanIngestDocument
    );
    inspectorSpan.EnsureSuccessStatusCode();
    (await inspectorSpan.Content.ReadFromJsonAsync<int>()).ShouldBe(0);
    Names(log).ShouldBe(["IncrementCount", "IncrementCount"]);
    log.Snapshot().ShouldAllBe(entry => entry.StateTypeName != InspectorCapture.InspectorStateTypeName);

    await sender.Send(new InspectorState.ClearLogActionSet.Action());
    log.Snapshot().ShouldBeEmpty();
    store.GetState<InspectorState>().SelectedSequence.ShouldBe(0);
  }

  public static void RejectABodyThatIsNotTheDocument()
  {
    using WebApplicationFactory<Program> factory = new();
    HttpClient client = factory.CreateClient();
    using StringContent content = new("{", System.Text.Encoding.UTF8, "application/json");
    HttpResponseMessage response = client.PostAsync("/ingest/spans", content).GetAwaiter().GetResult();
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  public static void RegisterInspectorStateOnlyInTheDevToolsAssembly()
  {
    IState clone = StateCloneRegistry.Clone(new InspectorState());
    clone.ShouldBeOfType<InspectorState>();

    Assembly assembly = typeof(Program).Assembly;
    assembly.GetReferencedAssemblies()
      .Select(name => name.Name)
      .ShouldNotContain("TimeWarp.State.Telemetry");

    IReadOnlyList<string> methods = MethodNames(assembly.Location);
    methods.ShouldContain(name => name.Contains("InspectorState_SetFilterActionSet_Action", StringComparison.Ordinal));
    methods.ShouldContain(name => name.Contains("Clone_TimeWarp_State_DevTools_Features_Inspector_InspectorState", StringComparison.Ordinal));
  }

  private static List<string> Names(InspectorLog log) =>
    log.Snapshot().Select(entry => entry.ActionName).ToList();

  private static SpanIngestDocument AppSpan(string actionName, string snapshotJson) =>
    new()
    {
      Spans =
      [
        new SpanIngestItem
        {
          Name = "CounterState." + actionName,
          Status = "Ok",
          DurationMilliseconds = 1.5,
          StartTimeUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
          Attributes = new Dictionary<string, string>
          {
            [InspectorCapture.ActionTagName] = actionName,
            [InspectorCapture.StateTypeTagName] = "CounterState"
          },
          Events =
          [
            new SpanIngestEvent
            {
              Name = InspectorCapture.SnapshotEventName,
              Attributes = new Dictionary<string, string>
              {
                [InspectorCapture.SnapshotJsonTagName] = snapshotJson
              }
            }
          ]
        }
      ]
    };

  private static SpanIngestDocument InspectorSpan() =>
    new()
    {
      Spans =
      [
        new SpanIngestItem
        {
          Name = "InspectorState.SetFilterActionSet.Action",
          Status = "Ok",
          Attributes = new Dictionary<string, string>
          {
            [InspectorCapture.ActionTagName] = "SetFilter",
            [InspectorCapture.StateTypeTagName] = InspectorCapture.InspectorStateTypeName
          }
        },
        new SpanIngestItem
        {
          Name = "CounterState.IncrementCount",
          Status = "Ok",
          Attributes = new Dictionary<string, string>
          {
            [InspectorCapture.ActionTagName] = "IncrementCount",
            [InspectorCapture.StateTypeTagName] = "CounterState",
            [InspectorCapture.InspectorTagName] = "true"
          }
        }
      ]
    };

  private static IReadOnlyList<string> MethodNames(string assemblyPath)
  {
    using FileStream stream = File.OpenRead(assemblyPath);
    using PEReader pe = new(stream);
    MetadataReader reader = pe.GetMetadataReader();
    List<string> names = [];
    foreach (TypeDefinitionHandle handle in reader.TypeDefinitions)
    {
      TypeDefinition type = reader.GetTypeDefinition(handle);
      foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
      {
        names.Add(reader.GetString(reader.GetMethodDefinition(methodHandle).Name));
      }
    }

    return names;
  }
}
