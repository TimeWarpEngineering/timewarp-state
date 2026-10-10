#region Purpose
// Configuration options for TimeWarp.State, their validator, and the configuration exception thrown on invalid options.
#endregion

#region Design
// Options carry the IServiceCollection so extension methods such as UseReduxDevTools can register services.
// CaptureRenderCaller defaults to false so production render paths skip StackTrace allocation.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Options for configuring TimeWarp.State
/// </summary>
public class TimeWarpStateOptions
{
  /// <summary>
  /// Assemblies to be searched for TimeWarp.Mediator Actions and Handlers
  /// </summary>
  /// <remarks>
  /// Will default to the calling assembly
  /// If the user specifies any assemblies they will have to specify the calling assembly also if they want it to be used.
  /// </remarks>
  public IEnumerable<Assembly> Assemblies { get; set; }

  /// <summary>
  /// Use the StateTransactionBehavior (default) or not
  /// </summary>
  public bool UseStateTransactionBehavior { get; set; } = true;

  /// <summary>
  /// When true, a handler exception is published as <c>ExceptionNotification</c> and then rethrown so
  /// <c>Send</c> faults. Default false: the notification is published and <c>Send</c> returns the default
  /// response. <see cref="OperationCanceledException"/> is always rethrown after rollback. Cancellation is
  /// not a failure and is not published.
  /// </summary>
  public bool RethrowHandlerExceptions { get; set; }

  public bool UseRouting { get; set; } = true;

  /// <summary>
  /// Use the FullName of the State in the ReduxDevTools
  /// </summary>
  public bool UseFullNameForStatesInDevTools { get; set; } = false;
  /// <summary>
  /// Capture <c>Class.Method</c> of the caller of <c>ShouldRender</c>, <c>SetParametersAsync</c>,
  /// and <c>StateHasChanged</c> onto the public <c>*WasCalledBy</c> diagnostic properties.
  /// </summary>
  /// <remarks>
  /// Default is <c>false</c> so production hot paths skip <see cref="StackTrace"/> allocation.
  /// Diagnostic pages that render those properties set this to <c>true</c>.
  /// Do not key this off log level; the strings are shown on screen, not only in logs.
  /// </remarks>
  public bool CaptureRenderCaller { get; set; } = false;
  public JsonSerializerOptions JsonSerializerOptions { get; }
  
  public readonly IServiceCollection ServiceCollection;

  public TimeWarpStateOptions(IServiceCollection serviceCollection)
  {
    ServiceCollection = serviceCollection;
    Assemblies = Array.Empty<Assembly>();
    JsonSerializerOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
  }
}

public class TimeWarpStateOptionsValidator
{
  public static void Validate(TimeWarpStateOptions options)
  {
    if (options.Assemblies == null || !options.Assemblies.Any())
    {
      throw new TimeWarpStateConfigurationException(nameof(options.Assemblies), "At least one assembly must be specified for scanning.");
    }

    if (options.ServiceCollection == null)
    {
      throw new TimeWarpStateConfigurationException(nameof(options.ServiceCollection), "ServiceCollection must be provided.");
    }

    if (options.JsonSerializerOptions == null)
    {
      throw new TimeWarpStateConfigurationException(nameof(options.JsonSerializerOptions), "JsonSerializerOptions must be initialized.");
    }
  }
}


public class TimeWarpStateConfigurationException : Exception
{
  public string PropertyName { get; }

  public TimeWarpStateConfigurationException(string propertyName, string message)
    : base($"{propertyName}: {message}")
  {
    PropertyName = propertyName;
  }
}

