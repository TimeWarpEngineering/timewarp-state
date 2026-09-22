#region Purpose
// Proves ThrowIfNotTestAssembly accepts kebab-case and PascalCase test assembly names,
// rejects a non-test name, and honors StateTestOptions.Enable().
#endregion

namespace ThrowIfNotTestAssemblyTests;

public class Should_
{
  private static readonly object Gate = new();

  public void Lowercase_Kebab_Name_Is_A_Test_Assembly()
  {
    lock (Gate)
    {
      StateTestOptions.Reset();
      ProbeState probe = new();
      Assembly assembly = NamedAssembly("something-tests");

      Should.NotThrow(() => probe.Guard(assembly));
    }
  }

  public void PascalCase_Tests_Name_Is_A_Test_Assembly()
  {
    lock (Gate)
    {
      StateTestOptions.Reset();
      ProbeState probe = new();
      Assembly assembly = NamedAssembly("something.Tests");

      Should.NotThrow(() => probe.Guard(assembly));
    }
  }

  public void Non_Test_Name_Is_Not_A_Test_Assembly()
  {
    lock (Gate)
    {
      StateTestOptions.Reset();
      ProbeState probe = new();
      Assembly assembly = NamedAssembly("widget-host");

      Should.Throw<FieldAccessException>(() => probe.Guard(assembly))
        .Message.ShouldBe("Do not use this in production. This method is intended for Test access only!");
    }
  }

  public void Enable_Allows_A_Non_Test_Assembly()
  {
    lock (Gate)
    {
      StateTestOptions.Reset();
      try
      {
        StateTestOptions.Enable();
        ProbeState probe = new();
        Assembly assembly = NamedAssembly("widget-host");

        Should.NotThrow(() => probe.Guard(assembly));
      }
      finally
      {
        StateTestOptions.Reset();
      }
    }
  }

  private static Assembly NamedAssembly(string name)
  {
    AssemblyName assemblyName = new(name);
    Assembly assembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly
    (
      assemblyName,
      System.Reflection.Emit.AssemblyBuilderAccess.Run
    );
    assembly.FullName.ShouldNotBeNull();
    assembly.FullName.ShouldContain(name);
    return assembly;
  }

  private sealed class ProbeState : State<ProbeState>
  {
    public override void Initialize() { }

    public void Guard(Assembly assembly) => ThrowIfNotTestAssembly(assembly);
  }
}
