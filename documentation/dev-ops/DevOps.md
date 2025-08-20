# DevOps

## Production Artifacts

* TimeWarp.State Nuget Package on Nuget.org
* TimeWarp.State.Plus Nuget Package on Nuget.org
* TimeWarp.State.Analyzer Nuget Package on Nuget.org
* TimeWarp.State.SourceGenerator Nuget Package on Nuget.org
* TimeWarp.State.Policies Nuget Package on Nuget.org
* [Documentation Site](https://timewarpengineering.github.io/timewarp-state/)

## Release Process

NuGet packages are published automatically when a GitHub Release is created:

1. **Master Branch**: Builds and tests on every push, but does not publish packages
2. **Release Creation**: When a GitHub Release is created, the release workflow validates the version and publishes all NuGet packages

### Creating a Release

1. Ensure the version in `Directory.Build.props` matches the intended release version
2. Create a GitHub Release with a tag that exactly matches the version number
3. The release workflow will automatically validate the version and publish packages to NuGet.org

### CI/CD Workflows

* **master-build.yml**: Runs on master branch pushes - builds, tests, but no publishing
* **ci-build.yml**: Runs on pull requests - builds and tests
* **release-publish.yml**: Runs on GitHub release creation - publishes NuGet packages
