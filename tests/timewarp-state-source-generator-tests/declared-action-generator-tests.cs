#region Purpose
// Declared actions (task 160): an Action another generator emits under a state, declared with Mediator's
// GeneratedRequestPattern, gets State's dispatch method, catalog entry and manifest row (origin "declared"); and
// DeclaredActionFacts expands templates by Mediator 043's documented rules.
#endregion

#region Design
// The sibling generator and the State generator run in one driver (DeclaredSources.Run), so the State generator works
// from the declaration only, and the output compilation proves the emitted method and catalog bind to the Actions the
// sibling emitted. Without the sibling the same output is what TWS0015 reports (DeclaredActionAnalyzerTests).
// The template tests pin State's mirror of Mediator's expansion to the set Mediator 15.0.0-beta.10 documents on
// GeneratedRequestPatternAttribute: {Host}, {Contract}, {ContractType}; anything else, or an unmatched brace, fails.
#endregion

// ReSharper disable InconsistentNaming
namespace DeclaredAction_;

public class Should_Emit_For_Declared_Actions
{
  public static void Given_Sibling_Generator_The_Output_Compiles()
  {
    (_, Compilation output) = DeclaredSources.Run(DeclaredSources.Host);

    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Declaration_Emits_Dispatch_Method_That_Sends_The_Action()
  {
    (GeneratorDriver driver, _) = DeclaredSources.Run(DeclaredSources.Host);

    string method = DeclaredSources.Source(driver, "App.RoleState.CreateRoleActionSet_Method.g.cs").ShouldNotBeNull();
    method.ShouldContain("partial class RoleState");
    method.ShouldContain("internal async Task CreateRole(global::App.RoleState.CreateRoleActionSet.Action action, CancellationToken? externalCancellationToken = null)");
    method.ShouldContain("        await Sender.Send\n        (\n            action,\n");
    DeclaredSources.Source(driver, "App.RoleState.DeleteRoleActionSet_Method.g.cs").ShouldNotBeNull();
  }

  public static void Given_Declaration_Emits_Catalog_Entry_From_Contract_Attributes()
  {
    (GeneratorDriver driver, _) = DeclaredSources.Run(DeclaredSources.Host);

    string catalog = DeclaredSources.Source(driver, CatalogEmitter.HintName).ShouldNotBeNull();
    catalog.ShouldContain("name: \"Role.CreateRole\",");
    catalog.ShouldContain("description: \"Create a role.\",");
    catalog.ShouldContain("permissions: new string[] { \"roles.manage\" },");
    catalog.ShouldContain("actionType: typeof(global::App.RoleState.CreateRoleActionSet.Action),");
    catalog.ShouldContain("new global::TimeWarp.State.ActionCatalogParameter(\"action\", typeof(global::App.RoleState.CreateRoleActionSet.Action), true, null, null),");
    catalog.ShouldContain("return state.CreateRole(global::TimeWarp.State.ActionCatalogArguments.Get<global::App.RoleState.CreateRoleActionSet.Action>(\"Role.CreateRole\", arguments, 0, \"action\"), externalCancellationToken: cancellationToken);");
    catalog.ShouldContain("name: \"Role.DeleteRole\",");
    catalog.ShouldContain("description: \"\",");
  }

  public static void Given_Declaration_Emits_Manifest_Rows()
  {
    (_, Compilation output) = DeclaredSources.Run(DeclaredSources.Host);

    StateManifestState role = DeclaredSources.Manifest(output).States.ShouldHaveSingleItem();
    role.Type.ShouldBe("App.RoleState");
    role.Actions.Select(action => action.Name).ShouldBe(["CreateRoleActionSet.Action", "DeleteRoleActionSet.Action"]);

    StateManifestAction create = role.Actions[0];
    create.Type.ShouldBe("App.RoleState+CreateRoleActionSet+Action");
    create.Method.ShouldBe("CreateRole");
    create.CatalogName.ShouldBe("Role.CreateRole");
    create.Origin.ShouldBe(StateManifestAction.DeclaredOrigin);
    create.IsDeclared.ShouldBeTrue();
    create.Handler.ShouldBe("App.RoleState+CreateRoleActionSet+Handler");
    create.Response.ShouldBe("App.CreateRole.Response");
    create.Parameters.ShouldBe([new StateManifestParameter("action", "App.RoleState.CreateRoleActionSet.Action", null, true)]);

    StateManifestAction delete = role.Actions[1];
    delete.Origin.ShouldBe(StateManifestAction.DeclaredOrigin);
    delete.Response.ShouldBeNull();
  }

  public static void Given_Derived_Attribute_With_Const_Templates()
  {
    const string Source =
      """
      namespace App
      {
        using TimeWarp.Mediator;
        using TimeWarp.State;

        public sealed class ContractStateAttribute(params System.Type[] contracts) : GeneratedRequestPatternAttribute(contracts)
        {
          public const string RequestTypeName = "{Host}.{Contract}ActionSet.Action";
          public const string HandlerTypeName = "{Host}.{Contract}ActionSet.Handler";
          public const string RequestInterfaceName = "TimeWarp.Mediator.IAction";
        }

        // The sibling also emits CreateRoleActionSet, which this test does not declare.
        public static class CreateRole { public sealed class Response { } }

        public static class DeleteRole { }

        [ContractState(typeof(DeleteRole))]
        public sealed partial class RoleState : State<RoleState>
        {
          public override void Initialize() { }
        }
      }
      """;

    (GeneratorDriver driver, Compilation output) = DeclaredSources.Run(Source);

    DeclaredSources.Source(driver, "App.RoleState.DeleteRoleActionSet_Method.g.cs").ShouldNotBeNull();
    DeclaredSources.Manifest(output).States.ShouldHaveSingleItem().Actions.ShouldHaveSingleItem().IsDeclared.ShouldBeTrue();
    GeneratorTestHost.ShouldCompile(output);
  }
}

public class Should_Not_Emit_For_Declared_Actions
{
  public static void Given_The_Action_Is_Written_In_Source()
  {
    const string Source =
      """
      namespace App
      {
        using TimeWarp.Mediator;
        using TimeWarp.State;

        public static class DeleteRole { }

        [GeneratedRequestPattern("{Host}.{Contract}ActionSet.Action", "{Host}.{Contract}ActionSet.Handler", "TimeWarp.Mediator.IAction", typeof(DeleteRole))]
        public sealed partial class RoleState : State<RoleState>
        {
          public override void Initialize() { }

          public static class DeleteRoleActionSet
          {
            public sealed class Action : IAction { }
          }
        }
      }
      """;

    (GeneratorDriver driver, Compilation output) = DeclaredSources.Run(Source, sibling: null);

    DeclaredSources.Source(driver, "App.RoleState.DeleteRoleActionSet_Method.g.cs").ShouldNotBeNull().ShouldContain("new DeleteRoleActionSet.Action()");
    StateManifestAction action = DeclaredSources.Manifest(output).States.ShouldHaveSingleItem().Actions.ShouldHaveSingleItem();
    action.Origin.ShouldBe(StateManifestAction.SourceOrigin);
    GeneratorTestHost.ShouldCompile(output);
  }

  public static void Given_Request_Not_Under_A_State()
  {
    const string Source =
      """
      namespace App
      {
        using TimeWarp.Mediator;
        using TimeWarp.State;

        public static class DeleteRole { }

        [GeneratedRequestPattern("{ContractType}.Command", "{ContractType}.Handler", "TimeWarp.Mediator.IAction", typeof(DeleteRole))]
        public sealed partial class RoleState : State<RoleState>
        {
          public override void Initialize() { }
        }
      }
      """;

    (GeneratorDriver driver, Compilation output) = DeclaredSources.Run(Source, sibling: null);

    GeneratorTestHost.Sources(driver).ShouldNotContain(source => source.HintName.EndsWith("_Method.g.cs", StringComparison.Ordinal));
    DeclaredSources.Manifest(output).States.ShouldHaveSingleItem().Actions.ShouldBeEmpty();
  }

  public static void Given_The_State_Already_Has_The_Method()
  {
    const string Source =
      """
      namespace App
      {
        using TimeWarp.Mediator;
        using TimeWarp.State;

        public static class DeleteRole { }

        [GeneratedRequestPattern("{Host}.{Contract}ActionSet.Action", "{Host}.{Contract}ActionSet.Handler", "TimeWarp.Mediator.IAction", typeof(DeleteRole))]
        public sealed partial class RoleState : State<RoleState>
        {
          public override void Initialize() { }

          public System.Threading.Tasks.Task DeleteRole(string name) => System.Threading.Tasks.Task.CompletedTask;
        }
      }
      """;

    (GeneratorDriver driver, Compilation output) = DeclaredSources.Run(Source, sibling: null);

    DeclaredSources.Source(driver, "App.RoleState.DeleteRoleActionSet_Method.g.cs").ShouldBeNull();
    DeclaredSources.Source(driver, CatalogEmitter.HintName).ShouldBeNull();
    StateManifestAction action = DeclaredSources.Manifest(output).States.ShouldHaveSingleItem().Actions.ShouldHaveSingleItem();
    action.Method.ShouldBe("DeleteRole");
    action.CatalogName.ShouldBeNull();
  }
}

public class Should_Expand_Templates_Like_Mediator
{
  public static void Given_The_Documented_Placeholders()
  {
    DeclaredActionFacts.TryExpand("{Host}.{Contract}ActionSet.Action", "App.RoleState", "CreateRole", "App.Contracts.CreateRole", out string request)
      .ShouldBeTrue();
    request.ShouldBe("App.RoleState.CreateRoleActionSet.Action");

    DeclaredActionFacts.TryExpand("TimeWarp.Mediator.IRequest<{ContractType}.Response>", "App.RoleState", "CreateRole", "App.Contracts.CreateRole", out string response)
      .ShouldBeTrue();
    response.ShouldBe("TimeWarp.Mediator.IRequest<App.Contracts.CreateRole.Response>");
  }

  public static void Given_Unknown_Placeholder_Or_Unmatched_Brace_Or_Generic_Host()
  {
    DeclaredActionFacts.TryExpand("{Host}.{Name}ActionSet.Action", "App.RoleState", "CreateRole", "App.CreateRole", out _).ShouldBeFalse();
    DeclaredActionFacts.TryExpand("{Host.Action", "App.RoleState", "CreateRole", "App.CreateRole", out _).ShouldBeFalse();
    DeclaredActionFacts.TryExpand("Host}.Action", "App.RoleState", "CreateRole", "App.CreateRole", out _).ShouldBeFalse();
    DeclaredActionFacts.TryExpand("{Host}.Action", null, "CreateRole", "App.CreateRole", out _).ShouldBeFalse();
  }

  public static void Given_Dotted_Name_Checks()
  {
    DeclaredActionFacts.IsDottedTypeName("global::App.RoleState.CreateRoleActionSet.Action").ShouldBeTrue();
    DeclaredActionFacts.IsDottedTypeName("App.RoleState<int>.Action").ShouldBeFalse();
    DeclaredActionFacts.IsDottedTypeName("").ShouldBeFalse();
  }
}

public class Should_Read_Declared_Manifest_Fields
{
  public static void Given_Origin_Handler_And_Response()
  {
    StateManifestAction action = StateManifest.Parse
    (
      """{"version":1,"assembly":"A","states":[{"type":"A.S","name":"S","actions":[{"type":"A.S+XActionSet+Action","name":"XActionSet.Action","origin":"declared","handler":"A.S+XActionSet+Handler","response":"A.X.Response"}]}]}"""
    ).States.ShouldHaveSingleItem().Actions.ShouldHaveSingleItem();

    action.IsDeclared.ShouldBeTrue();
    action.Handler.ShouldBe("A.S+XActionSet+Handler");
    action.Response.ShouldBe("A.X.Response");
  }

  public static void Given_No_Origin_Reads_As_Source()
  {
    StateManifestAction action = StateManifest.Parse
    (
      """{"version":1,"assembly":"A","states":[{"type":"A.S","name":"S","actions":[{"type":"A.S+XActionSet+Action","name":"XActionSet.Action"}]}]}"""
    ).States.ShouldHaveSingleItem().Actions.ShouldHaveSingleItem();

    action.Origin.ShouldBe(StateManifestAction.SourceOrigin);
    action.IsDeclared.ShouldBeFalse();
    action.Handler.ShouldBeNull();
    action.Response.ShouldBeNull();
  }
}

public class Should_Keep_Declared_Step_Cached
{
  public static void Given_Unrelated_File_Added()
  {
    CSharpCompilation compilation = GeneratorTestHost.CreateCompilation(DeclaredSources.Host, "Declared.Tests");
    (GeneratorDriver driver, _, _) = GeneratorTestHost.Run(compilation, new TimeWarpStateGenerator());
    string[] first = GeneratorTestHost.GeneratedTexts(driver);

    driver = driver.RunGenerators(compilation.AddSyntaxTrees(GeneratorTestHost.Parse("namespace App; public sealed class Unrelated { }", "Unrelated.cs")));

    GeneratorRunResult result = driver.GetRunResult().Results.ShouldHaveSingleItem();
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Declared).ShouldAllBe(reason => GeneratorTestHost.IsCachedOrUnchanged(reason));
    GeneratorTestHost.TrackedReasons(result, TrackingNames.Graph).ShouldAllBe(reason => reason == IncrementalStepRunReason.Cached);
    GeneratorTestHost.GeneratedTexts(driver).ShouldBe(first);
  }
}
