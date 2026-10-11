#region Purpose
// Proves the inspected-app fixture does not clone or pipeline DevTools states.
#endregion

#region Design
// This assembly references the inspected-app fixture and the exporter, not the DevTools app and not
// the telemetry sample. The fixture project-references TimeWarp.State source, so restore does not
// need a packed TimeWarp.State package. Loading it runs the fixture's clone initializer and not the
// inspector's. The fixture sender dispatches CounterState through StateTransactionBehavior and has
// no InspectorState dispatch method.
#endregion

namespace TimeWarp.State.DevTools.Isolation.Tests;

public class InspectedApp_Should
{
  public static void NotReferenceTheDevToolsApp()
  {
    Assembly inspectedApp = typeof(Program).Assembly;
    string[] referenced = inspectedApp.GetReferencedAssemblies().Select(name => name.Name!).ToArray();
    referenced.ShouldContain("TimeWarp.State.DevTools.Export");
    referenced.ShouldNotContain("TimeWarp.State.DevTools");
    referenced.ShouldContain("TimeWarp.State");
  }

  public static void RegisterCounterStateAndNotInspectorState()
  {
    _ = typeof(CounterState);
    ImmutableDictionary<Type, Func<IState, IState>> cloners = Cloners();
    cloners.ContainsKey(typeof(CounterState)).ShouldBeTrue();
    cloners.Keys.Any(type =>
      type.Name == "InspectorState"
      || (type.FullName?.Contains("TimeWarp.State.DevTools", StringComparison.Ordinal) ?? false))
      .ShouldBeFalse();
  }

  public static void LeaveInspectorStatesOutOfTheExportAssembly()
  {
    typeof(TimeWarp.State.DevTools.Export.AssemblyMarker).Assembly.GetTypes()
      .Where(type => type.IsClass && !type.IsAbstract && typeof(IState).IsAssignableFrom(type))
      .ShouldBeEmpty();
  }

  public static void RunCounterActionsThroughStateTransactionBehaviorAndNotInspectorActions()
  {
    IReadOnlyList<string> methods = MethodNames(typeof(Program).Assembly.Location);
    methods.ShouldContain("Dispatch_global__InspectedApp_Features_Counter_CounterState_IncrementCountActionSet_Action");
    methods.ShouldContain(name => name.Contains("Clone_InspectedApp_Features_Counter_CounterState", StringComparison.Ordinal));
    methods.ShouldNotContain(name => name.Contains("Inspector", StringComparison.Ordinal));

    using FileStream stream = File.OpenRead(typeof(Program).Assembly.Location);
    using PEReader pe = new(stream);
    MetadataReader reader = pe.GetMetadataReader();
    bool transaction = false;
    foreach (TypeReferenceHandle handle in reader.TypeReferences)
    {
      TypeReference type = reader.GetTypeReference(handle);
      if (reader.GetString(type.Name).Contains("StateTransactionBehavior", StringComparison.Ordinal))
      {
        transaction = true;
      }
    }

    transaction.ShouldBeTrue();
  }

  private static ImmutableDictionary<Type, Func<IState, IState>> Cloners()
  {
    FieldInfo field = typeof(StateCloneRegistry).GetField
    (
      "Cloners",
      BindingFlags.NonPublic | BindingFlags.Static
    )!;
    field.ShouldNotBeNull();
    return (ImmutableDictionary<Type, Func<IState, IState>>)field.GetValue(null)!;
  }

  private static IReadOnlyList<string> MethodNames(string assemblyPath)
  {
    using FileStream stream = File.OpenRead(assemblyPath);
    using PEReader pe = new(stream);
    MetadataReader reader = pe.GetMetadataReader();
    List<string> names = [];
    foreach (TypeDefinitionHandle handle in reader.TypeDefinitions)
    {
      foreach (MethodDefinitionHandle methodHandle in reader.GetTypeDefinition(handle).GetMethods())
      {
        names.Add(reader.GetString(reader.GetMethodDefinition(methodHandle).Name));
      }
    }

    return names;
  }
}
