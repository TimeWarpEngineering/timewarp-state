#region Purpose
// Posts the inspected app's State manifests to the DevTools manifest endpoint once, at host start.
#endregion

#region Design
// A hosted service so the post happens after the app is configured and needs no call site beyond registration.
// StartAsync never throws and never waits on DevTools longer than the 2 second client timeout: a stopped
// inspector must not fail or slow the inspected app. Assemblies without a StateManifestAttribute contribute nothing,
// and with nothing to send no request is made. The attribute is read by type, not by name, which stays AOT safe.
// The post happens once and is not retried. A DevTools instance started after the app shows no manifest until the app
// restarts. Re-posting would need a retry loop or a DevTools pull, which this channel does not have yet.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Hosted service that posts State manifests to <c>POST /ingest/manifest</c> when the host starts.
/// </summary>
public sealed class DevToolsManifestPublisher : IHostedService, IDisposable
{
  private readonly HttpClient HttpClient;
  private readonly Uri IngestUri;
  private readonly Assembly[] Assemblies;

  /// <summary>
  /// Creates a publisher that posts to <paramref name="ingestUri"/>.
  /// </summary>
  /// <param name="ingestUri">Absolute <c>/ingest/manifest</c> URI.</param>
  /// <param name="assemblies">Assemblies whose manifests are sent.</param>
  public DevToolsManifestPublisher(Uri ingestUri, IEnumerable<Assembly> assemblies)
  {
    ArgumentNullException.ThrowIfNull(ingestUri);
    ArgumentNullException.ThrowIfNull(assemblies);
    if (!ingestUri.IsAbsoluteUri)
    {
      throw new ArgumentException("Ingest URI must be absolute.", nameof(ingestUri));
    }

    IngestUri = ingestUri;
    Assemblies = assemblies.ToArray();
    HttpClient = new HttpClient
    {
      Timeout = TimeSpan.FromSeconds(2)
    };
  }

  /// <summary>
  /// The raw manifest JSON of each assembly that declares states.
  /// </summary>
  internal static List<string> CollectManifests(IEnumerable<Assembly> assemblies)
  {
    List<string> manifests = [];
    foreach (Assembly assembly in assemblies)
    {
      StateManifestAttribute? attribute = assembly?.GetCustomAttribute<StateManifestAttribute>();
      if (attribute is not null && !string.IsNullOrWhiteSpace(attribute.Json))
      {
        manifests.Add(attribute.Json);
      }
    }

    return manifests;
  }

  /// <summary>
  /// Posts the manifests. Returns true when DevTools accepted them. Never throws.
  /// </summary>
  internal async Task<bool> PublishAsync(CancellationToken cancellationToken)
  {
    try
    {
      List<string> manifests = CollectManifests(Assemblies);
      if (manifests.Count == 0)
      {
        return false;
      }

      ManifestIngestDocument document = new()
      {
        Manifests = manifests
      };
      string json = JsonSerializer.Serialize(document, ManifestIngestJsonContext.Default.ManifestIngestDocument);
      using HttpRequestMessage request = new(HttpMethod.Post, IngestUri)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };
      using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
      return response.IsSuccessStatusCode;
    }
    catch (Exception exception) when (exception is not OutOfMemoryException)
    {
      return false;
    }
  }

  /// <inheritdoc />
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await PublishAsync(cancellationToken);
  }

  /// <inheritdoc />
  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

  /// <inheritdoc />
  public void Dispose()
  {
    HttpClient.Dispose();
  }
}
