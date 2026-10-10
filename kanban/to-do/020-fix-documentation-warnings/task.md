# Task 020: Fix Documentation Warnings

## Description

Fix 36 documentation warnings found during production release preparation. These include invalid file links, missing UID references in TOC files, and invalid cross-references in migration documentation.

## Requirements

- Fix all invalid file links in sample documentation
- Resolve missing UID references in TOC files
- Fix invalid cross-references in migration documentation
- Ensure documentation builds without warnings

## Checklist

### Analysis
- [x] Review all documentation warnings
- [x] Categorize warnings by type:
  - [x] Invalid file links
  - [x] Missing UID references
  - [x] Invalid cross-references
- [x] Create list of files needing updates

### Implementation
- [x] Fix invalid file links in sample documentation:
  - [x] Sample00-StateActionHandler
  - [x] Sample01-ReduxDevTools
  - [x] Sample02-ActionTracking
  - [x] Sample03-Routing
- [x] Fix TOC file UID references:
  - [x] Features/toc.yml
  - [x] Topics/toc.yml
  - [x] Main toc.yml
- [x] Fix migration documentation cross-references
- [x] Update any outdated documentation paths

### Verification
- [x] Build documentation
- [x] Verify no warnings remain
- [x] Test all documentation links
- [x] Verify cross-references work

### Documentation
- [x] Update any related documentation guidelines
- [x] Document proper link formats
- [x] Update documentation contribution guide if needed

## Notes

- 36 warnings found during documentation build
- Issues include:
  - Invalid file links in sample documentation
  - Missing UID references in TOC files
  - Invalid cross-references in migration documentation
- Part of production readiness requirements
- No critical errors preventing documentation deployment

## Implementation Notes

- 2026-10-10: `dotnet docfx documentation/docfx.json --disableGitFeatures` on the pre-fix tree reported 23 warnings, 0 errors. Groups:
  - Invalid includes and TOC hrefs after kebab-case renames (`Overview.md`, `Partials/*`, `Topics/`, `ReleaseNotes/`, `Migrations/`, `dev-ops/DevOps.md`)
  - Code snippets still pointed at `Samples/01-ReduxDevTools/Wasm/Sample01Wasm/`
  - File links from conceptual articles to sample and package readmes that were outside the DocFX content set
  - `UidNotFound` for `TimeWarp.State:03-Routing.md` because sample articles were not in the content set
  - Metadata paths `Source/TimeWarp.State/*.csproj` do not exist (`No .NET API project detected`)
- `features/toc.yml` and `topics/toc.yml` `topicUid` values already match article `uid`s. Main `toc.yml` failed on folder `href`s, not on `topicUid`.
- Migration `xref` targets already match migration and release-note `uid`s. The 10-to-11 release blog and the releases partial still used the old published paths `Migrations/Migration10-11.html` and `ReleaseNotes/Release11.0.0.html`; those are now `xref`s.
- Sample articles are included from `docfx.json`. Relative links to `.cs`, `.razor`, and project folders are invalid file links even when the path exists, because those files are not DocFX content. Those tips now use GitHub `blob`/`tree` URLs on `master`. Sample-to-sample links go to `overview.md`. Redux screenshots link to `samples/01-redux-dev-tools/images/`, which is a DocFX resource.
- API metadata stays out of `docfx.json`. Pointing it at the real projects makes DocFX recompile them and report existing `NU1510` / `CS7035` build warnings. That is separate from the documentation link defects. Conceptual `dotnet docfx` is the warning gate.
- Link rules are in `documentation/contributing/overview.md`.

## Session

- Implementer: grok session 01a123c8-fe9f-7f12-8bff-da6ecee02c68 (2026-10-10)
- Review oracle: claude-opus-5-5 (2026-10-10), general reviewer subagent a9d878d886d845b42

## Results

Documentation build is clean. `dotnet docfx documentation/docfx.json --disableGitFeatures` reports 0 warnings and 0 errors.

What changed:

- TOC hrefs use the real folders `topics/`, `release-notes/`, and `migrations/`. `dev-ops/toc.yml` points at `dev-ops.md`.
- Includes in `index.md` and `overview.md` use kebab-case partial paths.
- Sample 00, 01, 02, and 03 articles link to real paths. Sample 02 YAML front matter is closed so its `uid` is registered.
- `docfx.json` includes sample articles plus the Plus and Telemetry readmes, and image resources use the lowercase `images/` and `assets/` folders.
- Migration and release cross-references that still used retired site paths now use `xref`.
- `documentation/contributing/overview.md` records the link forms DocFX accepts.

Files changed: `documentation/docfx.json`, `documentation/toc.yml`, `documentation/dev-ops/toc.yml`, `documentation/index.md`, `documentation/overview.md`, `documentation/contributing/overview.md`, `documentation/partials/summary.md`, `documentation/partials/contributing.md`, `documentation/partials/releases.md`, `documentation/topics/add-redux-dev-tools.md`, `documentation/blogs/2024-02-time-warp-state-11-release.md`, four blips under `documentation/blips/`, `samples/overview.md`, the sample 00/01/02/03 overview articles, and `samples/03-routing/wasm/ai.prompt.md`.

Decision: source files stay on GitHub URLs instead of relative DocFX links. API reference generation is not part of this build.

Test: DocFX build succeeded, 0 warnings, 0 errors. Generated pages exist for topics, release notes, migrations, and the sample articles. `topics/routing.html` resolves the Sample 03 cross-reference (no raw `xref:` left in the page).

### How to validate

**Smoke**

```bash
dotnet tool restore
dotnet docfx documentation/docfx.json --disableGitFeatures --logLevel warning
```

**Expect**

```text
Build succeeded.
    0 warning(s)
    0 error(s)
```

Open `documentation/_site/topics/routing.html` and confirm the Routing Tutorial link targets the Sample 03 article. Open `documentation/_site/migrations/toc.html` and confirm the migration entries are listed. Open `documentation/_site/samples/overview.html` and confirm samples 00–03 link to `00-state-action-handler`, `01-redux-dev-tools`, `02-action-tracking`, and `03-routing`.

**Not in scope:** .NET API reference pages. `docfx.json` does not run metadata, because that recompile reports existing `NU1510` and `CS7035` build warnings.

### Review

- Rounds: 1 · effort 2 · roster: general (Claude Opus 5.5 review oracle + general subagent)
- Final counts: bug 0, suggestion 0, nit 1 (wontfix) · 0 open
- Disposition: **accepted-exceptions** — M1 nit (PascalCase link text in `samples/overview.md`) kept as display titles
- Orchestrator re-ran DocFX: 0 warnings, 0 errors
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`
