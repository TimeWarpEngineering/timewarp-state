#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru
#:package TimeWarp.Amuru.Tools
#:property RunAnalyzers=false

#region Purpose
// On a task/<id>-… branch, add a Task: <id> trailer to the commit message so git log and git blame name the task.
// The hook exits 0 in every case: a missing trailer must never stop a commit.
#endregion

#region Design
// git passes the message file, the message source, and (for -c/-C/--amend) a commit.
// The id follows the kitchen-id rule ganda uses: the first digit segment, plus every
// following exactly-three-digit segment, each padded to three digits (task/552-027-x -> 552-027).
// git interpret-trailers --if-exists doNothing joins the existing trailer block
// (Co-Authored-By) and never adds a second Task trailer, which covers --amend and -c/-C.
// Skipped: the merge source, the template source, a message with no text yet (plain
// git commit before the editor opens), and any branch that is not task/<id> (master,
// dev, feature/*, detached HEAD). git treats a trailer-only message as non-empty, so
// stamping an empty or template message would commit Task: <id> alone when the user
// aborts the editor. Only the text above git's scissors line counts: with commit.verbose
// or git commit -v the diff below that line is uncommented, and git strips it from the
// final message.
// why: task 645 (2026-11-15)
#endregion

using System.Text.RegularExpressions;
using TimeWarp.Amuru;

try
{
  if (args.Length == 0)
  {
    Console.Error.WriteLine("prepare-commit-msg: no message file was passed; skipped the Task trailer.");
    return 0;
  }

  string messageFile = args[0];
  string source = args.Length > 1 ? args[1] : string.Empty;
  if (source is "merge" or "template")
  {
    return 0;
  }

  CommandOutput head = await Shell.Builder("git")
    .WithArguments("symbolic-ref", "--quiet", "--short", "HEAD")
    .WithNoValidation()
    .CaptureAsync();
  if (head.ExitCode != 0)
  {
    return 0;
  }

  Match match = Regex.Match(
    head.Stdout.Trim(),
    @"^task/(?<id>\d+(?:-\d{3}(?=-|$))*)(?:-.*)?$",
    RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
  if (!match.Success)
  {
    return 0;
  }

  string taskId = string.Join(
    "-",
    match.Groups["id"].Value.Split('-').Select(segment => segment.TrimStart('0').PadLeft(3, '0')));

  CommandOutput commentConfig = await Shell.Builder("git")
    .WithArguments("config", "--get", "core.commentChar")
    .WithNoValidation()
    .CaptureAsync();
  string configured = commentConfig.Stdout.Trim();
  string commentPrefix = commentConfig.ExitCode == 0 && configured.Length > 0 && configured != "auto" ? configured : "#";

  string scissors = $"{commentPrefix} ------------------------ >8 ------------------------";
  string[] lines = await File.ReadAllLinesAsync(messageFile);
  bool hasText = lines
    .TakeWhile(line => line != scissors)
    .Any(line => line.Trim().Length > 0 && !line.StartsWith(commentPrefix, StringComparison.Ordinal));
  if (!hasText)
  {
    return 0;
  }

  CommandOutput stamp = await Shell.Builder("git")
    .WithArguments("interpret-trailers", "--in-place", "--if-exists", "doNothing", "--trailer", $"Task: {taskId}", messageFile)
    .WithNoValidation()
    .CaptureAsync();
  if (stamp.ExitCode != 0)
  {
    Console.Error.WriteLine($"prepare-commit-msg: git interpret-trailers failed (exit {stamp.ExitCode}); the commit has no Task trailer. {stamp.Stderr.Trim()}");
  }
}
catch (Exception exception)
{
  // Any failure is a warning: the hook must never stop the commit.
  Console.Error.WriteLine($"prepare-commit-msg: skipped the Task trailer ({exception.GetType().Name}: {exception.Message}).");
}

return 0;
