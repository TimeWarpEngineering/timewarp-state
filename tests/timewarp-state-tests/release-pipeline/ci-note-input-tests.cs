#region Purpose
// Proves the `dev ci` note input carries every gate stage as pass, the tools, and the packages, and that the
// Chromium revision and RID read the way the workflow gate compares them (task 150).
#endregion

#region Design
// CiNoteInput is the same file `dev ci` compiles. The stage list is pinned here and in
// scripts/ci-fast-path-gate.ps1 ($RequiredStages); the test reads that script so the two cannot drift.
// The namespace nests under DevCli.Commands so the linked internal types resolve without a using.
// The JSON is parsed back rather than compared as text, apart from key order, which the note keeps ordinal.
#endregion

namespace DevCli.Commands.CiNoteInputTests;

public class Should_
{
  public static void Record_Every_Gate_Stage_As_Pass()
  {
    string json = CiNoteInput.BuildJson(new Dictionary<string, string>(), []);

    using JsonDocument document = JsonDocument.Parse(json);
    JsonElement stages = document.RootElement.GetProperty("stages");
    stages.EnumerateObject().Select(stage => stage.Name)
      .ShouldBe(["assert-version-ssot", "build", "e2e", "test", "verify-samples"]);
    stages.EnumerateObject().ShouldAllBe(stage => stage.Value.GetString() == "pass");
  }

  public static void Write_Tools_And_Packages_In_Ordinal_Order()
  {
    Dictionary<string, string> tools = new()
    {
      [CiNoteInput.SdkTool] = "11.0.100",
      [CiNoteInput.RidTool] = "linux-x64",
      [CiNoteInput.ChromiumTool] = "1200",
      [CiNoteInput.PlaywrightTool] = "1.64.1"
    };
    CiNotePackage[] packages =
    [
      new("TimeWarp.State.Plus.1.0.0.nupkg", new string('b', 64)),
      new("TimeWarp.State.1.0.0.nupkg", new string('a', 64))
    ];

    using JsonDocument document = JsonDocument.Parse(CiNoteInput.BuildJson(tools, packages));

    document.RootElement.GetProperty("tools").EnumerateObject().Select(tool => tool.Name)
      .ShouldBe(["chromium", "playwright", "rid", "sdk"]);
    document.RootElement.GetProperty("tools").GetProperty("sdk").GetString().ShouldBe("11.0.100");
    document.RootElement.GetProperty("packages").EnumerateArray()
      .Select(package => $"{package.GetProperty("name").GetString()} {package.GetProperty("sha256").GetString()}")
      .ShouldBe([$"TimeWarp.State.1.0.0.nupkg {new string('a', 64)}", $"TimeWarp.State.Plus.1.0.0.nupkg {new string('b', 64)}"]);
  }

  public static void Read_The_Chromium_Revision_From_Browsers_Json()
  {
    const string BrowsersJson = """
      {"comment":"x","browsers":[{"name":"firefox","revision":"9"},{"name":"chromium","revision":"1187","installByDefault":true}]}
      """;

    CiNoteInput.ReadChromiumRevision(BrowsersJson).ShouldBe("1187");
    CiNoteInput.ReadChromiumRevision("""{"browsers":[]}""").ShouldBeNull();
    CiNoteInput.ReadChromiumRevision("not json").ShouldBeNull();
  }

  public static void Hash_A_File_As_Lowercase_Sha256_Hex()
  {
    string path = Path.GetTempFileName();
    try
    {
      File.WriteAllText(path, "abc");
      CiNoteInput.Sha256Hex(path).ShouldBe("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }
    finally
    {
      File.Delete(path);
    }
  }

  public static void Name_The_Rid_As_Os_Dash_Arch()
  {
    string rid = CiNoteInput.CurrentRid();

    rid.ShouldMatch("^(linux|osx|win|unknown)-[a-z0-9]+$");
    if (OperatingSystem.IsLinux())
    {
      rid.ShouldStartWith("linux-");
    }
  }

  public static void Match_The_Gate_Script_Stage_List()
  {
    string script = File.ReadAllText(Path.Combine(FindRepoRoot(), "scripts", "ci-fast-path-gate.ps1"));
    const string Prefix = "$RequiredStages = @(";
    string line = script.Split('\n').Single(text => text.StartsWith(Prefix, StringComparison.Ordinal));
    string[] gateStages = [.. line[Prefix.Length..line.IndexOf(')', StringComparison.Ordinal)]
      .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
      .Select(stage => stage.Trim('\''))];

    gateStages.ShouldBe(CiNoteInput.StageIds);
  }

  private static string FindRepoRoot()
  {
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "timewarp-state.slnx")))
    {
      directory = directory.Parent;
    }

    return directory?.FullName ?? throw new InvalidOperationException("timewarp-state.slnx not found above the test output.");
  }
}
