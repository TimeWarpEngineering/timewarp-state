# Gitignore the Aspire CLI aspire.config.json written next to AppHosts

## Description

Running an AppHost with the Aspire CLI (`aspire run`, or `dotnet run` under Aspire 13.6.1, which
prints `💾 Created settings file at 'aspire.config.json'`) writes
`samples/04-telemetry/apphost/aspire.config.json`:

```json
{ "appHost": { "path": "sample-04-apphost.csproj" } }
```

It is a per-checkout convenience file, not repo content, and it is not ignored. On 2026-10-12 it was
the only dirt that made `dev release --dry-run` refuse with "working tree is dirty" on master right
before cutting 12.0.0-beta.11; it had to be deleted by hand.

Filed by Claude (cockpit) at Steven's request, 2026-10-12.

## Requirements

- Add `aspire.config.json` to the root `.gitignore` (any directory, so future AppHosts are covered).
- Confirm no copy is tracked (`git ls-files | grep aspire.config.json` is empty) and that
  `ganda repo audit` has no rule that wants the file.
- One line in `samples/04-telemetry/overview.md` (run section) noting the CLI writes it and it is ignored.

## Checklist

- [ ] `.gitignore` entry
- [ ] Nothing tracked; audit clean
- [ ] Sample 04 overview note

## Session

- Created: cockpit (2026-10-12)

## Notes

- Related: 111 (first AppHost run on Linux), 113 (second run), release 12.0.0-beta.11.
