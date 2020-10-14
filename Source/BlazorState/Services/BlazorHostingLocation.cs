namespace BlazorState.Services
{
  using System;

  public class BlazorHostingLocation
  {
    public bool IsClientSide => HasMono;
    public bool IsServerSide => !HasMono;
    public bool HasMono => Type.GetType("Mono.Runtime") != null;
  }
}