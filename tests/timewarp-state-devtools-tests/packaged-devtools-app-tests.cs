#region Purpose
// Proves an AppHost outside this repo starts the inspector from the packed packages alone.
#endregion

#region Design
// Packs TimeWarp.State.DevTools.App and TimeWarp.State.DevTools.Hosting into a temp feed, restores
// a temp AppHost that PackageReferences Hosting only, and reads the stamped assembly metadata.
// The stamped path has to sit in that temp package folder, not in this repo. The test builds the
// same executable resource AddTimeWarpStateDevTools adds, then starts the packaged dll with that
// resource's command, the dll's directory as the working directory, and ASPNETCORE_HTTP_PORTS.
// It requests the inspector page, /_framework/blazor.web.js, and logo.png. The page head links
// that PNG. The response is 200 image/png and matches assets/logo.png. The nupkg lists
// tools/wwwroot/logo.png. The package must not ship appsettings.Development.json, tsconfig.json,
// web.config, or XML doc files.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class PackagedDevToolsApp_Should
{
  public static async Task StartFromAnOutOfTreeAppHost()
  {
    string repo = RepoPaths.Root();
    string root = Directory.CreateTempSubdirectory("tw-devtools-consumer-").FullName;
    string feed = Path.Combine(root, "feed");
    string packages = Path.Combine(root, "packages");
    string publish = Path.Combine(root, "publish");
    string consumer = Path.Combine(root, "apphost");
    Directory.CreateDirectory(feed);
    Directory.CreateDirectory(consumer);
    Path.GetFullPath(consumer).StartsWith(repo, StringComparison.Ordinal).ShouldBeFalse();

    try
    {
      string version = PackageVersion(repo);
      await RunAsync
      (
        repo,
        "dotnet",
        [
          "pack",
          Path.Combine(repo, "source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj"),
          "--configuration", "Release",
          "--output", feed,
          "-p:GeneratePackageOnBuild=false",
          $"-p:DevToolsToolsDir={publish}{Path.DirectorySeparatorChar}"
        ]
      );
      await RunAsync
      (
        repo,
        "dotnet",
        [
          "pack",
          Path.Combine(repo, "source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj"),
          "--configuration", "Release",
          "--output", feed,
          "--no-dependencies",
          "-p:GeneratePackageOnBuild=false"
        ]
      );

      File.WriteAllText(Path.Combine(consumer, "global.json"), File.ReadAllText(Path.Combine(repo, "global.json")));
      File.WriteAllText
      (
        Path.Combine(consumer, "nuget.config"),
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <config>
            <add key="globalPackagesFolder" value="{packages}" />
          </config>
          <packageSources>
            <clear />
            <add key="local" value="{feed}" />
            <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
          </packageSources>
        </configuration>
        """
      );
      File.WriteAllText
      (
        Path.Combine(consumer, "tw-devtools-consumer.csproj"),
        $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <Sdk Name="Aspire.AppHost.Sdk" Version="13.6.1" />
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net11.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <IsAspireHost>true</IsAspireHost>
            <AspireUseCliBundle>true</AspireUseCliBundle>
            <AssemblyName>TwDevToolsConsumer</AssemblyName>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Aspire.Hosting.AppHost" Version="13.6.1" />
            <PackageReference Include="TimeWarp.State.DevTools.Hosting" Version="{version}" />
          </ItemGroup>
        </Project>
        """
      );
      File.WriteAllText
      (
        Path.Combine(consumer, "program.cs"),
        """
        using Aspire.Hosting;
        using TimeWarp.State.DevTools.Hosting;

        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
        builder.AddTimeWarpStateDevTools();
        builder.Build().Run();
        """
      );

      string appPackage = Directory.GetFiles(feed, "*.nupkg")
        .Single(path => Path.GetFileName(path).Contains("DevTools.App", StringComparison.OrdinalIgnoreCase));
      AssertPackagedTools(appPackage);

      await RunAsync(consumer, "dotnet", ["build", "--configuration", "Debug"]);

      string consumerAssembly = Path.Combine(consumer, "bin", "Debug", "net11.0", "TwDevToolsConsumer.dll");
      File.Exists(consumerAssembly).ShouldBeTrue();
      string? appPath = ReadAppPath(consumerAssembly);
      appPath.ShouldNotBeNullOrWhiteSpace();
      File.Exists(appPath).ShouldBeTrue();
      Path.GetFileName(appPath).ShouldBe(TimeWarpStateDevToolsHosting.AppAssemblyFileName);
      Path.GetFullPath(appPath).StartsWith(repo, StringComparison.Ordinal).ShouldBeFalse();
      appPath.Contains("timewarp.state.devtools.app", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
      appPath.Contains($"{Path.DirectorySeparatorChar}tools{Path.DirectorySeparatorChar}", StringComparison.Ordinal).ShouldBeTrue();
      appPath.Contains("timewarp-state-devtools.csproj", StringComparison.Ordinal).ShouldBeFalse();
      Path.GetFullPath(appPath).StartsWith(Path.GetFullPath(packages), StringComparison.Ordinal).ShouldBeTrue();

      IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder
      (
        new DistributedApplicationOptions
        {
          Args = [],
          DisableDashboard = true,
          AllowUnsecuredTransport = true,
          AssemblyName = "TimeWarp.State.DevTools.Tests",
          ProjectDirectory = Path.Combine(repo, "samples/04-telemetry/apphost")
        }
      );
      IResourceBuilder<ExecutableResource> devtools = appBuilder.AddTimeWarpStateDevTools();
      string command = devtools.Resource.Command;
      command.ShouldBe(TimeWarpStateDevToolsHosting.ResolveAppLaunchCommand());
      devtools.Resource.WorkingDirectory.ShouldNotBeNullOrWhiteSpace();

      int port = FreePort();
      string? directory = Path.GetDirectoryName(appPath);
      directory.ShouldNotBeNullOrWhiteSpace();
      using CancellationTokenSource lifetime = new(TimeSpan.FromSeconds(45));
      Task<CommandOutput> running = Shell.Builder(command)
        .WithArguments(appPath)
        .WithWorkingDirectory(directory)
        .WithEnvironmentVariable(TimeWarpStateDevToolsHosting.HttpPortsEnvironmentVariable, port.ToString())
        .WithNoValidation()
        .CaptureAsync(lifetime.Token);
      try
      {
        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(2) };
        string? body = null;
        HttpStatusCode? scriptStatus = null;
        DateTime deadline = DateTime.UtcNow.AddSeconds(40);
        while (DateTime.UtcNow < deadline)
        {
          if (running.IsCompleted)
          {
            CommandOutput exited = await running;
            throw new InvalidOperationException($"Inspector exited {exited.ExitCode}. {exited.Stderr}");
          }

          try
          {
            using HttpResponseMessage page = await client.GetAsync($"http://127.0.0.1:{port}/");
            if (!page.IsSuccessStatusCode)
            {
              await Task.Delay(250);
              continue;
            }

            body = await page.Content.ReadAsStringAsync();
            using HttpResponseMessage script = await client.GetAsync
            (
              $"http://127.0.0.1:{port}/_framework/blazor.web.js"
            );
            scriptStatus = script.StatusCode;
            break;
          }
          catch (HttpRequestException)
          {
            await Task.Delay(250);
          }
        }

        body.ShouldNotBeNullOrWhiteSpace();
        body.ShouldContain("TimeWarp State DevTools");
        scriptStatus.ShouldBe(HttpStatusCode.OK);
        AssertIcon(body, repo);
        using HttpResponseMessage icon = await client.GetAsync($"http://127.0.0.1:{port}/logo.png");
        icon.StatusCode.ShouldBe(HttpStatusCode.OK);
        icon.Content.Headers.ContentType?.MediaType.ShouldBe("image/png");
        byte[] served = await icon.Content.ReadAsByteArrayAsync();
        served.ShouldBe(File.ReadAllBytes(Path.Combine(repo, "assets", "logo.png")));
      }
      finally
      {
        await lifetime.CancelAsync();
        try
        {
          await running;
        }
        catch (OperationCanceledException)
        {
        }
      }
    }
    finally
    {
      try
      {
        Directory.Delete(root, recursive: true);
      }
      catch (IOException)
      {
      }
    }
  }

  private static void AssertPackagedTools(string packagePath)
  {
    using FileStream stream = File.OpenRead(packagePath);
    using System.IO.Compression.ZipArchive package = new(stream, System.IO.Compression.ZipArchiveMode.Read);
    var names = package.Entries
      .Select(entry => entry.FullName.Replace('\\', '/'))
      .ToList();
    names.ShouldContain(name => name.EndsWith("tools/wwwroot/_framework/blazor.web.js", StringComparison.Ordinal));
    names.ShouldContain(name => name.EndsWith("tools/wwwroot/logo.png", StringComparison.Ordinal));
    names.ShouldNotContain(name => name.EndsWith("tools/wwwroot/logo.svg", StringComparison.OrdinalIgnoreCase));
    names.ShouldNotContain(name => IsToolsPackageEntry(name) && name.EndsWith("appsettings.Development.json", StringComparison.OrdinalIgnoreCase));
    names.ShouldNotContain(name => IsToolsPackageEntry(name) && name.EndsWith("tsconfig.json", StringComparison.OrdinalIgnoreCase));
    names.ShouldNotContain(name => IsToolsPackageEntry(name) && name.EndsWith("web.config", StringComparison.OrdinalIgnoreCase));
    names.ShouldNotContain(name => IsToolsPackageEntry(name) && name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
    names.ShouldContain(name => IsToolsPackageEntry(name) && name.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase));

    System.IO.Compression.ZipArchiveEntry icon = package.Entries.Single
    (
      entry => entry.FullName.Replace('\\', '/').EndsWith("tools/wwwroot/logo.png", StringComparison.Ordinal)
    );
    using Stream iconStream = icon.Open();
    using MemoryStream copy = new();
    iconStream.CopyTo(copy);
    copy.ToArray().ShouldBe(File.ReadAllBytes(Path.Combine(RepoPaths.Root(), "assets", "logo.png")));
  }

  private static void AssertIcon(string body, string repo)
  {
    int headStart = body.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
    int headEnd = body.IndexOf("</head>", headStart < 0 ? 0 : headStart, StringComparison.OrdinalIgnoreCase);
    headStart.ShouldBeGreaterThanOrEqualTo(0);
    headEnd.ShouldBeGreaterThan(headStart);
    string head = body[headStart..headEnd];
    head.ShouldContain("rel=\"icon\"");
    head.ShouldContain("type=\"image/png\"");
    head.ShouldContain("href=\"logo.png\"");
    head.ShouldNotContain("image/svg+xml");
    head.ShouldNotContain("apple-touch-icon");
    File.Exists(Path.Combine(repo, "source", "timewarp-state-devtools", "wwwroot", "logo.png")).ShouldBeFalse();
  }

  // Package paths are tools/file, not a tfm folder plus /tools/.
  private static bool IsToolsPackageEntry(string name)
  {
    return name.StartsWith("tools/", StringComparison.OrdinalIgnoreCase)
      || name.Contains("/tools/", StringComparison.OrdinalIgnoreCase);
  }

  private static string PackageVersion(string repo)
  {
    string props = File.ReadAllText(Path.Combine(repo, "source", "Directory.Build.props"));
    const string open = "<Version>";
    int start = props.IndexOf(open, StringComparison.Ordinal);
    start.ShouldBeGreaterThanOrEqualTo(0);
    start += open.Length;
    int end = props.IndexOf("</Version>", start, StringComparison.Ordinal);
    end.ShouldBeGreaterThan(start);
    return props[start..end];
  }

  private static async Task RunAsync(string workingDirectory, string fileName, IReadOnlyList<string> arguments)
  {
    CommandOutput output = await Shell.Builder(fileName)
      .WithArguments(arguments.ToArray())
      .WithWorkingDirectory(workingDirectory)
      .WithTimeout(TimeSpan.FromMinutes(20))
      .WithNoValidation()
      .CaptureAsync();

    if (output.TimedOut)
    {
      throw new TimeoutException
      (
        $"{fileName} {string.Join(' ', arguments)} did not finish.{Environment.NewLine}{output.Stdout}{Environment.NewLine}{output.Stderr}"
      );
    }

    if (!output.Success)
    {
      throw new InvalidOperationException
      (
        $"{fileName} {string.Join(' ', arguments)} exited {output.ExitCode}.{Environment.NewLine}{output.Stdout}{Environment.NewLine}{output.Stderr}"
      );
    }
  }

  private static int FreePort()
  {
    using TcpListener listener = new(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
  }

  private static string? ReadAppPath(string assemblyPath)
  {
    using FileStream stream = File.OpenRead(assemblyPath);
    using PEReader pe = new(stream);
    MetadataReader reader = pe.GetMetadataReader();
    AssemblyDefinition assembly = reader.GetAssemblyDefinition();
    foreach (CustomAttributeHandle handle in assembly.GetCustomAttributes())
    {
      CustomAttribute attribute = reader.GetCustomAttribute(handle);
      if (attribute.Constructor.Kind != HandleKind.MemberReference)
      {
        continue;
      }

      MemberReference constructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
      if (constructor.Parent.Kind != HandleKind.TypeReference)
      {
        continue;
      }

      TypeReference type = reader.GetTypeReference((TypeReferenceHandle)constructor.Parent);
      if (!string.Equals(reader.GetString(type.Name), "AssemblyMetadataAttribute", StringComparison.Ordinal))
      {
        continue;
      }

      CustomAttributeValue<string> decoded = attribute.DecodeValue(MetadataTypeProvider.Instance);
      if (decoded.FixedArguments.Length == 2
        && decoded.FixedArguments[0].Value is string key
        && string.Equals(key, TimeWarpStateDevToolsHosting.AppPathMetadataKey, StringComparison.Ordinal)
        && decoded.FixedArguments[1].Value is string value)
      {
        return value;
      }
    }

    return null;
  }

  private sealed class MetadataTypeProvider : ICustomAttributeTypeProvider<string>
  {
    internal static readonly MetadataTypeProvider Instance = new();

    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();

    public string GetSystemType() => "System.Type";

    public string GetSZArrayType(string elementType) => elementType + "[]";

    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => "";

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => "";

    public string GetTypeFromSerializedName(string name) => name;

    public PrimitiveTypeCode GetUnderlyingEnumType(string type) => PrimitiveTypeCode.Int32;

    public bool IsSystemType(string type) => string.Equals(type, "System.Type", StringComparison.Ordinal);
  }
}
