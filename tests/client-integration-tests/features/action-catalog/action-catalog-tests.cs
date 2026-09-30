#region Purpose
// Enumerates the generated Test.App.Client action catalog and executes entries end to end through the store.
#endregion

namespace ActionCatalog_;

public class Registry_Should : BaseTest
{
  public Registry_Should(ClientHost clientHost) : base(clientHost) { }

  public void Contain_Only_Opted_In_Actions()
  {
    Test.App.Client.GeneratedActionCatalog.All
      .Select(entry => entry.Name)
      .ShouldBe(["Counter.AddToCount", "EventStream.AddEvent"]);
  }

  public void Describe_Entry_Metadata()
  {
    ActionCatalogEntry entry = Test.App.Client.GeneratedActionCatalog.All.Single(e => e.Name == "Counter.AddToCount");

    entry.Description.ShouldBe("Add an amount to the counter.");
    entry.Permissions.ShouldBe(["counter.write"]);
    entry.Visibility.ShouldBe(ActionVisibility.Both);
    entry.StateType.ShouldBe(typeof(Test.App.Client.Features.Counter.CounterState));
    entry.ActionType.ShouldBe(typeof(Test.App.Client.Features.Counter.CounterState.AddToCountActionSet.Action));
    entry.Parameters.Count.ShouldBe(2);
    entry.Parameters[0].ShouldBe(new ActionCatalogParameter("amount", typeof(int), true, null, """{"type":"integer"}"""));
    entry.Parameters[1].ShouldBe(new ActionCatalogParameter("multiplier", typeof(int), false, "1", """{"type":"integer"}"""));
    entry.InputSchema.ShouldBe
    (
      """{"type":"object","properties":{"amount":{"type":"integer"},"multiplier":{"type":"integer"}},"required":["amount"],"additionalProperties":false}"""
    );
  }

  public void Resolve_Through_Di_Across_Assemblies()
  {
    IActionCatalog catalog = ServiceProvider.GetRequiredService<IActionCatalog>();

    catalog.Entries.Select(entry => entry.Name).ShouldBe(["Counter.AddToCount", "EventStream.AddEvent"]);
    catalog.Find("EventStream.AddEvent").ShouldNotBeNull().Visibility.ShouldBe(ActionVisibility.Human);
    catalog.Find("Missing.Action").ShouldBeNull();
  }
}

public class Catalog_Should
{
  public void Throw_On_Duplicate_Names_Across_Sources()
  {
    ActionCatalogEntry first = Test.App.Client.GeneratedActionCatalog.All.Single(e => e.Name == "Counter.AddToCount");
    ActionCatalogEntry second = Test.App.Client.GeneratedActionCatalog.All.Single(e => e.Name == "EventStream.AddEvent");
    ActionCatalogEntry duplicate = new
    (
      first.Name, "dup", [], ActionVisibility.Both, second.StateType, second.ActionType, [], "{}",
      static (_, _, _) => Task.CompletedTask
    );

    InvalidOperationException exception = Should.Throw<InvalidOperationException>
    (
      () => new ActionCatalog([new ActionCatalogSource([first]), new ActionCatalogSource([duplicate])])
    );

    exception.Message.ShouldContain("Counter.AddToCount");
  }
}

public class Execute_Should : BaseTest
{
  public Execute_Should(ClientHost clientHost) : base(clientHost) { }

  private IActionCatalog Catalog => ServiceProvider.GetRequiredService<IActionCatalog>();

  public async Task Send_Action_Through_Store()
  {
    Test.App.Client.Features.Counter.CounterState counterState = Store.GetState<Test.App.Client.Features.Counter.CounterState>();
    counterState.Initialize(count: 10);

    await Catalog.Find("Counter.AddToCount")!.Execute(Store, [5]);

    Store.GetState<Test.App.Client.Features.Counter.CounterState>().Count.ShouldBe(15);
  }

  public async Task Convert_String_And_Long_Arguments()
  {
    Test.App.Client.Features.Counter.CounterState counterState = Store.GetState<Test.App.Client.Features.Counter.CounterState>();
    counterState.Initialize(count: 0);

    await Catalog.Find("Counter.AddToCount")!.Execute(Store, ["5", 3L]);

    Store.GetState<Test.App.Client.Features.Counter.CounterState>().Count.ShouldBe(15);
  }

  public async Task Pass_Optional_Arguments_When_Given()
  {
    Test.App.Client.Features.Counter.CounterState counterState = Store.GetState<Test.App.Client.Features.Counter.CounterState>();
    counterState.Initialize(count: 0);

    await Catalog.Find("Counter.AddToCount")!.Execute(Store, [5, 3]);

    Store.GetState<Test.App.Client.Features.Counter.CounterState>().Count.ShouldBe(15);
  }

  public async Task Send_String_Argument()
  {
    await Catalog.Find("EventStream.AddEvent")!.Execute(Store, ["from the catalog"]);

    Store.GetState<EventStreamState>().Events.ShouldContain("from the catalog");
  }

  public async Task Reject_Wrong_Argument_Count()
  {
    ArgumentException exception =
      await Should.ThrowAsync<ArgumentException>(() => Catalog.Find("Counter.AddToCount")!.Execute(Store, []));

    exception.Message.ShouldContain("expects 1 to 2 argument(s) but received 0");
  }

  public async Task Reject_Wrong_Argument_Type()
  {
    ArgumentException exception =
      await Should.ThrowAsync<ArgumentException>(() => Catalog.Find("Counter.AddToCount")!.Execute(Store, ["five"]));

    exception.Message.ShouldContain("parameter 'amount' expects System.Int32");
  }
}
