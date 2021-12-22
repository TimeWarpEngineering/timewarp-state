namespace BlazorState.Services;

public class BlazorHostingLocation
{
  public bool IsClientSide => System.OperatingSystem.IsBrowser();
  public bool IsServerSide => !IsClientSide;
}
