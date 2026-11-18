# Purpose
#   The kanban-only tree rule for scripts/ci-fast-path-gate.ps1: two trees count as the same build input when
#   they are equal once the top-level `kanban` entry is removed from both.
#
# Design
#   Dot-sourced, defines functions only, so the gate and a unit test
#   (tests/timewarp-state-tests/release-pipeline/ci-fast-path-tree-tests.cs) run the same rule.
#   Get-TreeWithoutKanban lists the tree's top-level entries (`git ls-tree`), drops the one named `kanban`, and
#   writes the rest back with `git mktree`; equal input entries give the same tree id, so comparing the two ids is
#   the whole check. mktree unquotes the names ls-tree quotes, so the round trip keeps every other entry intact.
#   Get-KanbanOnlyDifference returns $null when the rule holds and a printable reason (the first paths that
#   differ outside kanban/) when it does not. Every git call runs in the current directory.
#   why: task 162, PR #692 missed because the merge tree carried master's land-kanban commits (2026-11-18)

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

  $paths = @(& git diff --name-only $Tree $OtherTree 2>$null |
    Where-Object { -not $_.StartsWith('kanban/', [StringComparison]::Ordinal) })
  $shown = ($paths | Select-Object -First 5) -join ', '
  if ($paths.Count -gt 5) { $shown += ", and $($paths.Count - 5) more" }
  return "differs outside kanban/: $shown"
}
