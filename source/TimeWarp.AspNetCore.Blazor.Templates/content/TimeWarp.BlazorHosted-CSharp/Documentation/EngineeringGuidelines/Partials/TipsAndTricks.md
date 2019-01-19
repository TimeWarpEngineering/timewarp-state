
## Tips and tricks

:grey_exclamation: The *structure* of the code that we write and the *tools* that we use to write the code.


### I've broken my build and I can't get up!

The build system is brand new, so problems can catch us by surprise. As such, you'll sometimes end up in a broken state and can't build. The following steps should fix most broken builds:

```
git clean -xdf (clean all non-source controlled files)
build (this will run the build and will pull in NuGet packages, etc.)
```


### GitHub Flavored Markdown

GitHub supports Markdown in many places throughout the system (issues, comments, etc.). However, there are a few differences from regular Markdown that are described here:

	https://help.github.com/articles/github-flavored-markdown

Break long lines at no more than 80 columns.
Markdown will not render a carriage return unless you add 
two spaces to the end of the line.
Using this method makes pull requests on documentation much
easier to determine the actual difference.


### Including people in a GitHub discussion

To include another team member in a discussion on GitHub you can use an `@ mention` to cause a notification to be sent to that person. This will automatically send a notification email to that person (assuming they have not altered their GitHub account settings). For example, in a PR's discussion thread or in an issue tracker comment you can type `@username` to have them receive a notification. This is useful when you want to "include" someone in a code review in a PR, or if you want to get another opinion on an issue in the issue tracker.
