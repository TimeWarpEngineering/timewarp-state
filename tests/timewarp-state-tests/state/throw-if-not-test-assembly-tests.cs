#region Purpose
// Proves ThrowIfNotTestAssembly ignores the assembly name and honors only StateTestOptions.Enable().
#endregion

#region Design
// Uses dynamic assemblies with chosen names, and a lock because StateTestOptions is static and is reset around each
// test. Each test re-enables access in finally so a later test in this process is not left locked out.
#endregion

namespace ThrowIfNotTestAssemblyTests;

public class Should_
{
  private static readonly object Gate = new();

  public void A_Name_That_Contains_Test_Is_Not_Enough()
  {
    lock (Gate)
    {
      StateTestOptions.Reset();
      try
      {
        ProbeState probe = new();
        Assembly assembly = NamedAssembly("something-tests");

        FieldAccessException exception = Should.Throw<FieldAccessException>(() => probe.Guard(assembly));
        exception.Message.ShouldContain("Call StateTestOptions.Enable()");
        exception.Message.ShouldContain("something-tests");
      }
      finally
      {
        StateTestOptions.Enable();
      }
    }
  }

  public void Contoso_Latest_Is_Not_A_Test_Assembly()
  {
    lock (Gate)
    {
      StateTestOptions.Reset();
      try
      {
        ProbeState probe = new();
        Assembly assembly = NamedAssembly("Contoso.Latest");

        FieldAccessException exception = Should.Throw<FieldAccessException>(() => probe.Guard(assembly));
        exception.Message.ShouldContain("Contoso.Latest");
      }
      finally
      {
        StateTestOptions.Enable();
      }
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
        Assembly assembly = NamedAssembly("Contoso.Latest");

        Should.NotThrow(() => probe.Guard(assembly));
      }
      finally
      {
        StateTestOptions.Enable();
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

  internal sealed class ProbeState : State<ProbeState>
  {
    public override void Initialize() { }

    public void Guard(Assembly assembly) => ThrowIfNotTestAssembly(assembly);
  }
}
