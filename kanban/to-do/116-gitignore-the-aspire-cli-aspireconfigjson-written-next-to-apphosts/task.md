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

- [x] `.gitignore` entry
- [x] Nothing tracked; audit clean
- [x] Sample 04 overview note

## Session

- Created: cockpit (2026-10-12)
- Reviewer: claude opus 5.5 review oracle, effort 1 general (2026-10-12)
- Implementer: grok session `01a12f80-c87d-7531-96b8-83f61622dda6` (2026-10-12)

## Results

Root `.gitignore` ignores `aspire.config.json` in any directory. The Aspire CLI writes that file beside an AppHost (`aspire run`, or `dotnet run` under Aspire 13.6+). It is per-checkout settings, and a copy next to sample 04 was enough to make `dev release --dry-run` refuse on a dirty tree.

**Files changed**

- `.gitignore` — unanchored `aspire.config.json` (matches every directory)
- `samples/04-telemetry/overview.md` — one line in the AppHost run section: the CLI writes the file and it is gitignored
- `kanban/to-do/116-gitignore-the-aspire-cli-aspireconfigjson-written-next-to-apphosts/task.md` — folderized kitchen, checklist, session, results

**Key decisions / deviations**

- Pattern is the bare filename, so a future AppHost is covered without a path prefix.
- `git ls-files` has no `aspire.config.json`, so nothing was removed from the index.
- No audit check asks for the file to be tracked. `required-gitignore` still passes with the new line.

**Test outcomes**

- `git check-ignore -v samples/04-telemetry/apphost/aspire.config.json` → `.gitignore:351:aspire.config.json`
- `git check-ignore -v samples/99-future/apphost/aspire.config.json` → same pattern (any directory)
- A real `samples/04-telemetry/apphost/aspire.config.json` does not appear in `git status --porcelain` (probe file removed after the check)
- `git ls-files | grep aspire.config.json` → empty
- `ganda repo audit` → exit 0. Passed 29, skipped 1, one pre-existing advisory warning (`kebab-path-names`, six paths already on master). `required-gitignore` PASS.

### How to validate

**Smoke**

```bash
git check-ignore -v samples/04-telemetry/apphost/aspire.config.json
git check-ignore -v samples/99-future/apphost/aspire.config.json
git ls-files | grep aspire.config.json || true
printf '%s\n' '{ "appHost": { "path": "sample-04-apphost.csproj" } }' > samples/04-telemetry/apphost/aspire.config.json
git status --porcelain -- samples/04-telemetry/apphost/aspire.config.json
rm -f samples/04-telemetry/apphost/aspire.config.json
ganda repo audit
```

**Expect**

- Both `git check-ignore -v` lines name root `.gitignore` and the pattern `aspire.config.json`
- `git ls-files | grep aspire.config.json` prints nothing
- `git status --porcelain` for the written file prints nothing
- `ganda repo audit` exits 0. `required-gitignore` PASS. No check asks for `aspire.config.json` to be committed
- `samples/04-telemetry/overview.md` AppHost run section says the CLI writes `aspire.config.json` and that it is gitignored

**Automated gate**

```bash
ganda repo audit
```

**Not in scope:** packing and launching the sample 04 AppHost. The ignore rule is what keeps that run from dirtying the tree.

### Review

- Rounds: 1. Effort 1, roster: general
- Final counts: bug 0, suggestion 0, nit 0 (0 open, 0 fixed, 0 wontfix)
- Disposition: **clean**
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`

## Notes

- Related: 111 (first AppHost run on Linux), 113 (second run), release 12.0.0-beta.11.
