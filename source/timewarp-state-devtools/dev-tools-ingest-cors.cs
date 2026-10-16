#region Purpose
// Development-only CORS for POST /ingest/spans, limited to configured browser origins.
#endregion

#region Design
// The server exporter is same-process HTTP and does not send Origin, so it needs no CORS.
// A browser post is cross-origin. The policy is registered only in Development and only when at
// least one absolute http(s) origin is configured. No origin means no CORS middleware, so the
// inspector is not an open sink. Origins come from TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS (Aspire
// sets that from every inspected app) and from TimeWarp:State:DevTools:CorsOrigins. Invalid entries
// are skipped. UseCors() enables the middleware without a global policy. SpanIngestEndpoint
// calls RequireCors so only POST /ingest/spans receives allow headers. When that response already
// carries Access-Control-Allow-Origin and the preflight asks for private-network access, the
// response allows it. Other routes, including the Blazor circuit, do not.
#endregion

namespace TimeWarp.State.DevTools;

/// <summary>
/// CORS policy for the DevTools span ingest route.
/// </summary>
public static class DevToolsIngestCors
{
  /// <summary>CORS policy name.</summary>
  public const string PolicyName = "TimeWarpStateDevToolsIngest";

  /// <summary>
  /// Comma- or semicolon-separated browser origins. Must match
  /// <c>TimeWarpStateDevToolsHosting.CorsOriginsEnvironmentVariable</c>. A test asserts the match.
  /// </summary>
  public const string OriginsEnvironmentVariable = "TIMEWARP_STATE_DEVTOOLS_CORS_ORIGINS";

  /// <summary>
  /// Configuration key. A string, or a section whose children are origins.
  /// </summary>
  public const string OriginsConfigurationKey = "TimeWarp:State:DevTools:CorsOrigins";

  /// <summary>
  /// Registers <see cref="PolicyName"/> when the host is Development and at least one origin is configured.
  /// </summary>
  /// <param name="builder">The inspector host builder.</param>
  /// <returns>True when the policy is registered.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
  public static bool Add(WebApplicationBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    if (!builder.Environment.IsDevelopment())
    {
      return false;
    }

    string[] origins = ParseOrigins
    (
      Environment.GetEnvironmentVariable(OriginsEnvironmentVariable),
      builder.Configuration[OriginsConfigurationKey],
      builder.Configuration.GetSection(OriginsConfigurationKey)
    );
    if (origins.Length == 0)
    {
      return false;
    }

    builder.Services.AddCors(options =>
    {
      options.AddPolicy(PolicyName, policy =>
      {
        policy.WithOrigins(origins)
          .WithMethods(HttpMethods.Post)
          .WithHeaders(HeaderNames.ContentType)
          .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
      });
    });
    return true;
  }

  /// <summary>
  /// Enables CORS middleware without a global policy, and answers a private-network preflight
  /// on <see cref="SpanIngestEndpoint.Route"/> after the policy has allowed the origin.
  /// Call only when <see cref="Add"/> returned true. The ingest route calls
  /// <c>RequireCors</c> with <see cref="PolicyName"/>.
  /// </summary>
  /// <param name="app">The inspector application.</param>
  /// <exception cref="ArgumentNullException"><paramref name="app"/> is null.</exception>
  public static void Use(WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);
    app.Use(async (context, next) =>
    {
      if (context.Request.Path.StartsWithSegments(SpanIngestEndpoint.Route))
      {
        context.Response.OnStarting(() =>
        {
          if
          (
            context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network")
            && context.Response.Headers.ContainsKey(HeaderNames.AccessControlAllowOrigin)
          )
          {
            context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
          }

          return Task.CompletedTask;
        });
      }

      await next(context);
    });
    app.UseCors();
  }

  internal static string[] ParseOrigins
  (
    string? environmentValue,
    string? configurationValue,
    IConfigurationSection? section
  )
  {
    List<string> raw = [];
    AddRaw(raw, environmentValue);
    AddRaw(raw, configurationValue);
    if (section is not null)
    {
      foreach (IConfigurationSection child in section.GetChildren())
      {
        AddRaw(raw, child.Value);
      }
    }

    List<string> origins = [];
    foreach (string value in raw)
    {
      foreach (string part in value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
      {
        if (!Uri.TryCreate(part, UriKind.Absolute, out Uri? uri))
        {
          continue;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
          && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        string origin = uri.GetLeftPart(UriPartial.Authority);
        if (origin.Length == 0 || origins.Contains(origin, StringComparer.Ordinal))
        {
          continue;
        }

        origins.Add(origin);
      }
    }

    return origins.ToArray();
  }

  private static void AddRaw(List<string> raw, string? value)
  {
    if (!string.IsNullOrWhiteSpace(value))
    {
      raw.Add(value);
    }
  }
}
