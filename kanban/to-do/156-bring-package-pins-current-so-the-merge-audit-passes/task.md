# Bring package pins current so the merge audit passes

## Description

`ganda pr merge` refuses every PR in this repo because the `package-pin-currency` audit check
fails on master. PR #685 (task 154) had to be merged with `--skip-audit` on 2026-11-16 on
Steven's instruction, with this update to follow.

Blocking (2 of 13 pins fail):

- `TimeWarp.Mediator` 15.0.0-beta.5 → latest 15.0.0-beta.9 (library reference in
  `source/timewarp-state-devtools/timewarp-state-devtools.csproj`, no `PrivateAssets=all`)
- `TimeWarp.Mediator.Generators` 15.0.0-beta.5 → latest 15.0.0-beta.9

Behind but not yet blocking (reported as current at the last meaningful commit, 2026-11-11):

- `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` 3.0.0-beta.2 → 3.0.0-beta.5
- `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` 4.0.0-beta.12 → 4.0.0-beta.14
- `TimeWarp.SourceGenerators` 1.0.0-beta.13 → 1.0.0-beta.16
- `TimeWarp.Terminal` 2.0.0-beta.1 → 2.0.0-beta.2
- `TimeWarp.Build.Tasks` 1.0.0 → 2.0.1-beta.1 (major bump)

Any commit to this repo makes the "last meaningful commit" newer, so the non-blocking pins are
likely to start failing as soon as this task's own PR is audited. Bring them all current here.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes (audit fixes are policy).
2. Bump both Mediator pins in `Directory.Packages.props` to the latest release and fix whatever
   breaks. Four beta versions is enough for API or generator changes; known Mediator generator
   gotchas from the 14 line: `order:` is a constructor argument, the pipeline delegate parameter
   must be named `next`, and request types must be public. Check the Mediator release notes for
   beta.6 through beta.9 before guessing.
3. Bump the remaining behind pins listed above. Amuru's exit-code behavior has changed across
   betas before (RunAsync returning codes vs throwing); verify the dev CLI and scripts still
   behave, do not trust old comments.
4. `TimeWarp.Build.Tasks` 1.0.0 → 2.0.1-beta.1 is a major bump. Attempt it; if it needs more than
   mechanical changes, leave it at 1.0.0, say so in Results, and do not exempt it silently.
5. TimeWarp packages come from https://nuget.pkg.github.com/TimeWarpEngineering/index.json as well
   as nuget.org; use whichever feed the audit names for each package.
6. Gates: `./bin/dev build` with 0 warnings and 0 errors; unit tests green; `ganda repo audit`
   with `package-pin-currency` passing. If `./bin/dev test` reports fixie needs a tool restore,
   that is the stale `~/.dotnet/toolResolverCache`; say so rather than working around it.
7. `--no-incremental` builds delete tracked `wwwroot` JS in `timewarp-state-blazor`; rebuild to
   regenerate and check `git status` before finishing.
8. Bump the State package version if a packed dependency changed (Mediator is a packed dependency
   of State; `dev check-version` must pass).

## Checklist

- [ ] `ganda repo audit --fix` run and committed
- [ ] Mediator + Mediator.Generators at latest; code compiles and tests pass
- [ ] Amuru, Nuru, SourceGenerators, Terminal pins at latest
- [ ] Build.Tasks major bump done, or reported as deferred with the reason
- [ ] State version bumped if a packed dependency changed
- [ ] Build clean, tests green, tracked wwwroot JS intact
- [ ] `ganda repo audit`: `package-pin-currency` passes

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-16)

## Notes

Requested by Steven 2026-11-16: "skip audit and then we update".
