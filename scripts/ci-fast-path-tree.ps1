# Purpose
#   The kanban-only tree rule for scripts/ci-fast-path-gate.ps1: two trees count as the same build input when
#   they are equal once the top-level `kanban` entry is removed from both, and the candidate walk that finds the
#   attested tree with that rule (each candidate, then its first-parent ancestors through kanban-only commits).
#
# Design
#   Dot-sourced, defines functions only, so the gate and a unit test
#   (tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs) run the same rule.
#   Get-TreeWithoutKanban lists the tree's top-level entries (`git ls-tree`), drops the one named `kanban`, and
#   writes the rest back with `git mktree`; equal input entries give the same tree id, so comparing the two ids is
#   the whole check. mktree unquotes the names ls-tree quotes, so the round trip keeps every other entry intact.
#   Get-KanbanOnlyDifference returns $null when the rule holds and a printable reason (the first paths that
#   differ outside kanban/) when it does not. That path list uses `git diff --no-renames`, so a move from source/
#   into kanban/ still names the source/ path (same reason as ci-artifact-service.cs); the accept or reject decision
#   is the stripped tree ids alone. Every git call runs in the current directory.
#   why: task 162, PR #692 missed because the merge tree carried master's land-kanban commits (2026-11-18)
#   why: task 162 review round 1 M1, rename detection hid the source/ side of a move into kanban/ (2026-11-18)
#
#   Find-AttestedTree is the lookup itself (task 163). `ganda task work` runs `dev ci` at open-pr and then pushes
#   kanban-only land-pr commits, so the attested commit sits one or more commits behind the PR head and behind the
#   merge's parent 2. For each candidate the walk tests the candidate, then its first-parent ancestors, with the
#   same rule: a hit is a tree with a note that equals HEAD's tree outside kanban/. A chain stops at the first
#   tree that differs outside kanban/ (an older commit cannot be the attested build of this tree), at a root, or
#   after $MaxAncestors ancestors. A commit already walked ends the chain, and a tree already tested is not
#   tested again, so the PR head reached as both pr-head and parent 2 costs one walk. Reasons keep the miss line
#   readable: one per tested candidate, plus one for how each ancestor chain ended.
#   why: task 163, PR #694 run 44330457502 missed one land-pr commit past the attested commit (2026-11-18)

function Invoke-Git([string[]] $Arguments)
{
  $output = & git @Arguments 2>$null
  if ($LASTEXITCODE -ne 0) { return $null }
  return ($output -join "`n").Trim()
}

function Get-TreeWithoutKanban([string] $Tree)
{
  $entries = & git ls-tree $Tree 2>$null
  if ($LASTEXITCODE -ne 0) { return $null }
  $kept = @($entries | Where-Object { ($_ -split "`t", 2)[1] -cne 'kanban' })
  $stripped = $kept | & git mktree 2>$null
  if ($LASTEXITCODE -ne 0) { return $null }
  return ($stripped -join '').Trim()
}

function Get-KanbanOnlyDifference([string] $Tree, [string] $OtherTree)
{
  $left = Get-TreeWithoutKanban $Tree
  $right = Get-TreeWithoutKanban $OtherTree
  if (-not $left -or -not $right) { return "cannot strip kanban from tree $Tree or $OtherTree" }
  if ($left -ceq $right) { return $null }

  $paths = @(& git diff --name-only --no-renames $Tree $OtherTree 2>$null |
    Where-Object { -not $_.StartsWith('kanban/', [StringComparison]::Ordinal) })
  $shown = ($paths | Select-Object -First 5) -join ', '
  if ($paths.Count -gt 5) { $shown += ", and $($paths.Count - 5) more" }
  return "differs outside kanban/: $shown"
}

# Returns @{ Tree; Label; Note; Reasons }. Tree is $null on a miss and Reasons says why for every chain.
# Each candidate is @{ Label = 'pr-head'; Commit = '<sha>' }.
function Find-AttestedTree([string] $HeadTree, [object[]] $Candidates, [string] $NotesRef, [int] $MaxAncestors = 10)
{
  $verdicts = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
  $walked = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
  $reasons = [System.Collections.Generic.List[string]]::new()
  foreach ($candidate in $Candidates)
  {
    $commit = if ($candidate.Commit) { Invoke-Git @('rev-parse', '--verify', '--quiet', "$($candidate.Commit)^{commit}") } else { $null }
    if (-not $commit) { $reasons.Add("$($candidate.Label) $($candidate.Commit): cannot read its tree"); continue }
    for ($depth = 0; $depth -le $MaxAncestors; $depth++)
    {
      $label = if ($depth -eq 0) { "$($candidate.Label) $commit" } else { "$($candidate.Label) ancestor $depth $commit" }
      if (-not $walked.Add($commit)) { break }
      $tree = Invoke-Git @('rev-parse', '--verify', '--quiet', "$commit^{tree}")
      if (-not $tree) { $reasons.Add("${label}: cannot read its tree"); break }
      if ($verdicts.ContainsKey($tree))
      {
        if ($verdicts[$tree]) { break }
      }
      else
      {
        $note = Invoke-Git @('notes', "--ref=$NotesRef", 'show', $tree)
        $difference = if ($tree -ceq $HeadTree) { $null } else { Get-KanbanOnlyDifference $tree $HeadTree }
        if ($difference)
        {
          $verdicts[$tree] = $difference
          $attested = if ($note) { 'attested tree' } else { 'tree' }
          $reasons.Add("${label}: $attested $tree $difference")
          break
        }
        if ($note) { return @{ Tree = $tree; Label = $label; Note = $note; Reasons = $reasons } }
        $verdicts[$tree] = ''
        if ($depth -eq 0) { $reasons.Add("${label}: no CI note for tree $tree") }
      }
      if ($depth -eq $MaxAncestors)
      {
        $reasons.Add("$($candidate.Label): no CI note within $MaxAncestors kanban-only ancestors")
        break
      }
      $parent = Invoke-Git @('rev-parse', '--verify', '--quiet', "$commit^1")
      if (-not $parent)
      {
        if ($depth -gt 0) { $reasons.Add("$($candidate.Label): no CI note down to the root commit at ancestor $depth") }
        break
      }
      $commit = $parent
    }
  }
  return @{ Tree = $null; Label = $null; Note = $null; Reasons = $reasons }
}
