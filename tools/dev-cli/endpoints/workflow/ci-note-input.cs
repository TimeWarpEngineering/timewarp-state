#region Purpose
// Builds the --from file `dev ci` hands to `ganda repo ci-note write`: the stage ids, the tool versions CI
// compares, and the packed nupkgs with their sha256.
#endregion

#region Design
// Pure so the unit tests link it (tests/timewarp-state-tests). JSON is written with Utf8JsonWriter because the
// dev CLI publishes AOT and reflection serialization is off there. Keys are written in ordinal order.
// StageIds is the list scripts/ci-fast-path-gate.sh requires; change both together. `dev ci` writes a note
// only after every stage passed, so every id is "pass" (ganda accepts "fail", but a failed run has no
// fast-path use and writes nothing).
// Rid is "<os>-<arch>" from the running process (linux-x64 on the ubuntu runner and on a WSL box), built the
// same way as the gate's `uname` mapping, so the two compare exactly. A non-linux RID never takes the fast path.
// The Chromium revision is the one Playwright's browsers.json (copied into the e2e test output) asks for;
// scripts/e2e-build.cs installs that revision before the tests run, so it is the revision that ran.
// why: task 150, no-signing override (Steven 2026-11-17), unsigned note from timewarp-ganda 660
#endregion

namespace DevCli.Commands;

/// <summary>One packed nupkg the note lists: file name and lowercase sha256 hex.</summary>
internal sealed record CiNotePackage(string Name, string Sha256);

/// <summary>Builds the <c>ganda repo ci-note write --from</c> input for a passing <c>dev ci</c> run.</summary>
internal static class CiNoteInput
{
  /// <summary>Stage ids a passing run records and the workflow gate requires. Order is pipeline order.</summary>
  internal static readonly string[] StageIds = ["assert-version-ssot", "build", "test", "e2e", "verify-samples"];

  /// <summary>Tool key for <c>dotnet --version</c>.</summary>
  internal const string SdkTool = "sdk";

  /// <summary>Tool key for the runtime identifier of the machine that ran.</summary>
  internal const string RidTool = "rid";

  /// <summary>Tool key for the Microsoft.Playwright.MSTest.v4 package version.</summary>
  internal const string PlaywrightTool = "playwright";

  /// <summary>Tool key for the Chromium revision Playwright ran.</summary>
  internal const string ChromiumTool = "chromium";

  /// <summary>"&lt;os&gt;-&lt;arch&gt;" for this process, for example <c>linux-x64</c>.</summary>
  internal static string CurrentRid()
  {
    string os =
      OperatingSystem.IsLinux() ? "linux"
      : OperatingSystem.IsMacOS() ? "osx"
      : OperatingSystem.IsWindows() ? "win"
      : "unknown";
    string arch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
    return $"{os}-{arch}";
  }

  /// <summary>The <c>--from</c> JSON: every stage in <see cref="StageIds"/> as pass, the tools, and the packages.</summary>
  internal static string BuildJson(IReadOnlyDictionary<string, string> tools, IReadOnlyList<CiNotePackage> packages)
  {
    ArgumentNullException.ThrowIfNull(tools);
    ArgumentNullException.ThrowIfNull(packages);

    using MemoryStream stream = new();
    using (System.Text.Json.Utf8JsonWriter writer = new(stream))
    {
      writer.WriteStartObject();

      writer.WriteStartObject("stages");
      foreach (string stage in StageIds.Order(StringComparer.Ordinal))
      {
        writer.WriteString(stage, "pass");
      }

      writer.WriteEndObject();

      writer.WriteStartObject("tools");
      foreach (KeyValuePair<string, string> tool in tools.OrderBy(pair => pair.Key, StringComparer.Ordinal))
      {
        writer.WriteString(tool.Key, tool.Value);
      }

      writer.WriteEndObject();

      writer.WriteStartArray("packages");
      foreach (CiNotePackage package in packages.OrderBy(package => package.Name, StringComparer.Ordinal))
      {
        writer.WriteStartObject();
        writer.WriteString("name", package.Name);
        writer.WriteString("sha256", package.Sha256);
        writer.WriteEndObject();
      }

      writer.WriteEndArray();
      writer.WriteEndObject();
    }

    return System.Text.Encoding.UTF8.GetString(stream.ToArray());
  }

  /// <summary>Lowercase sha256 hex of <paramref name="path"/>.</summary>
  internal static string Sha256Hex(string path)
  {
    using FileStream file = File.OpenRead(path);
    return Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(file));
  }

  /// <summary>The <c>chromium</c> revision in Playwright's browsers.json text, or null when absent or unreadable.</summary>
  internal static string? ReadChromiumRevision(string browsersJson)
  {
    try
    {
      using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(browsersJson);
      if (!document.RootElement.TryGetProperty("browsers", out System.Text.Json.JsonElement browsers)
          || browsers.ValueKind != System.Text.Json.JsonValueKind.Array)
      {
        return null;
      }

      foreach (System.Text.Json.JsonElement browser in browsers.EnumerateArray())
      {
        if (browser.TryGetProperty("name", out System.Text.Json.JsonElement name)
            && name.GetString() == "chromium"
            && browser.TryGetProperty("revision", out System.Text.Json.JsonElement revision))
        {
          return revision.GetString();
        }
      }

      return null;
    }
    catch (System.Text.Json.JsonException)
    {
      return null;
    }
  }
}
