## Source code management

:grey_exclamation: The *structure* of the code that we write and the *tools* that we use to write the code.


### Repos

To create a new repo in the https://github.com/aspnet/ org, contact @eilon.


### Universe repo

The https://github.com/aspnet/Universe repo is the "one repo to rule them all." It is a special repo that has a build that will pull in and build projects from all the other repos. This is great to have in your enlistment if you're making cross-repo changes, such as renaming a core API. https://github.com/aspnet/Universe/blob/master/docs/CrossRepoBreakingChanges.md has some instructions for making cross-repo breaking changes


### Build numbers

By default, builds of ASP.NET Core projects on a local developer machine will produce the same version every time you execute the `build.ps1` or `build.sh` scripts. e.g. `2.1.0-preview1-t000`.

To change the build number, you can either:

1. Pass in the build number explicitly 
    ```
    ./build.ps1 /p:BuildNumber=1234
    ```
2. Set an environment variable 
    ```
    $env:BUILD_NUMBER='1234'
    ./build.ps1
    ```
3. Set "IncrementalVersion" to produce a time-based build number each time the build is produced.
    ```
    ./build.ps1 /p:IncrementalVersion=true
    ```

### Branch strategy

In general:

* `master` has the code that is being worked on but not yet released. This is the branch into which developers normally submit pull requests and merge changes into.
  - Currently `master` contains work for the 3.0 release.
* `release/<major>.<minor>` contains code that is intended for release. The `<major>.<minor>` part of the branch name indicates the beginning of the version of the product the code will end up in. The branch may be used for several patch releases with the same major.minor number. It is common for repos to have multiple `release/*` branches, each which may receive servicing updates.
  - External contributors do not normally need to make pull requests to these branches. However, when multiple releases are in simultaneous development, targeting a release branch may be preferred. At the moment, the 2.2.0 release has not gone out and developers should use the `release/2.2` for most changes -- all but those intended only for 3.0.

#### Making changes to release branches

If you make a change to a `release/*` branch directly, you typically also need to merge those changes back to `master`. This often causes some merge conflicts, so make sure to proceed carefully. If you're not sure how to do this, follow these steps.

The following is not currently necessary to get `release/2.2` changes into `master`. That specific process is automated.

##### Manual merges to master

```sh
# Make your changes to release/x.y
git checkout release/x.y    
git merge --ff-only your-name/my-approved-PR-branch
git push

# Make sure master is up to date first
git checkout master
git pull 

# Prepare your merge on a separate branch

git checkout --branch your-name/merge-release-x-y-to-master
git merge --no-commit release/x.y

# This is where you need to manually check your changes are good

git commit -m "Merge branch release/x.y into master"
git push -u origin your-name/merge-release-x-y-to-master

# Open a PR to review the changes

# Once approved
git checkout master
git merge --ff-only your-name/merge-release-x-y-to-master
git push
```

### Solution and project folder structure and naming

Solution files go in the repo root.

Solution names match repo names (e.g. Mvc.sln in the Mvc repo).

Solutions need to contain solution folders that match the physical folders (`src`, `test`, etc.).

For example, in the `Fruit` repo with the `Banana` and `Lychee` projects you would have these files checked in:

```
/Fruit.sln
/src/Microsoft.AspNet.Banana/Banana.csproj
/src/Microsoft.AspNet.Banana/Banana.cs
/src/Microsoft.AspNet.Banana/Util/BananaUtil.cs
/src/Microsoft.AspNet.Lychee/Lychee.csproj
/src/Microsoft.AspNet.Lychee/Lychee.cs
/src/Microsoft.AspNet.Lychee/Util/LycheeUtil.cs
/test/Microsoft.AspNet.Banana.Tests/BananaTest.csproj
/test/Microsoft.AspNet.Banana.Tests/BananaTest.cs
/test/Microsoft.AspNet.Banana.Tests/Util/BananaUtilTest.cs
```


### Conditional compilation for multiple Target Frameworks

Code sometimes has to be written to be target framework-specific due to API changes between frameworks. Use `#if` statements to create these conditional code segments:
Desktop:

```c#
#if NET46
            Console.WriteLine("Hi .NET 4.6");
#elif NETCOREAPP2_0
            Console.WriteLine("Hi .NET Standard 2.0");
#else
#error Target framework needs to be updated
#endif
```

Note the `#error` section that is present in case the target frameworks change - this ensure that we don't have dead code in the projects, and also no missing conditions.


### Assembly naming pattern

The general naming pattern is `Microsoft.AspNetCore.<area>.<subarea>`, `Microsoft.EntityFrameworkCore.<area>.<subarea>`, and `Microsoft.Extensions.<area>.<subarea>`.


### Unit tests

We use xUnit.net for all unit testing.


### Repo-specific Samples

Some repos will have their own sample projects that are used for testing purposes and experimentation. Please ensure that these go in a `samples/` sub-folder in the repo.


TODO: Include Coding GuideLines here
