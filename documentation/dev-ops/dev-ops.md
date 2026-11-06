# DevOps

## Production Artifacts

* TimeWarp.State Nuget Package on Nuget.org
* TimeWarp.State.Plus Nuget Package on Nuget.org
* TimeWarp.State.Analyzer Nuget Package on Nuget.org
* TimeWarp.State.SourceGenerator Nuget Package on Nuget.org
* TimeWarp.State.Policies Nuget Package on Nuget.org
* [Documentation Site](https://timewarpengineering.github.io/timewarp-state/)

## Release Process

Packages are published when a GitHub Release is created. `dev release` creates the tag and the release from `<Version>` in `source/Directory.Build.props`. The release run promotes the CI artifact of the tagged commit; it does not rebuild. [Releasing](releasing.md) has the steps, the kanban-only commit rule, and the fixes for each refusal.

### CI/CD Workflow

`.github/workflows/workflow.yml` is a thin trigger for `dev workflow`:

* **pull_request** and **push to master**: build, test, e2e, and verify samples, then upload the `Packages-*` artifact
* **release: published**: promote that artifact to NuGet.org
* **workflow_dispatch**: `merge` (build and test), `release` (break-glass publish; needs `confirm=release`), or `probe` (trusted-publishing check)
