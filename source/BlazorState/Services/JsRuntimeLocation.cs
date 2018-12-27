namespace BlazorState.Services
{
  using System;

  public class JsRuntimeLocation
  {
    public bool IsClientSide => HasMono;
    public bool IsServerSide => !HasMono;
    public bool HasMono => Type.GetType("Mono.Runtime") != null;
  }
}
