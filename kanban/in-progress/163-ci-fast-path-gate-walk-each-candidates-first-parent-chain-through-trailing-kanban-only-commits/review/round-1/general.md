# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/163-ci-fast-path-gate-walk-each-candidates-first` vs merge-base `1602f7c4`, implementation `a07b0a90`. Gate, tree helper, workflow comment, `documentation/dev-ops/local-ci.md`, and `ci-fast-path-tree-tests.cs`, checked against the task Goal and the implementer restatement.

## Summary

`Find-AttestedTree` in `scripts/ci-fast-path-tree.ps1` tests each candidate, then up to ten first-parent ancestors, with the same note and kanban-only rule as task 162. A chain stops on a hit labeled `<candidate> ancestor K <sha>`, on the first tree that differs outside `kanban/`, at a root, or at the bound. Commits already walked end a later chain, and a tree already judged is not judged again. The gate's later stage, SDK, RID, and package checks are unchanged. The four new tests cover a hit two kanban-only commits back, the non-kanban stop (including an older matching note that must not be reached), the 10-ancestor bound (hit at 10, miss at 11), and dedupe when two candidates are the same commit. Recorded results cover `./bin/dev build` (0 warnings), the touched Fixie class (8 passed), the note-input class (6 passed), and `ganda repo audit`. The restatement matches the task. No misread.

## Issues

<!-- None. -->
