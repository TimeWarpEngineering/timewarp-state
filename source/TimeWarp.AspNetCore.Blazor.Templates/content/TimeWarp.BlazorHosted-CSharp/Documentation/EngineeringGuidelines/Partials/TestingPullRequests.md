## Testing PRs

Though it's expected that you run tests locally for your PR, tests will also run on the Jenkins server automatically on any PR you send to GitHub. (As of 3/20/2018, not all repos are yet enabled.)

You should see Jenkins builds show up on GitHub (alongside Travis and AppVeyor) for now) for all pull requests, including external contributors. Additionally, here's some commands to interact with the build-bot:
 
* `@dotnet-bot test this please`
  * Re-runs build on all configurations (Windows, OSX, Ubuntu 16.04)
* `@dotnet-bot test <configuration> please` -> e.g. `@dotnet-bot test Windows Release x64 Build`
  * Re-runs build on the specific configuration

More info on how the dotnet-ci build infrastructure works: https://github.com/dotnet/dotnet-ci/tree/master/docs
