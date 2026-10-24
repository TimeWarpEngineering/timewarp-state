# Disposition — task 126

**Date:** 2026-10-24
**Outcome:** clean
**Rounds:** 1
**Final open count:** 0

## Summary

The implementation matches the task and the restatement. Apart from three files, the BOM strip is byte-for-byte BOM-only across 121 files, no tracked file still has a BOM, and a second normalize run changes nothing. The three files are `.editorconfig` (unquoted `charset = utf-8` under `[*]`), `.gitignore` (adds `mission.heartbeat`) and `Directory.Packages.props` (Nuru and Nuru.DevCli at 4.0.0-beta.5). `ganda repo audit` exits 0 with warnings only. The runfile scripts compile against Nuru 4.0.0-beta.5. The encoding commit and the audit-fix commit are separate. The one nit (M1, a Results wording error) was fixed in the kitchen.

## Exception log (if accepted-exceptions)

None.

## Escalations

- None.
