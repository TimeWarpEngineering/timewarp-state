# Round 1 — general
**Date:** 2026-10-24
**Scope reviewed:** `3c4d0f5b..HEAD`, product paths (124 files). Kitchen files only read for the claims they make.

## Summary

The branch removes the UTF-8 BOM from 121 tracked files in its own commit. A second commit unquotes `charset` in
`.editorconfig`, adds `mission.heartbeat` to the root `.gitignore`, and pins `TimeWarp.Nuru` and
`TimeWarp.Nuru.DevCli` to 4.0.0-beta.5. This is low risk. The only bytes that change apart from the BOM are in those three config files.

## Verification

- `git diff --numstat 3c4d0f5b..HEAD` on product paths: every file is `1 1` except `.editorconfig` (2/2),
  `.gitignore` (3/0), and `Directory.Packages.props` (3/3). The changed lines in the other 121 files differ only by the
  leading `EF BB BF`.
- No tracked file starts with `EF BB BF` (`git ls-files -z | xargs -0 grep -l $'^\xEF\xBB\xBF'` prints nothing).
- `ganda repo normalize-encoding` (dry-run): scanned 1393, changed 0, skipped binary 19, needs-review 0, errors 0.
- `.editorconfig` has `charset = utf-8` (unquoted) under `[*]` at line 18. The quoted copy under the dev-files glob is
  gone, so no section overrides `[*]` with a quoted value.
- `ganda repo audit` exits 0. Only two warnings remain: `kanban-landed` (098 has no worktree) and `kebab-path-names` (7 paths).
  `editorconfig-charset`, `kanban-porcelain-gitignore` and `nuru` pass.
- Nuru 4.0.0-beta.5 compatibility: `dotnet build scripts/clean.cs` and `dotnet build scripts/build.cs` succeed. Both runfiles use `#:package TimeWarp.Nuru` through CPM, and CI triggers on `scripts/**`. The implementer
  reports `./bin/dev build` and `./bin/dev test` exit 0, and the dev-cli self-install compiled against 4.0.0-beta.5. I did not re-run the
  full 7.5-minute test suite.
- The two commits stay separate as required: `500780f2` (BOM only) and `508cd512` (audit fixes).

## Issues

### Issue 1 — Severity: nit
- File: kanban/in-progress/126-normalize-file-encoding-to-utf-8-without-bom/task.md:56
- Description: Results says `kebab-path-names` "lists the 7 Blazor `*.lib.module.js` / MSBuild `.targets` paths". One of
  the 7 is `kanban/backlog/110-state-plus-a-la-carte-modules-pluggable-action-sets--predefined-state-for-any-site/`
  (a double hyphen in a kanban folder name), which is not a Blazor or MSBuild name. The task allowed the warning to stay
  ("Leave them unless audit `--fix` changes them"), so the outcome is fine. Only the Results description is inaccurate.
- Suggestion: Correct the Results line so it names 6 Blazor/MSBuild paths plus the kanban 110 folder.
- Status: open
