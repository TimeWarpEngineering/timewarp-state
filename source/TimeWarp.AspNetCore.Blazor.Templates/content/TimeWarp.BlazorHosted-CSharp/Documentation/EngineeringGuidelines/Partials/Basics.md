## Basics

### External dependencies

This refers to dependencies on projects (i.e. NuGet packages) outside of the `aspnet` repo, and especially outside of Microsoft. Because these repos are a core part of all ASP.NET/EF apps, it is important that we be careful and manage our dependencies properly.

*Adding* or *updating* any external dependency requires approval from @eilon and @damianedwards. After you have gotten approval you must also update [this](https://github.com/aspnet/Universe/blob/master/build/external-dependencies.props) file to contain the dependency you're adding.

Dependencies that are used only in test projects and build tools are not nearly as rigid, but still require approval from @eilon.

There is also an OSS-approval process that has to be followed (to be done by @eilon).

### Code reviews and checkins

To help ensure that only the highest quality code makes its way into the project, please submit all your code changes to GitHub as PRs. This includes runtime code changes, unit test updates, and updates to official samples (e.g. Music Store). For example, sending a PR for just an update to a unit test might seem like a waste of time but the unit tests are just as important as the product code and as such, reviewing changes to them is also just as important. This also helps create visibility for your changes so that others can observe what is going on.

The advantages are numerous: improving code quality, more visibility on changes and their potential impact, avoiding duplication of effort, and creating general awareness of progress being made in various areas.

In general a PR should be signed off (using GitHub's "approve" feature) by the Subject Matter Expert (SME) of that code. For example, a change to the Banana project should be signed off by `@MrMonkey`, and not by `@MrsGiraffe`. If you don't know the SME, please talk to one of the engineering leads and they will be happy to help you identify the SME. Of course, sometimes it's the SME who is making a change, in which case a secondary person will have to sign off on the change (e.g. `@JuniorMonkey`).

To commit the PR to the repo either use GitHub's "Squash and Merge" button on the main PR page, or do a typical push that you would use with Git (e.g. local pull, rebase, merge, push).

