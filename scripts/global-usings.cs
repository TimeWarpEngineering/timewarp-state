#region Purpose
// Global usings shared by the scripts/ runfiles.
#endregion

#region Design
// Included in every runfile by scripts/Directory.Build.props. Imports Regex, System.Text.Json (JsonDocument for
// e2e-build.cs), Amuru, Nuru and static System.Console.
#endregion

global using System.Text.Json;
global using System.Text.RegularExpressions;
global using TimeWarp.Amuru;
global using TimeWarp.Nuru;
global using static System.Console;
