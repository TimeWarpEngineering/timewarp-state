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

  public bool UseRouting { get; set; } = true;

  /// <summary>
  /// Use the FullName of the State in the ReduxDevTools
  /// </summary>
  public bool UseFullNameForStatesInDevTools { get; set; } = false;
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

